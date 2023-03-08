//
// Copyright (c) 2009-2012 Krueger Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#if WINDOWS_PHONE && !USE_WP8_NATIVE_SQLITE
#define USE_CSHARP_SQLITE
#endif

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

#if USE_CSHARP_SQLITE
using Sqlite3 = Community.CsharpSqlite.Sqlite3;
using Sqlite3DatabaseHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite3Statement = Community.CsharpSqlite.Sqlite3.Vdbe;
#elif USE_WP8_NATIVE_SQLITE
using Sqlite3 = Sqlite.Sqlite3;
using Sqlite3DatabaseHandle = Sqlite.Database;
using Sqlite3Statement = Sqlite.Statement;
#else
using Sqlite3DatabaseHandle = System.IntPtr;
using Sqlite3Statement = System.IntPtr;
#endif

namespace SQLite4Unity3d
{
	public class SQLiteException : Exception
	{
		public SQLite3.Result Result { get; private set; }

		protected SQLiteException (SQLite3.Result r,string message) : base(message)
		{
			Result = r;
		}

		public static SQLiteException New (SQLite3.Result r, string message)
		{
			return new SQLiteException (r, message);
		}
	}

	public class NotNullConstraintViolationException : SQLiteException
	{
		public IEnumerable<TableMapping.Column> Columns { get; protected set; }

		protected NotNullConstraintViolationException (SQLite3.Result r, string message)
			: this (r, message, null, null)
		{

		}

		protected NotNullConstraintViolationException (SQLite3.Result r, string message, TableMapping mapping, object obj)
			: base (r, message)
		{
			if (mapping != null && obj != null) {
				this.Columns = from c in mapping.Columns
							   where c.IsNullable == false && c.GetValue(obj) == null
							   select c;
			}
		}

		public static new NotNullConstraintViolationException New (SQLite3.Result r, string message)
		{
			return new NotNullConstraintViolationException (r, message);
		}

		public static NotNullConstraintViolationException New (SQLite3.Result r, string message, TableMapping mapping, object obj)
		{
			return new NotNullConstraintViolationException (r, message, mapping, obj);
		}

		public static NotNullConstraintViolationException New (SQLiteException exception, TableMapping mapping, object obj)
		{
			return new NotNullConstraintViolationException (exception.Result, exception.Message, mapping, obj);
		}
	}

	[Flags]
	public enum SQLiteOpenFlags {
		ReadOnly = 1, ReadWrite = 2, Create = 4,
		NoMutex = 0x8000, FullMutex = 0x10000,
		SharedCache = 0x20000, PrivateCache = 0x40000,
		ProtectionComplete = 0x00100000,
		ProtectionCompleteUnlessOpen = 0x00200000,
		ProtectionCompleteUntilFirstUserAuthentication = 0x00300000,
		ProtectionNone = 0x00400000
	}

    [Flags]
    public enum CreateFlags
    {
        None = 0,
        ImplicitPK = 1,    // create a primary key for field called 'Id' (Orm.ImplicitPkName)
        ImplicitIndex = 2, // create an index for fields ending in 'Id' (Orm.ImplicitIndexSuffix)
        AllImplicit = 3,   // do both above

        AutoIncPK = 4      // force PK field to be auto inc
    }

	/// <summary>
	/// Represents an open connection to a SQLite database.
	/// </summary>
	public partial class SQLiteConnection : ISQLiteConnection
    {
		private bool _open;
		private TimeSpan _busyTimeout;
		private Dictionary<string, TableMapping> _mappings = null;
		private Dictionary<string, TableMapping> _tables = null;
		private System.Diagnostics.Stopwatch _sw;
		private long _elapsedMilliseconds = 0;

		private int _transactionDepth = 0;

		public Sqlite3DatabaseHandle Handle { get; private set; }
		internal static readonly Sqlite3DatabaseHandle NullHandle = default(Sqlite3DatabaseHandle);

		public string DatabasePath { get; private set; }

		public bool TimeExecution { get; set; }

		#region debug tracing

		public bool Trace { get; set; }

		public delegate void TraceHandler (string message);
		public event TraceHandler TraceEvent;

		internal void InvokeTrace (string message)
		{
			if (TraceEvent != null) {
				TraceEvent(message);
			}
		}

		#endregion

		public bool StoreDateTimeAsTicks { get; private set; }

        /// Provide a way to set or change database encryption key
        /// </summary>
        /// <param name="key"></param>
        public void SetDbKey(string key)
        {
            var r = SQLite3.Rekey(Handle, key, key.Length);
            if (r != SQLite3.Result.OK)
            {
                string msg = SQLite3.GetErrmsg(Handle);
                throw SQLiteException.New(r, msg);
            }
        }

        /// <summary>
        /// Provide a way to decrypt your database
        /// </summary>
        /// <param name="key"></param>
        public void Key(string key)
        {
            var r = SQLite3.Key(Handle, key, key.Length);
            if (r != SQLite3.Result.OK)
            {
                string msg = SQLite3.GetErrmsg(Handle);
                throw SQLiteException.New(r, msg);
            }
        }

        /// <summary>
        /// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
        /// </summary>
        /// <param name="databasePath">
        /// Specifies the path to the database file.
        /// </param>
        /// <param name="storeDateTimeAsTicks">
        /// Specifies whether to store DateTime properties as ticks (true) or strings (false). You
        /// absolutely do want to store them as Ticks in all new projects. The default of false is
        /// only here for backwards compatibility. There is a *significant* speed advantage, with no
        /// down sides, when setting storeDateTimeAsTicks = true.
        /// </param>
        public SQLiteConnection (string databasePath, bool storeDateTimeAsTicks = false)
			: this (databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, storeDateTimeAsTicks)
		{
		}

		/// <summary>
		/// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
		/// </summary>
		/// <param name="databasePath">
		/// Specifies the path to the database file.
		/// </param>
		/// <param name="storeDateTimeAsTicks">
		/// Specifies whether to store DateTime properties as ticks (true) or strings (false). You
		/// absolutely do want to store them as Ticks in all new projects. The default of false is
		/// only here for backwards compatibility. There is a *significant* speed advantage, with no
		/// down sides, when setting storeDateTimeAsTicks = true.
		/// </param>
		public SQLiteConnection (string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = false)
		{
			if (string.IsNullOrEmpty (databasePath))
				throw new ArgumentException ("Must be specified", "databasePath");

			DatabasePath = databasePath;

#if NETFX_CORE
			SQLite3.SetDirectory(/*temp directory type*/2, Windows.Storage.ApplicationData.Current.TemporaryFolder.Path);
#endif

			Sqlite3DatabaseHandle handle;

#if SILVERLIGHT || USE_CSHARP_SQLITE
            var r = SQLite3.Open (databasePath, out handle, (int)openFlags, IntPtr.Zero);
#else
			// open using the byte[]
			// in the case where the path may include Unicode
			// force open to using UTF-8 using sqlite3_open_v2
			var databasePathAsBytes = GetNullTerminatedUtf8 (DatabasePath);
			var r = SQLite3.Open (databasePathAsBytes, out handle, (int) openFlags, IntPtr.Zero);
#endif

			Handle = handle;
			if (r != SQLite3.Result.OK) {
				throw SQLiteException.New (r, String.Format ("Could not open database file: {0} ({1})", DatabasePath, r));
			}
			_open = true;

			StoreDateTimeAsTicks = storeDateTimeAsTicks;
			
			BusyTimeout = TimeSpan.FromSeconds (0.1);
		}
		
		static SQLiteConnection ()
		{
		}

        public void EnableLoadExtension(int onoff)
        {
            SQLite3.Result r = SQLite3.EnableLoadExtension(Handle, onoff);
			if (r != SQLite3.Result.OK) {
				string msg = SQLite3.GetErrmsg (Handle);
				throw SQLiteException.New (r, msg);
			}
        }

		static byte[] GetNullTerminatedUtf8 (string s)
		{
			var utf8Length = System.Text.Encoding.UTF8.GetByteCount (s);
			var bytes = new byte [utf8Length + 1];
			utf8Length = System.Text.Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
			return bytes;
		}
		

		/// <summary>
		/// Sets a busy handler to sleep the specified amount of time when a table is locked.
		/// The handler will sleep multiple times until a total time of <see cref="BusyTimeout"/> has accumulated.
		/// </summary>
		public TimeSpan BusyTimeout {
			get { return _busyTimeout; }
			set {
				_busyTimeout = value;
				if (Handle != NullHandle) {
					SQLite3.BusyTimeout (Handle, (int)_busyTimeout.TotalMilliseconds);
				}
			}
		}

		/// <summary>
		/// Returns the mappings from types to tables that the connection
		/// currently understands.
		/// </summary>
		public IEnumerable<TableMapping> TableMappings {
			get {
				return _tables != null ? _tables.Values : Enumerable.Empty<TableMapping> ();
			}
		}

		/// <summary>
		/// Retrieves the mapping that is automatically generated for the given type.
		/// </summary>
		/// <param name="type">
		/// The type whose mapping to the database is returned.
		/// </param>         
        /// <param name="createFlags">
		/// Optional flags allowing implicit PK and indexes based on naming conventions
		/// </param>     
		/// <returns>
		/// The mapping represents the schema of the columns of the database and contains 
		/// methods to set and get properties of objects.
		/// </returns>
        public TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
		{
			if (_mappings == null) {
				_mappings = new Dictionary<string, TableMapping> ();
			}
			TableMapping map;
			if (!_mappings.TryGetValue (type.FullName, out map)) {
				map = new TableMapping (type, createFlags);
				_mappings [type.FullName] = map;
			}
			return map;
		}
		
		/// <summary>
		/// Retrieves the mapping that is automatically generated for the given type.
		/// </summary>
		/// <returns>
		/// The mapping represents the schema of the columns of the database and contains 
		/// methods to set and get properties of objects.
		/// </returns>
		public TableMapping GetMapping<T> ()
		{
			return GetMapping (typeof (T));
		}

		private struct IndexedColumn
		{
			public int Order;
			public string ColumnName;
		}

		private struct IndexInfo
		{
			public string IndexName;
			public string TableName;
			public bool Unique;
			public List<IndexedColumn> Columns;
		}

		public class ColumnInfo
		{
//			public int cid { get; set; }

			[Column ("name")]
			public string Name { get; set; }

//			[Column ("type")]
//			public string ColumnType { get; set; }

			public int notnull { get; set; }

//			public string dflt_value { get; set; }

//			public int pk { get; set; }

			public override string ToString ()
			{
				return Name;
			}
		}

		/// <summary>
		/// Creates a new SQLiteCommand. Can be overridden to provide a sub-class.
		/// </summary>
		/// <seealso cref="SQLiteCommand.OnInstanceCreated"/>
		protected virtual SQLiteCommand NewCommand ()
		{
			return new SQLiteCommand (this);
		}

		/// <summary>
		/// Creates a new SQLiteCommand given the command text with arguments. Place a '?'
		/// in the command text for each of the arguments.
		/// </summary>
		/// <param name="cmdText">
		/// The fully escaped SQL.
		/// </param>
		/// <param name="args">
		/// Arguments to substitute for the occurences of '?' in the command text.
		/// </param>
		/// <returns>
		/// A <see cref="SQLiteCommand"/>
		/// </returns>
		public SQLiteCommand CreateCommand (string cmdText, params object[] ps)
		{
			if (!_open)
				throw SQLiteException.New (SQLite3.Result.Error, "Cannot create commands from unopened database");

			var cmd = NewCommand ();
			cmd.CommandText = cmdText;
			foreach (var o in ps) {
				cmd.Bind (o);
			}
			return cmd;
		}

		/// <summary>
		/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
		/// in the command text for each of the arguments and then executes that command.
		/// Use this method instead of Query when you don't expect rows back. Such cases include
		/// INSERTs, UPDATEs, and DELETEs.
		/// You can set the Trace or TimeExecution properties of the connection
		/// to profile execution.
		/// </summary>
		/// <param name="query">
		/// The fully escaped SQL.
		/// </param>
		/// <param name="args">
		/// Arguments to substitute for the occurences of '?' in the query.
		/// </param>
		/// <returns>
		/// The number of rows modified in the database as a result of this execution.
		/// </returns>
		public int Execute (string query, params object[] args)
		{
			var cmd = CreateCommand (query, args);
			
			if (TimeExecution) {
				if (_sw == null) {
					_sw = new Stopwatch ();
				}
				_sw.Reset ();
				_sw.Start ();
			}

			var r = cmd.ExecuteNonQuery ();
			
			if (TimeExecution) {
				_sw.Stop ();
				_elapsedMilliseconds += _sw.ElapsedMilliseconds;
				System.Diagnostics.Debug.WriteLine (string.Format ("Finished in {0} ms ({1:0.0} s total)", _sw.ElapsedMilliseconds, _elapsedMilliseconds / 1000.0));
			}
			
			return r;
		}

		public T ExecuteScalar<T> (string query, params object[] args)
		{
			var cmd = CreateCommand (query, args);
			
			if (TimeExecution) {
				if (_sw == null) {
					_sw = new Stopwatch ();
				}
				_sw.Reset ();
				_sw.Start ();
			}
			
			var r = cmd.ExecuteScalar<T> ();
			
			if (TimeExecution) {
				_sw.Stop ();
				_elapsedMilliseconds += _sw.ElapsedMilliseconds;
				System.Diagnostics.Debug.WriteLine (string.Format ("Finished in {0} ms ({1:0.0} s total)", _sw.ElapsedMilliseconds, _elapsedMilliseconds / 1000.0));
			}
			
			return r;
		}

		/// <summary>
		/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
		/// in the command text for each of the arguments and then executes that command.
		/// It returns each row of the result using the mapping automatically generated for
		/// the given type.
		/// </summary>
		/// <param name="query">
		/// The fully escaped SQL.
		/// </param>
		/// <param name="args">
		/// Arguments to substitute for the occurences of '?' in the query.
		/// </param>
		/// <returns>
		/// An enumerable with one result for each row returned by the query.
		/// </returns>
		public List<Dictionary<string, string>> Query (string query)
		{
			var cmd = CreateCommand (query, new object[]{});
			return cmd.ExecuteQuery();
		}


		/// <summary>
		/// Whether <see cref="BeginTransaction"/> has been called and the database is waiting for a <see cref="Commit"/>.
		/// </summary>
		public bool IsInTransaction {
			get { return _transactionDepth > 0; }
		}

		~SQLiteConnection ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			Close ();
		}

		public void Close ()
		{
			if (_open && Handle != NullHandle) {
				try {
					if (_mappings != null) {
						foreach (var sqlInsertCommand in _mappings.Values) {
							sqlInsertCommand.Dispose();
						}
					}
					var r = SQLite3.Close (Handle);
					if (r != SQLite3.Result.OK) {
						string msg = SQLite3.GetErrmsg (Handle);
						throw SQLiteException.New (r, msg);
					}
				}
				finally {
					Handle = NullHandle;
					_open = false;
				}
			}
		}
	}

	/// <summary>
	/// Represents a parsed connection string.
	/// </summary>
	class SQLiteConnectionString
	{
		public string ConnectionString { get; private set; }
		public string DatabasePath { get; private set; }
		public bool StoreDateTimeAsTicks { get; private set; }

#if NETFX_CORE
		static readonly string MetroStyleDataPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#endif

		public SQLiteConnectionString (string databasePath, bool storeDateTimeAsTicks)
		{
			ConnectionString = databasePath;
			StoreDateTimeAsTicks = storeDateTimeAsTicks;

#if NETFX_CORE
			DatabasePath = System.IO.Path.Combine (MetroStyleDataPath, databasePath);
#else
			DatabasePath = databasePath;
#endif
		}
	}

	[AttributeUsage (AttributeTargets.Property)]
	public class NotNullAttribute : Attribute
	{
	}

	public class TableMapping
	{
		public Type MappedType { get; private set; }

		public string TableName { get; private set; }

		public Column[] Columns { get; private set; }

		public Column PK { get; private set; }

		public string GetByPrimaryKeySql { get; private set; }

		Column _autoPk;
		Column[] _insertColumns;
		Column[] _insertOrReplaceColumns;

        public TableMapping(Type type, CreateFlags createFlags = CreateFlags.None)
		{
			MappedType = type;

#if NETFX_CORE
			var tableAttr = (TableAttribute)System.Reflection.CustomAttributeExtensions
                .GetCustomAttribute(type.GetTypeInfo(), typeof(TableAttribute), true);
#else
			var tableAttr = (TableAttribute)type.GetCustomAttributes (typeof (TableAttribute), true).FirstOrDefault ();
#endif

			TableName = tableAttr != null ? tableAttr.Name : MappedType.Name;

#if !NETFX_CORE
			var props = MappedType.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
#else
			var props = from p in MappedType.GetRuntimeProperties()
						where ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic) || (p.GetMethod != null && p.GetMethod.IsStatic) || (p.SetMethod != null && p.SetMethod.IsStatic))
						select p;
#endif
			var cols = new List<Column> ();
			foreach (var p in props) {
#if !NETFX_CORE
				var ignore = p.GetCustomAttributes (typeof(IgnoreAttribute), true).Length > 0;
#else
				var ignore = p.GetCustomAttributes (typeof(IgnoreAttribute), true).Count() > 0;
#endif
				if (p.CanWrite && !ignore) {
					cols.Add (new Column (p, createFlags));
				}
			}
			Columns = cols.ToArray ();
			foreach (var c in Columns) {
				if (c.IsAutoInc && c.IsPK) {
					_autoPk = c;
				}
				if (c.IsPK) {
					PK = c;
				}
			}
			
			HasAutoIncPK = _autoPk != null;

			if (PK != null) {
				GetByPrimaryKeySql = string.Format ("select * from \"{0}\" where \"{1}\" = ?", TableName, PK.Name);
			}
			else {
				// People should not be calling Get/Find without a PK
				GetByPrimaryKeySql = string.Format ("select * from \"{0}\" limit 1", TableName);
			}
		}

		public bool HasAutoIncPK { get; private set; }

		public void SetAutoIncPK (object obj, long id)
		{
			if (_autoPk != null) {
				_autoPk.SetValue (obj, Convert.ChangeType (id, _autoPk.ColumnType, null));
			}
		}

		public Column[] InsertColumns {
			get {
				if (_insertColumns == null) {
					_insertColumns = Columns.Where (c => !c.IsAutoInc).ToArray ();
				}
				return _insertColumns;
			}
		}

		public Column[] InsertOrReplaceColumns {
			get {
				if (_insertOrReplaceColumns == null) {
					_insertOrReplaceColumns = Columns.ToArray ();
				}
				return _insertOrReplaceColumns;
			}
		}

		public Column FindColumnWithPropertyName (string propertyName)
		{
			var exact = Columns.FirstOrDefault (c => c.PropertyName == propertyName);
			return exact;
		}

		public Column FindColumn (string columnName)
		{
			var exact = Columns.FirstOrDefault (c => c.Name == columnName);
			return exact;
		}
		
		PreparedSqlLiteInsertCommand _insertCommand;
		string _insertCommandExtra;

		public PreparedSqlLiteInsertCommand GetInsertCommand(SQLiteConnection conn, string extra)
		{
			if (_insertCommand == null) {
				_insertCommand = CreateInsertCommand(conn, extra);
				_insertCommandExtra = extra;
			}
			else if (_insertCommandExtra != extra) {
				_insertCommand.Dispose();
				_insertCommand = CreateInsertCommand(conn, extra);
				_insertCommandExtra = extra;
			}
			return _insertCommand;
		}
		
		PreparedSqlLiteInsertCommand CreateInsertCommand(SQLiteConnection conn, string extra)
		{
			var cols = InsertColumns;
		    string insertSql;
            if (!cols.Any() && Columns.Count() == 1 && Columns[0].IsAutoInc)
            {
                insertSql = string.Format("insert {1} into \"{0}\" default values", TableName, extra);
            }
            else
            {
				var replacing = string.Compare (extra, "OR REPLACE", StringComparison.OrdinalIgnoreCase) == 0;

				if (replacing) {
					cols = InsertOrReplaceColumns;
				}

                insertSql = string.Format("insert {3} into \"{0}\"({1}) values ({2})", TableName,
                                   string.Join(",", (from c in cols
                                                     select "\"" + c.Name + "\"").ToArray()),
                                   string.Join(",", (from c in cols
                                                     select "?").ToArray()), extra);
                
            }
			
			var insertCommand = new PreparedSqlLiteInsertCommand(conn);
			insertCommand.CommandText = insertSql;
			return insertCommand;
		}
		
		protected internal void Dispose()
		{
			if (_insertCommand != null) {
				_insertCommand.Dispose();
				_insertCommand = null;
			}
		}

		public class Column
		{
			PropertyInfo _prop;

			public string Name { get; private set; }

			public string PropertyName { get { return _prop.Name; } }

			public Type ColumnType { get; private set; }

			public string Collation { get; private set; }

            public bool IsAutoInc { get; private set; }
            public bool IsAutoGuid { get; private set; }

			public bool IsPK { get; private set; }

			public IEnumerable<IndexedAttribute> Indices { get; set; }

			public bool IsNullable { get; private set; }

			public int? MaxStringLength { get; private set; }

            public Column(PropertyInfo prop, CreateFlags createFlags = CreateFlags.None)
            {
                var colAttr = (ColumnAttribute)prop.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();

                _prop = prop;
                Name = colAttr == null ? prop.Name : colAttr.Name;
                //If this type is Nullable<T> then Nullable.GetUnderlyingType returns the T, otherwise it returns null, so get the actual type instead
                ColumnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                Collation = Orm.Collation(prop);

                IsPK = Orm.IsPK(prop) ||
					(((createFlags & CreateFlags.ImplicitPK) == CreateFlags.ImplicitPK) &&
					 	string.Compare (prop.Name, Orm.ImplicitPkName, StringComparison.OrdinalIgnoreCase) == 0);

                var isAuto = Orm.IsAutoInc(prop) || (IsPK && ((createFlags & CreateFlags.AutoIncPK) == CreateFlags.AutoIncPK));
                IsAutoGuid = isAuto && ColumnType == typeof(Guid);
                IsAutoInc = isAuto && !IsAutoGuid;

                Indices = Orm.GetIndices(prop);
                if (!Indices.Any()
                    && !IsPK
                    && ((createFlags & CreateFlags.ImplicitIndex) == CreateFlags.ImplicitIndex)
                    && Name.EndsWith (Orm.ImplicitIndexSuffix, StringComparison.OrdinalIgnoreCase)
                    )
                {
                    Indices = new IndexedAttribute[] { new IndexedAttribute() };
                }
                IsNullable = !(IsPK || Orm.IsMarkedNotNull(prop));
                MaxStringLength = Orm.MaxStringLength(prop);
            }

			public void SetValue (object obj, object val)
			{
				_prop.SetValue (obj, val, null);
			}

			public object GetValue (object obj)
			{
				return _prop.GetGetMethod().Invoke(obj, null);
			}
		}
	}

	public static class Orm
	{
        public const int DefaultMaxStringLength = 140;
        public const string ImplicitPkName = "Id";
        public const string ImplicitIndexSuffix = "Id";

		public static string SqlDecl (TableMapping.Column p, bool storeDateTimeAsTicks)
		{
			string decl = "\"" + p.Name + "\" " + SqlType (p, storeDateTimeAsTicks) + " ";
			
			if (p.IsPK) {
				decl += "primary key ";
			}
			if (p.IsAutoInc) {
				decl += "autoincrement ";
			}
			if (!p.IsNullable) {
				decl += "not null ";
			}
			if (!string.IsNullOrEmpty (p.Collation)) {
				decl += "collate " + p.Collation + " ";
			}
			
			return decl;
		}

		public static string SqlType (TableMapping.Column p, bool storeDateTimeAsTicks)
		{
			var clrType = p.ColumnType;
			if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32)) {
				return "integer";
			} else if (clrType == typeof(UInt32) || clrType == typeof(Int64)) {
				return "bigint";
			} else if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal)) {
				return "float";
			} else if (clrType == typeof(String)) {
				int? len = p.MaxStringLength;

				if (len.HasValue)
					return "varchar(" + len.Value + ")";

				return "varchar";
			} else if (clrType == typeof(TimeSpan)) {
                return "bigint";
			} else if (clrType == typeof(DateTime)) {
				return storeDateTimeAsTicks ? "bigint" : "datetime";
			} else if (clrType == typeof(DateTimeOffset)) {
				return "bigint";
#if !NETFX_CORE
			} else if (clrType.IsEnum) {
#else
			} else if (clrType.GetTypeInfo().IsEnum) {
#endif
				return "integer";
			} else if (clrType == typeof(byte[])) {
				return "blob";
            } else if (clrType == typeof(Guid)) {
                return "varchar(36)";
            } else {
				throw new NotSupportedException ("Don't know about " + clrType);
			}
		}

		public static bool IsPK (MemberInfo p)
		{
			var attrs = p.GetCustomAttributes (typeof(PrimaryKeyAttribute), true);
#if !NETFX_CORE
			return attrs.Length > 0;
#else
			return attrs.Count() > 0;
#endif
		}

		public static string Collation (MemberInfo p)
		{
			var attrs = p.GetCustomAttributes (typeof(CollationAttribute), true);
#if !NETFX_CORE
			if (attrs.Length > 0) {
				return ((CollationAttribute)attrs [0]).Value;
#else
			if (attrs.Count() > 0) {
                return ((CollationAttribute)attrs.First()).Value;
#endif
			} else {
				return string.Empty;
			}
		}

		public static bool IsAutoInc (MemberInfo p)
		{
			var attrs = p.GetCustomAttributes (typeof(AutoIncrementAttribute), true);
#if !NETFX_CORE
			return attrs.Length > 0;
#else
			return attrs.Count() > 0;
#endif
		}

		public static IEnumerable<IndexedAttribute> GetIndices(MemberInfo p)
		{
			var attrs = p.GetCustomAttributes(typeof(IndexedAttribute), true);
			return attrs.Cast<IndexedAttribute>();
		}
		
		public static int? MaxStringLength(PropertyInfo p)
		{
			var attrs = p.GetCustomAttributes (typeof(MaxLengthAttribute), true);
#if !NETFX_CORE
			if (attrs.Length > 0)
				return ((MaxLengthAttribute)attrs [0]).Value;
#else
			if (attrs.Count() > 0)
				return ((MaxLengthAttribute)attrs.First()).Value;
#endif

			return null;
		}

		public static bool IsMarkedNotNull(MemberInfo p)
		{
			var attrs = p.GetCustomAttributes (typeof (NotNullAttribute), true);
#if !NETFX_CORE
			return attrs.Length > 0;
#else
	return attrs.Count() > 0;
#endif
		}
	}

	public partial class SQLiteCommand
	{
		SQLiteConnection _conn;
		private List<Binding> _bindings;

		public string CommandText { get; set; }

		internal SQLiteCommand (SQLiteConnection conn)
		{
			_conn = conn;
			_bindings = new List<Binding> ();
			CommandText = "";
		}

		public int ExecuteNonQuery ()
		{
			if (_conn.Trace) {
				_conn.InvokeTrace ("Executing: " + this);
			}
			
			var r = SQLite3.Result.OK;
			var stmt = Prepare ();
			r = SQLite3.Step (stmt);
			Finalize (stmt);
			if (r == SQLite3.Result.Done) {
				int rowsAffected = SQLite3.Changes (_conn.Handle);
				return rowsAffected;
			} else if (r == SQLite3.Result.Error) {
				string msg = SQLite3.GetErrmsg (_conn.Handle);
				throw SQLiteException.New (r, msg);
			}
			else if (r == SQLite3.Result.Constraint) {
				if (SQLite3.ExtendedErrCode (_conn.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
					throw NotNullConstraintViolationException.New (r, SQLite3.GetErrmsg (_conn.Handle));
				}
			}

			throw SQLiteException.New(r, r.ToString());
		}

		public List<Dictionary<string, string>> ExecuteQuery ()
		{
			return ExecuteDeferredQuery();
		}

		/// <summary>
		/// Invoked every time an instance is loaded from the database.
		/// </summary>
		/// <param name='obj'>
		/// The newly created object.
		/// </param>
		/// <remarks>
		/// This can be overridden in combination with the <see cref="SQLiteConnection.NewCommand"/>
		/// method to hook into the life-cycle of objects.
		///
		/// Type safety is not possible because MonoTouch does not support virtual generic methods.
		/// </remarks>
		protected virtual void OnInstanceCreated (object obj)
		{
			// Can be overridden.
		}

		public List<Dictionary<string, string>> ExecuteDeferredQuery ()
		{
			if (_conn.Trace) {
				_conn.InvokeTrace ("Executing Query: " + this);
			}

			List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
			var stmt = Prepare ();
			try
			{
				int colCount = SQLite3.ColumnCount (stmt);

				while (SQLite3.Step (stmt) == SQLite3.Result.Row) 
				{
					Dictionary<string, string> row = new Dictionary<string, string>();
					for (int i = 0; i < colCount; i++) 
					{
						var name = SQLite3.ColumnName16 (stmt, i);
						var colType = SQLite3.ColumnType (stmt, i);
						var val = (string)ReadCol (stmt, i, colType, typeof(string));
						row[name] = val;
 					}
					result.Add(row);
				}
			}
			finally
			{
				SQLite3.Finalize(stmt);
			}
			return result;
		}

		public T ExecuteScalar<T> ()
		{
			if (_conn.Trace) {
				_conn.InvokeTrace ("Executing Query: " + this);
			}
			
			T val = default(T);
			
			var stmt = Prepare ();

            try
            {
                var r = SQLite3.Step (stmt);
                if (r == SQLite3.Result.Row) {
                    var colType = SQLite3.ColumnType (stmt, 0);
                    val = (T)ReadCol (stmt, 0, colType, typeof(T));
                }
                else if (r == SQLite3.Result.Done) {
                }
                else
                {
                    throw SQLiteException.New (r, SQLite3.GetErrmsg (_conn.Handle));
                }
            }
            finally
            {
                Finalize (stmt);
            }
			
			return val;
		}

		public void Bind (string name, object val)
		{
			_bindings.Add (new Binding {
				Name = name,
				Value = val
			});
		}

		public void Bind (object val)
		{
			Bind (null, val);
		}

		public override string ToString ()
		{
			var parts = new string[1 + _bindings.Count];
			parts [0] = CommandText;
			var i = 1;
			foreach (var b in _bindings) {
				parts [i] = string.Format ("  {0}: {1}", i - 1, b.Value);
				i++;
			}
			return string.Join (Environment.NewLine, parts);
		}

		Sqlite3Statement Prepare()
		{
			var stmt = SQLite3.Prepare2 (_conn.Handle, CommandText);
			BindAll (stmt);
			return stmt;
		}

		void Finalize (Sqlite3Statement stmt)
		{
			SQLite3.Finalize (stmt);
		}

		void BindAll (Sqlite3Statement stmt)
		{
			int nextIdx = 1;
			foreach (var b in _bindings) {
				if (b.Name != null) {
					b.Index = SQLite3.BindParameterIndex (stmt, b.Name);
				} else {
					b.Index = nextIdx++;
				}
				
				BindParameter (stmt, b.Index, b.Value, _conn.StoreDateTimeAsTicks);
			}
		}

		internal static IntPtr NegativePointer = new IntPtr (-1);

		internal static void BindParameter (Sqlite3Statement stmt, int index, object value, bool storeDateTimeAsTicks)
		{
			if (value == null) {
				SQLite3.BindNull (stmt, index);
			} else {
				if (value is Int32) {
					SQLite3.BindInt (stmt, index, (int)value);
				} else if (value is String) {
					SQLite3.BindText (stmt, index, (string)value, -1, NegativePointer);
				} else if (value is Byte || value is UInt16 || value is SByte || value is Int16) {
					SQLite3.BindInt (stmt, index, Convert.ToInt32 (value));
				} else if (value is Boolean) {
					SQLite3.BindInt (stmt, index, (bool)value ? 1 : 0);
				} else if (value is UInt32 || value is Int64) {
					SQLite3.BindInt64 (stmt, index, Convert.ToInt64 (value));
				} else if (value is Single || value is Double || value is Decimal) {
					SQLite3.BindDouble (stmt, index, Convert.ToDouble (value));
				} else if (value is TimeSpan) {
					SQLite3.BindInt64(stmt, index, ((TimeSpan)value).Ticks);
				} else if (value is DateTime) {
					if (storeDateTimeAsTicks) {
						SQLite3.BindInt64 (stmt, index, ((DateTime)value).Ticks);
					}
					else {
						SQLite3.BindText (stmt, index, ((DateTime)value).ToString ("yyyy-MM-dd HH:mm:ss"), -1, NegativePointer);
					}
				} else if (value is DateTimeOffset) {
					SQLite3.BindInt64 (stmt, index, ((DateTimeOffset)value).UtcTicks);
#if !NETFX_CORE
				} else if (value.GetType().IsEnum) {
#else
				} else if (value.GetType().GetTypeInfo().IsEnum) {
#endif
					SQLite3.BindInt (stmt, index, Convert.ToInt32 (value));
                } else if (value is byte[]){
                    SQLite3.BindBlob(stmt, index, (byte[]) value, ((byte[]) value).Length, NegativePointer);
                } else if (value is Guid) {
                    SQLite3.BindText(stmt, index, ((Guid)value).ToString(), 72, NegativePointer);
                } else {
                    throw new NotSupportedException("Cannot store type: " + value.GetType());
                }
			}
		}

		class Binding
		{
			public string Name { get; set; }

			public object Value { get; set; }

			public int Index { get; set; }
		}

		object ReadCol (Sqlite3Statement stmt, int index, SQLite3.ColType type, Type clrType)
		{
			if (type == SQLite3.ColType.Null) {
				return null;
			} else {
				if (clrType == typeof(String)) {
					return SQLite3.ColumnString (stmt, index);
				} else if (clrType == typeof(Int32)) {
					return (int)SQLite3.ColumnInt (stmt, index);
				} else if (clrType == typeof(Boolean)) {
					return SQLite3.ColumnInt (stmt, index) == 1;
				} else if (clrType == typeof(double)) {
					return SQLite3.ColumnDouble (stmt, index);
				} else if (clrType == typeof(float)) {
					return (float)SQLite3.ColumnDouble (stmt, index);
				} else if (clrType == typeof(TimeSpan)) {
					return new TimeSpan(SQLite3.ColumnInt64(stmt, index));
				} else if (clrType == typeof(DateTime)) {
					if (_conn.StoreDateTimeAsTicks) {
						return new DateTime (SQLite3.ColumnInt64 (stmt, index));
					}
					else {
						var text = SQLite3.ColumnString (stmt, index);
						return DateTime.Parse (text);
					}
				} else if (clrType == typeof(DateTimeOffset)) {
					return new DateTimeOffset(SQLite3.ColumnInt64 (stmt, index),TimeSpan.Zero);
#if !NETFX_CORE
				} else if (clrType.IsEnum) {
#else
				} else if (clrType.GetTypeInfo().IsEnum) {
#endif
					return SQLite3.ColumnInt (stmt, index);
				} else if (clrType == typeof(Int64)) {
					return SQLite3.ColumnInt64 (stmt, index);
				} else if (clrType == typeof(UInt32)) {
					return (uint)SQLite3.ColumnInt64 (stmt, index);
				} else if (clrType == typeof(decimal)) {
					return (decimal)SQLite3.ColumnDouble (stmt, index);
				} else if (clrType == typeof(Byte)) {
					return (byte)SQLite3.ColumnInt (stmt, index);
				} else if (clrType == typeof(UInt16)) {
					return (ushort)SQLite3.ColumnInt (stmt, index);
				} else if (clrType == typeof(Int16)) {
					return (short)SQLite3.ColumnInt (stmt, index);
				} else if (clrType == typeof(sbyte)) {
					return (sbyte)SQLite3.ColumnInt (stmt, index);
				} else if (clrType == typeof(byte[])) {
					return SQLite3.ColumnByteArray (stmt, index);
				} else if (clrType == typeof(Guid)) {
                  var text = SQLite3.ColumnString(stmt, index);
                  return new Guid(text);
                } else{
					throw new NotSupportedException ("Don't know how to read " + clrType);
				}
			}
		}
	}

	/// <summary>
	/// Since the insert never changed, we only need to prepare once.
	/// </summary>
	public class PreparedSqlLiteInsertCommand : IDisposable
	{
		public bool Initialized { get; set; }

		protected SQLiteConnection Connection { get; set; }

		public string CommandText { get; set; }

		protected Sqlite3Statement Statement { get; set; }
		internal static readonly Sqlite3Statement NullStatement = default(Sqlite3Statement);

		internal PreparedSqlLiteInsertCommand (SQLiteConnection conn)
		{
			Connection = conn;
		}

		public int ExecuteNonQuery (object[] source)
		{
			if (Connection.Trace) {
				Connection.InvokeTrace ("Executing: " + CommandText);
			}

			var r = SQLite3.Result.OK;

			if (!Initialized) {
				Statement = Prepare ();
				Initialized = true;
			}

			//bind the values.
			if (source != null) {
				for (int i = 0; i < source.Length; i++) {
					SQLiteCommand.BindParameter (Statement, i + 1, source [i], Connection.StoreDateTimeAsTicks);
				}
			}
			r = SQLite3.Step (Statement);

			if (r == SQLite3.Result.Done) {
				int rowsAffected = SQLite3.Changes (Connection.Handle);
				SQLite3.Reset (Statement);
				return rowsAffected;
			} else if (r == SQLite3.Result.Error) {
				string msg = SQLite3.GetErrmsg (Connection.Handle);
				SQLite3.Reset (Statement);
				throw SQLiteException.New (r, msg);
			} else if (r == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode (Connection.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
				SQLite3.Reset (Statement);
				throw NotNullConstraintViolationException.New (r, SQLite3.GetErrmsg (Connection.Handle));
			} else {
				SQLite3.Reset (Statement);
				throw SQLiteException.New (r, r.ToString ());
			}
		}

		protected virtual Sqlite3Statement Prepare ()
		{
			var stmt = SQLite3.Prepare2 (Connection.Handle, CommandText);
			return stmt;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (Statement != NullStatement) {
				try {
					SQLite3.Finalize (Statement);
				} finally {
					Statement = NullStatement;
					Connection = null;
				}
			}
		}

		~PreparedSqlLiteInsertCommand ()
		{
			Dispose (false);
		}
	}

	public abstract class BaseTableQuery
	{
		protected class Ordering
		{
			public string ColumnName { get; set; }
			public bool Ascending { get; set; }
		}
	}

	public static class SQLite3
	{
		public enum Result : int
		{
			OK = 0,
			Error = 1,
			Internal = 2,
			Perm = 3,
			Abort = 4,
			Busy = 5,
			Locked = 6,
			NoMem = 7,
			ReadOnly = 8,
			Interrupt = 9,
			IOError = 10,
			Corrupt = 11,
			NotFound = 12,
			Full = 13,
			CannotOpen = 14,
			LockErr = 15,
			Empty = 16,
			SchemaChngd = 17,
			TooBig = 18,
			Constraint = 19,
			Mismatch = 20,
			Misuse = 21,
			NotImplementedLFS = 22,
			AccessDenied = 23,
			Format = 24,
			Range = 25,
			NonDBFile = 26,
			Notice = 27,
			Warning = 28,
			Row = 100,
			Done = 101
		}

		public enum ExtendedResult : int
		{
			IOErrorRead = (Result.IOError | (1 << 8)),
			IOErrorShortRead = (Result.IOError | (2 << 8)),
			IOErrorWrite = (Result.IOError | (3 << 8)),
			IOErrorFsync = (Result.IOError | (4 << 8)),
			IOErrorDirFSync = (Result.IOError | (5 << 8)),
			IOErrorTruncate = (Result.IOError | (6 << 8)),
			IOErrorFStat = (Result.IOError | (7 << 8)),
			IOErrorUnlock = (Result.IOError | (8 << 8)),
			IOErrorRdlock = (Result.IOError | (9 << 8)),
			IOErrorDelete = (Result.IOError | (10 << 8)),
			IOErrorBlocked = (Result.IOError | (11 << 8)),
			IOErrorNoMem = (Result.IOError | (12 << 8)),
			IOErrorAccess = (Result.IOError | (13 << 8)),
			IOErrorCheckReservedLock = (Result.IOError | (14 << 8)),
			IOErrorLock = (Result.IOError | (15 << 8)),
			IOErrorClose = (Result.IOError | (16 << 8)),
			IOErrorDirClose = (Result.IOError | (17 << 8)),
			IOErrorSHMOpen = (Result.IOError | (18 << 8)),
			IOErrorSHMSize = (Result.IOError | (19 << 8)),
			IOErrorSHMLock = (Result.IOError | (20 << 8)),
			IOErrorSHMMap = (Result.IOError | (21 << 8)),
			IOErrorSeek = (Result.IOError | (22 << 8)),
			IOErrorDeleteNoEnt = (Result.IOError | (23 << 8)),
			IOErrorMMap = (Result.IOError | (24 << 8)),
			LockedSharedcache = (Result.Locked | (1 << 8)),
			BusyRecovery = (Result.Busy | (1 << 8)),
			CannottOpenNoTempDir = (Result.CannotOpen | (1 << 8)),
			CannotOpenIsDir = (Result.CannotOpen | (2 << 8)),
			CannotOpenFullPath = (Result.CannotOpen | (3 << 8)),
			CorruptVTab = (Result.Corrupt | (1 << 8)),
			ReadonlyRecovery = (Result.ReadOnly | (1 << 8)),
			ReadonlyCannotLock = (Result.ReadOnly | (2 << 8)),
			ReadonlyRollback = (Result.ReadOnly | (3 << 8)),
			AbortRollback = (Result.Abort | (2 << 8)),
			ConstraintCheck = (Result.Constraint | (1 << 8)),
			ConstraintCommitHook = (Result.Constraint | (2 << 8)),
			ConstraintForeignKey = (Result.Constraint | (3 << 8)),
			ConstraintFunction = (Result.Constraint | (4 << 8)),
			ConstraintNotNull = (Result.Constraint | (5 << 8)),
			ConstraintPrimaryKey = (Result.Constraint | (6 << 8)),
			ConstraintTrigger = (Result.Constraint | (7 << 8)),
			ConstraintUnique = (Result.Constraint | (8 << 8)),
			ConstraintVTab = (Result.Constraint | (9 << 8)),
			NoticeRecoverWAL = (Result.Notice | (1 << 8)),
			NoticeRecoverRollback = (Result.Notice | (2 << 8))
		}
        

		public enum ConfigOption : int
		{
			SingleThread = 1,
			MultiThread = 2,
			Serialized = 3
		}

#if !USE_CSHARP_SQLITE && !USE_WP8_NATIVE_SQLITE
		[DllImport("sqlite3", EntryPoint = "sqlite3_open", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Open ([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_open_v2", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Open ([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db, int flags, IntPtr zvfs);
		
		[DllImport("sqlite3", EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result Open(byte[] filename, out IntPtr db, int flags, IntPtr zvfs);

		[DllImport("sqlite3", EntryPoint = "sqlite3_open16", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result Open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_enable_load_extension", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result EnableLoadExtension (IntPtr db, int onoff);

		[DllImport("sqlite3", EntryPoint = "sqlite3_close", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Close (IntPtr db);
		
		[DllImport("sqlite3", EntryPoint = "sqlite3_initialize", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Initialize();
						
		[DllImport("sqlite3", EntryPoint = "sqlite3_shutdown", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Shutdown();
		
		[DllImport("sqlite3", EntryPoint = "sqlite3_config", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Config (ConfigOption option);

		[DllImport("sqlite3", EntryPoint = "sqlite3_win32_set_directory", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
		public static extern int SetDirectory (uint directoryType, string directoryPath);

		[DllImport("sqlite3", EntryPoint = "sqlite3_busy_timeout", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result BusyTimeout (IntPtr db, int milliseconds);

		[DllImport("sqlite3", EntryPoint = "sqlite3_changes", CallingConvention=CallingConvention.Cdecl)]
		public static extern int Changes (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_prepare_v2", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Prepare2 (IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, int numBytes, out IntPtr stmt, IntPtr pzTail);

#if NETFX_CORE
		[DllImport ("sqlite3", EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result Prepare2 (IntPtr db, byte[] queryBytes, int numBytes, out IntPtr stmt, IntPtr pzTail);
#endif

		public static IntPtr Prepare2 (IntPtr db, string query)
		{
			IntPtr stmt;
#if NETFX_CORE
            byte[] queryBytes = System.Text.UTF8Encoding.UTF8.GetBytes (query);
            var r = Prepare2 (db, queryBytes, queryBytes.Length, out stmt, IntPtr.Zero);
#else
            var r = Prepare2 (db, query, System.Text.UTF8Encoding.UTF8.GetByteCount (query), out stmt, IntPtr.Zero);
#endif
			if (r != Result.OK) {
				throw SQLiteException.New (r, GetErrmsg (db));
			}
			return stmt;
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_step", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Step (IntPtr stmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_reset", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Reset (IntPtr stmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_finalize", CallingConvention=CallingConvention.Cdecl)]
		public static extern Result Finalize (IntPtr stmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_last_insert_rowid", CallingConvention=CallingConvention.Cdecl)]
		public static extern long LastInsertRowid (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_errmsg16", CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr Errmsg (IntPtr db);

		public static string GetErrmsg (IntPtr db)
		{
			return Marshal.PtrToStringUni (Errmsg (db));
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_parameter_index", CallingConvention=CallingConvention.Cdecl)]
		public static extern int BindParameterIndex (IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_null", CallingConvention=CallingConvention.Cdecl)]
		public static extern int BindNull (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_int", CallingConvention=CallingConvention.Cdecl)]
		public static extern int BindInt (IntPtr stmt, int index, int val);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_int64", CallingConvention=CallingConvention.Cdecl)]
		public static extern int BindInt64 (IntPtr stmt, int index, long val);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_double", CallingConvention=CallingConvention.Cdecl)]
		public static extern int BindDouble (IntPtr stmt, int index, double val);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_text16", CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int BindText (IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string val, int n, IntPtr free);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_blob", CallingConvention=CallingConvention.Cdecl)]
		public static extern int BindBlob (IntPtr stmt, int index, byte[] val, int n, IntPtr free);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_count", CallingConvention=CallingConvention.Cdecl)]
		public static extern int ColumnCount (IntPtr stmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_name", CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr ColumnName (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_name16", CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr ColumnName16Internal (IntPtr stmt, int index);
		public static string ColumnName16(IntPtr stmt, int index)
		{
			return Marshal.PtrToStringUni(ColumnName16Internal(stmt, index));
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_type", CallingConvention=CallingConvention.Cdecl)]
		public static extern ColType ColumnType (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_int", CallingConvention=CallingConvention.Cdecl)]
		public static extern int ColumnInt (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_int64", CallingConvention=CallingConvention.Cdecl)]
		public static extern long ColumnInt64 (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_double", CallingConvention=CallingConvention.Cdecl)]
		public static extern double ColumnDouble (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_text", CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr ColumnText (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_text16", CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr ColumnText16 (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_blob", CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr ColumnBlob (IntPtr stmt, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_bytes", CallingConvention=CallingConvention.Cdecl)]
		public static extern int ColumnBytes (IntPtr stmt, int index);

        [DllImport("sqlite3", EntryPoint = "sqlite3_key", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Key(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string pKey, int nkey);

        [DllImport("sqlite3", EntryPoint = "sqlite3_rekey", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Rekey(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string pKey, int nkey);

        public static string ColumnString (IntPtr stmt, int index)
		{
			return Marshal.PtrToStringUni (SQLite3.ColumnText16 (stmt, index));
		}

		public static byte[] ColumnByteArray (IntPtr stmt, int index)
		{
			int length = ColumnBytes (stmt, index);
			var result = new byte[length];
			if (length > 0)
				Marshal.Copy (ColumnBlob (stmt, index), result, 0, length);
			return result;
		}

		[DllImport ("sqlite3", EntryPoint = "sqlite3_extended_errcode", CallingConvention = CallingConvention.Cdecl)]
		public static extern ExtendedResult ExtendedErrCode (IntPtr db);

		[DllImport ("sqlite3", EntryPoint = "sqlite3_libversion_number", CallingConvention = CallingConvention.Cdecl)]
		public static extern int LibVersionNumber ();
#else
		public static Result Open(string filename, out Sqlite3DatabaseHandle db)
		{
			return (Result) Sqlite3.sqlite3_open(filename, out db);
		}

		public static Result Open(string filename, out Sqlite3DatabaseHandle db, int flags, IntPtr zVfs)
		{
#if USE_WP8_NATIVE_SQLITE
			return (Result)Sqlite3.sqlite3_open_v2(filename, out db, flags, "");
#else
			return (Result)Sqlite3.sqlite3_open_v2(filename, out db, flags, null);
#endif
		}

		public static Result Close(Sqlite3DatabaseHandle db)
		{
			return (Result)Sqlite3.sqlite3_close(db);
		}

		public static Result BusyTimeout(Sqlite3DatabaseHandle db, int milliseconds)
		{
			return (Result)Sqlite3.sqlite3_busy_timeout(db, milliseconds);
		}

		public static int Changes(Sqlite3DatabaseHandle db)
		{
			return Sqlite3.sqlite3_changes(db);
		}

		public static Sqlite3Statement Prepare2(Sqlite3DatabaseHandle db, string query)
		{
			Sqlite3Statement stmt = default(Sqlite3Statement);
#if USE_WP8_NATIVE_SQLITE
			var r = Sqlite3.sqlite3_prepare_v2(db, query, out stmt);
#else
			stmt = new Sqlite3Statement();
			var r = Sqlite3.sqlite3_prepare_v2(db, query, -1, ref stmt, 0);
#endif
			if (r != 0)
			{
				throw SQLiteException.New((Result)r, GetErrmsg(db));
			}
			return stmt;
		}

		public static Result Step(Sqlite3Statement stmt)
		{
			return (Result)Sqlite3.sqlite3_step(stmt);
		}

		public static Result Reset(Sqlite3Statement stmt)
		{
			return (Result)Sqlite3.sqlite3_reset(stmt);
		}

		public static Result Finalize(Sqlite3Statement stmt)
		{
			return (Result)Sqlite3.sqlite3_finalize(stmt);
		}

		public static long LastInsertRowid(Sqlite3DatabaseHandle db)
		{
			return Sqlite3.sqlite3_last_insert_rowid(db);
		}

		public static string GetErrmsg(Sqlite3DatabaseHandle db)
		{
			return Sqlite3.sqlite3_errmsg(db);
		}

		public static int BindParameterIndex(Sqlite3Statement stmt, string name)
		{
			return Sqlite3.sqlite3_bind_parameter_index(stmt, name);
		}

		public static int BindNull(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_bind_null(stmt, index);
		}

		public static int BindInt(Sqlite3Statement stmt, int index, int val)
		{
			return Sqlite3.sqlite3_bind_int(stmt, index, val);
		}

		public static int BindInt64(Sqlite3Statement stmt, int index, long val)
		{
			return Sqlite3.sqlite3_bind_int64(stmt, index, val);
		}

		public static int BindDouble(Sqlite3Statement stmt, int index, double val)
		{
			return Sqlite3.sqlite3_bind_double(stmt, index, val);
		}

		public static int BindText(Sqlite3Statement stmt, int index, string val, int n, IntPtr free)
		{
#if USE_WP8_NATIVE_SQLITE
			return Sqlite3.sqlite3_bind_text(stmt, index, val, n);
#else
			return Sqlite3.sqlite3_bind_text(stmt, index, val, n, null);
#endif
		}

		public static int BindBlob(Sqlite3Statement stmt, int index, byte[] val, int n, IntPtr free)
		{
#if USE_WP8_NATIVE_SQLITE
			return Sqlite3.sqlite3_bind_blob(stmt, index, val, n);
#else
			return Sqlite3.sqlite3_bind_blob(stmt, index, val, n, null);
#endif
		}

		public static int ColumnCount(Sqlite3Statement stmt)
		{
			return Sqlite3.sqlite3_column_count(stmt);
		}

		public static string ColumnName(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_name(stmt, index);
		}

		public static string ColumnName16(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_name(stmt, index);
		}

		public static ColType ColumnType(Sqlite3Statement stmt, int index)
		{
			return (ColType)Sqlite3.sqlite3_column_type(stmt, index);
		}

		public static int ColumnInt(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_int(stmt, index);
		}

		public static long ColumnInt64(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_int64(stmt, index);
		}

		public static double ColumnDouble(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_double(stmt, index);
		}

		public static string ColumnText(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_text(stmt, index);
		}

		public static string ColumnText16(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_text(stmt, index);
		}

		public static byte[] ColumnBlob(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_blob(stmt, index);
		}

		public static int ColumnBytes(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_bytes(stmt, index);
		}

		public static string ColumnString(Sqlite3Statement stmt, int index)
		{
			return Sqlite3.sqlite3_column_text(stmt, index);
		}

		public static byte[] ColumnByteArray(Sqlite3Statement stmt, int index)
		{
			return ColumnBlob(stmt, index);
		}

		public static Result EnableLoadExtension(Sqlite3DatabaseHandle db, int onoff)
		{
			return (Result)Sqlite3.sqlite3_enable_load_extension(db, onoff);
		}

		public static ExtendedResult ExtendedErrCode(Sqlite3DatabaseHandle db)
		{
			return (ExtendedResult)Sqlite3.sqlite3_extended_errcode(db);
		}
#endif

		public enum ColType : int
		{
			Integer = 1,
			Float = 2,
			Text = 3,
			Blob = 4,
			Null = 5
		}
	}
}

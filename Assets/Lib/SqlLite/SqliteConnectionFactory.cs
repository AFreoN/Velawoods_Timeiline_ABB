using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SQLite4Unity3d
{
    public interface ISQLiteConnectionFactory
    {
        ISQLiteConnection Create(string address);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Name { get; set; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public virtual bool Unique { get; set; }

        public IndexedAttribute()
        {
        }

        public IndexedAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : IndexedAttribute
    {
        public override bool Unique
        {
            get { return true; }
            set
            {
                /* throw?  */
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MaxLengthAttribute : Attribute
    {
        public int Value { get; private set; }

        public MaxLengthAttribute(int length)
        {
            Value = length;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CollationAttribute : Attribute
    {
        public string Value { get; private set; }

        public CollationAttribute(string collation)
        {
            Value = collation;
        }
    }


    public interface ISQLiteConnection : IDisposable
    {
        string DatabasePath { get; }
        bool TimeExecution { get; set; }
        bool Trace { get; set; }
        int Execute(string query, params object[] args);
        T ExecuteScalar<T>(string query, params object[] args);
        List<Dictionary<string, string>> Query(string query);
        bool IsInTransaction { get; }
        void Close();
        void SetDbKey(string key);
        void Key(string key);
    }

    public interface ITableMapping
    {
        string TableName { get; }
    }

    public interface ISQLiteCommand
    {
    }

    public interface ITableQuery<T> : IEnumerable<T> where T : new()
    {
        ITableQuery<T> Where(Expression<Func<T, bool>> predExpr);
        ITableQuery<T> Take(int n);
        ITableQuery<T> Skip(int n);
        T ElementAt(int index);
        ITableQuery<T> OrderBy<U>(Expression<Func<T, U>> orderExpr);
        ITableQuery<T> OrderByDescending<U>(Expression<Func<T, U>> orderExpr);

        ITableQuery<TResult> Join<TInner, TKey, TResult>(
            ITableQuery<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector)
            where TInner : new()
            where TResult : new();

        ITableQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
            where TResult : new();

        int Count();
        T First();
        T FirstOrDefault();
    }


}
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;

namespace CoreLib
{
	public class EncryptionManager : MonoSingleton<EncryptionManager>
	{

		private Aes _aes;
		private ICryptoTransform _encryptor;
		private ICryptoTransform _decryptor;

		override protected void Init( ) {

			_aes = Aes.Create ();

			_aes.Key = System.Text.Encoding.Unicode.GetBytes("s5cg&sf54htNHDFU");


		}


		/// <summary>
		/// Encrypt the specified string. Returns two strings, the first is the encrypted version of the string. The second is
		/// the IV used to encrypt the string that must be sent along with it.
		/// </summary>
		/// <param name="toEncrypt">To encrypt.</param>

		public string[] Encrypt( string toEncrypt, string iv = null ) {

			string[] result = {"",""};

			// Get a new IV for this encryption
			if (iv == null) {
				_aes.GenerateIV ();
				result [1] = Convert.ToBase64String (_aes.IV);
			} else {
				_aes.IV = Convert.FromBase64String (iv);
				result [1] = iv;
			}

			_encryptor = _aes.CreateEncryptor ( _aes.Key, _aes.IV );

			byte[] encrypted_bytes;

			using (MemoryStream msEncrypt = new MemoryStream()) {

				using ( CryptoStream csEncrypt = new CryptoStream( msEncrypt, _encryptor, CryptoStreamMode.Write ) ) {
				
					using ( StreamWriter swEncrypt = new StreamWriter( csEncrypt ) ) {

						swEncrypt.Write( toEncrypt );

					}

					encrypted_bytes = msEncrypt.ToArray();
				
				}

			}

			result[0] = Convert.ToBase64String( encrypted_bytes );
			return result;
		}

		/// <summary>
		/// Decrypt the specified toDecrypt using IV.
		/// </summary>
		/// <param name="toDecrypt">To decrypt.</param>
		/// <param name="IV">I.</param>

		public string Decrypt( string toDecrypt, string IV ) {

			_aes.IV = Convert.FromBase64String (IV);

			_decryptor = _aes.CreateDecryptor ( _aes.Key, _aes.IV );

			byte[] encrypted_bytes = Convert.FromBase64String( toDecrypt );
			string text;
			
			using (MemoryStream msDecrypt = new MemoryStream(encrypted_bytes)) {
				
				using ( CryptoStream csDecrypt = new CryptoStream( msDecrypt, _decryptor, CryptoStreamMode.Read ) ) {
		
					using ( StreamReader srDecrypt = new StreamReader( csDecrypt ) ) {
						
						text = srDecrypt.ReadToEnd( );
						
					}
										
				}
				
			}
			
			return text;
			
		}
	}
}


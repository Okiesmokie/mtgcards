using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace MTGCards.Core {
	public class Util {
		/// <summary>
		/// Generates an MD5 Hash for the specified string
		/// </summary>
		/// <param name="text">The string to generate the hash for.</param>
		/// <returns>The MD5 Hash that represents the given string.</returns>
		public static string GenerateMD5(string text) {
			var md5 = MD5.Create();
			byte[] hash = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(text));

			var sb = new System.Text.StringBuilder();

			Array.ForEach(hash, b => {
				sb.Append(b.ToString("X2"));
			});

			return sb.ToString();
		}
	}
}

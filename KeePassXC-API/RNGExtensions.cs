using System;
using System.Security.Cryptography;

namespace KeePassXC_API
{
    static class RNGExtension
	{
		public static byte[] GetBytes(this RandomNumberGenerator rng, int len)
		{
			byte[] bytes = new byte[len];
			rng.GetBytes(bytes);
			return bytes;
		}
		public static string ToBase64(this byte[] ar)
		{
			return Convert.ToBase64String(ar);
		}

		public static byte[] FromBase64(this string val)
		{
			return Convert.FromBase64String(val);
		}
	}
}

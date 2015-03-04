using System;
using System.IO;
using System.Security.Cryptography;

namespace TunrSync
{
	public static class Md5Hash
	{
		public static string Md5HashFile(string fileName)
		{
			using (StreamReader sr = new StreamReader (fileName)) {
				var buf = new byte[SyncAgent.Md5Size];
				int bytesRead = 0;
				while (true) {
					var read = sr.BaseStream.Read (buf, 0, SyncAgent.Md5Size);
					bytesRead += read;
					if (bytesRead == SyncAgent.Md5Size || read == 0) {
						break;
					}
				}
				MD5CryptoServiceProvider md5h = new MD5CryptoServiceProvider ();
				var sHash = BitConverter.ToString (md5h.ComputeHash (buf)).Replace ("-", "");
				return sHash;
			}
		}
	}
}


using System;
using Gtk;

namespace TunrSync
{
	class Program
	{
		public static readonly int c_md5size = 128 * 1024;
		#if DEBUG
			public static readonly string baseurl = "https://dev.tunr.io";
		#else
			public static readonly string baseurl = "https://play.tunr.io";
		#endif
		public static readonly string apiprefix = "/api";

		public static void Main (string[] args)
		{
			Application.Init ();
			LogInWindow win = new LogInWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}

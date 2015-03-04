using System;
using Gtk;

namespace TunrSync
{
	public class Program
	{
		public SyncAgent SyncAgent { get; set; }
		public static void Main (string[] args)
		{
			new Program ().start ();
		}

		public void start()
		{
			Application.Init ();
			this.SyncAgent = new SyncAgent ();
			if (Configuration.Current.Authentication == null) {
				LogInWindow win = new LogInWindow (this);
				win.Show ();
			} else {
				SyncWindow syncwin = new SyncWindow (this);
				syncwin.Show ();
			}
			Application.Run ();
		}
	}
}

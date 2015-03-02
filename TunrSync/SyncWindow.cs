using System;
using Gtk;

namespace TunrSync
{
	public partial class SyncWindow : Gtk.Window
	{
		public SyncWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}


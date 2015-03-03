using System;
using Gtk;
using TunrSync.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace TunrSync {
	public partial class LogInWindow: Gtk.Window
	{
		private Program _program { get; set; }
		public LogInWindow (Program program) : base (Gtk.WindowType.Toplevel)
		{
			this._program = program;
			Build ();
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		protected async void BtnLogIn_clicked (object sender, EventArgs e)
		{
			Application.Invoke (delegate {
				EntryEmail.Sensitive = false;
				EntryPassword.Sensitive = false;
				BtnLogIn.Sensitive = false;
			});
			var authResult = _program.SyncAgent.Authenticate (EntryEmail.Text, EntryPassword.Text);
			if (await authResult) {
				Application.Invoke (delegate {
					SyncWindow syncwin = new SyncWindow (this._program);
					syncwin.Show ();
					this.Destroy ();
				});
			} else {
				Application.Invoke (delegate {
					MessageDialog md = new MessageDialog (this,
						DialogFlags.Modal,
						MessageType.Error,
						ButtonsType.Ok,
						"We couldn't log you in! Check your e-mail and password, then try again.");
					md.Run ();
					md.Destroy ();
					EntryEmail.Sensitive = true;
					EntryPassword.Sensitive = true;
					BtnLogIn.Sensitive = true;
				});
			}
		}


	}
}
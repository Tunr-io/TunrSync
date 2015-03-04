using System;
using Gtk;
using System.Threading.Tasks;

namespace TunrSync
{
	public partial class SyncWindow : Gtk.Window
	{
		private Program _program { get; set; }
		public SyncWindow (Program program) :
			base (Gtk.WindowType.Toplevel)
		{
			_program = program;
			this.Build ();
			// Set up all of our fields
			// Display Name
			LabelDisplayName.Text = Configuration.Current.Authentication.DisplayName;
			Pango.FontDescription font = new Pango.FontDescription ();
			font.Size = 24;
			font.Weight = Pango.Weight.Bold;
			LabelDisplayName.ModifyFont (font);
			// Email
			LabelEmail.Text = Configuration.Current.Authentication.userName;
			// Sync path
			EntryDirectory.Text = Configuration.Current.SyncDirectory;

			// Bind to SyncAgent events
			_program.SyncAgent.OnSyncMessage += (string message) => {
				Application.Invoke (delegate {
					TextSyncMessages.Buffer.Text += message + "\n";
					TextIter ti = TextSyncMessages.Buffer.GetIterAtLine(TextSyncMessages.Buffer.LineCount-1);
					TextMark tm = TextSyncMessages.Buffer.CreateMark("eot", ti, false);
					TextSyncMessages.ScrollToMark(tm, 0, false, 0, 0);
				});
			};

			_program.SyncAgent.OnSyncProgress += (double progress, string message) => {
				Application.Invoke( delegate {
					ProgressSync.Fraction = progress;
					ProgressSync.Text = message;
				});
			};

			_program.SyncAgent.OnSyncComplete += HandleOnSyncComplete;
		}

		void HandleOnSyncComplete ()
		{
			Application.Invoke((s, e) => ButtonSync.Sensitive = true );
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		protected void BtnChangeDirectory_clicked (object sender, EventArgs e)
		{
			Gtk.FileChooserDialog chooser = new Gtk.FileChooserDialog ("Choose your music directory",
				                                null,
				                                FileChooserAction.SelectFolder,
				                                "Cancel", ResponseType.Cancel,
				                                "Choose", ResponseType.Accept);
			if (chooser.Run () == (int)(ResponseType.Accept)) {
				Configuration.Current.SyncDirectory = chooser.Filename;
				EntryDirectory.Text = Configuration.Current.SyncDirectory;
			}
			chooser.Destroy ();
	}

		protected void ButtonSync_clicked (object sender, EventArgs e)
		{
			ButtonSync.Sensitive = false;
			Task.Run (() => {
				_program.SyncAgent.sync ();
			});
		}
	}
}


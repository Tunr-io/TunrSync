using System;
using Gtk;

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
			LabelDisplayName.Text = _program.SyncAgent.Authentication.DisplayName;
			Pango.FontDescription font = new Pango.FontDescription ();
			font.Size = 24;
			font.Weight = Pango.Weight.Bold;
			LabelDisplayName.ModifyFont (font);
			// Email
			LabelEmail.Text = _program.SyncAgent.Authentication.userName;
			// Sync path
			EntryDirectory.Text = _program.SyncAgent.MusicPath;
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
				_program.SyncAgent.MusicPath = chooser.Filename;
				EntryDirectory.Text = _program.SyncAgent.MusicPath;
			}
			chooser.Destroy ();
	}
	}
}


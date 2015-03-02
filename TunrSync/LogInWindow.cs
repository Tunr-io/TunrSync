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
		public LogInWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		protected async void BtnLogIn_clicked (object sender, EventArgs e)
		{
			var authResult = await Authenticate (EntryEmail.Text, EntryPassword.Text);
			if (authResult != null) {
				Application.Invoke (delegate {
					SyncWindow syncwin = new SyncWindow ();
					syncwin.Show ();
					this.Destroy ();
				});
			} else {
				Application.Invoke (delegate {
					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "We couldn't log you in! Check your e-mail and password, then try again.");
					md.Run ();
					md.Destroy ();
				});
			}
		}

		protected async Task<AuthResponse> Authenticate (string username, string password)
		{
			using (var client = new HttpClient())
			{
				// New code:
				client.BaseAddress = new Uri(Program.baseurl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				FormUrlEncodedContent content = new FormUrlEncodedContent(new[] 
					{
						new KeyValuePair<string, string>("grant_type", "password"),
						new KeyValuePair<string, string>("username", username),
						new KeyValuePair<string, string>("password", password)
					});
				HttpResponseMessage response = await client.PostAsync("/Token", content);
				if (response.IsSuccessStatusCode)
				{
					var authString = await response.Content.ReadAsStringAsync();
					var auth = JsonConvert.DeserializeObject<AuthResponse>(authString);
					return auth;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
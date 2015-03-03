using System;
using TunrSync.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace TunrSync
{
	public class SyncAgent
	{
		public static readonly int c_md5size = 128 * 1024;
		#if DEBUG
			public static readonly string baseurl = "https://dev.tunr.io";
		#else
			public static readonly string baseurl = "https://play.tunr.io";
		#endif
		public static readonly string apiprefix = "/api";
		public AuthResponse Authentication { get; set; }
		public string MusicPath { get; set; }

		public SyncAgent ()
		{
			MusicPath = GetDefaultMusicPath ();
		}

		public string GetDefaultMusicPath() {
			string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
			                  Environment.OSVersion.Platform == PlatformID.MacOSX)
				? Environment.GetEnvironmentVariable ("HOME")
				: Environment.ExpandEnvironmentVariables ("%HOMEDRIVE%%HOMEPATH%");
			return Path.Combine (homePath, "Music");
		}

		public async Task<bool> Authenticate (string username, string password)
		{
			using (var client = new HttpClient())
			{
				// New code:
				client.BaseAddress = new Uri(SyncAgent.baseurl);
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
					this.Authentication = auth;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}


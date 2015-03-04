using System;
using TunrSync.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace TunrSync
{
	public class SyncAgent
	{
		public const int Md5Size = 128 * 1024;
		public readonly string[] SupportedExtensions = { "*.mp3", "*.ogg", "*.m4a", "*.flac" };
		#if DEBUG
			public static readonly string baseurl = "https://dev.tunr.io";
		#else
			public static readonly string baseurl = "https://play.tunr.io";
		#endif
		public static readonly string apiprefix = "/api";

		// Events
		public delegate void SyncMessageEventHandler(string message);
		public event SyncMessageEventHandler OnSyncMessage;

		public delegate void ProgressEventHandler(double progress, string message);
		public event ProgressEventHandler OnSyncProgress;

		public delegate void SyncCompleteEventHandler();
		public event SyncCompleteEventHandler OnSyncComplete;

		public SyncAgent ()
		{
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
					Configuration.Current.Authentication = auth;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public void sync() {
			ScanLibrary ();
			OnSyncComplete ();
		}

		private Dictionary<string, FileInfo> ScanLibrary()
		{
			OnSyncMessage ("Searching for files...");
			var directory = new DirectoryInfo (Configuration.Current.SyncDirectory);
			var files = SupportedExtensions.AsParallel().SelectMany(searchPattern =>
				directory.EnumerateFiles(searchPattern, 
					SearchOption.AllDirectories));
			OnSyncMessage ("Found " + files.Count() + " files in sync directory.");
			OnSyncMessage ("Indexing files ...");
			var localIndex = new Dictionary<string, FileInfo> ();
			int processedCount = 0;
			foreach (var file in files) {
				var hash = Md5Hash.Md5HashFile (file.FullName);
				if (!localIndex.ContainsKey (hash)) {
					localIndex.Add (hash, file);
				}
				processedCount++;
				OnSyncProgress ((processedCount / (double)files.Count ()) * 0.25, 
					"Indexed " + processedCount + "/" + files.Count () + "...");
				OnSyncMessage ("Indexed '" + file.Name + "'...");
			}
			OnSyncMessage ("Indexing complete.");
			return localIndex;
		}
	}
}


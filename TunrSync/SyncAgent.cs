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
using System.Text;
using System.Collections.Concurrent;

namespace TunrSync
{
	public class SyncAgent
	{
		public const int Md5Size = 128 * 1024;
		public readonly string[] SupportedExtensions = { "*.mp3", "*.ogg", "*.m4a", "*.flac" };
		#if DEBUG
			public static readonly string BaseUrl = "https://dev.tunr.io";
		#else
			public static readonly string BaseUrl = "https://play.tunr.io";
		#endif
		public static readonly string ApiPrefix = "/api";
		private const int ThreadCount = 4;
		private const int IndexThreadCount = 10;

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
				client.BaseAddress = new Uri(SyncAgent.BaseUrl);
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

		public async void Sync() {
			OnSyncProgress (0, "Starting Sync");
			OnSyncMessage ("Starting Sync...");
			var localLibrary = await ScanLibrary ();
			var cloudLibrary = await FetchLibrary ();

			var toDownload = cloudLibrary.Keys.Except (localLibrary.Keys);
			var toUpload = localLibrary.Keys.Except (cloudLibrary.Keys);

			OnSyncMessage (toUpload.Count() + " files queued for upload.");

			var filesToUpload = localLibrary.Where (v => toUpload.Contains (v.Key)).Select (v => v.Value).ToList();
			await UploadFiles (filesToUpload);

			OnSyncMessage ("Sync complete.");
			OnSyncProgress (1, "Sync Complete");
			OnSyncComplete ();
		}

		private async Task<Dictionary<string, FileInfo>> ScanLibrary()
		{
			OnSyncMessage ("Searching for files...");
			var directory = new DirectoryInfo (Configuration.Current.SyncDirectory);
			var files = SupportedExtensions.AsParallel().SelectMany(searchPattern =>
				directory.EnumerateFiles(searchPattern, 
					SearchOption.AllDirectories)).ToList();
			OnSyncMessage ("Found " + files.Count() + " files in sync directory.");
			OnSyncMessage ("Indexing files ...");
			var localIndex = new ConcurrentDictionary<string, FileInfo> ();
			int processedCount = 0;

			List<FileInfo>[] indexList = new List<FileInfo>[IndexThreadCount];
			for (int i = 0; i < IndexThreadCount; i++) {
				indexList[i] = new List<FileInfo> ();
			}
			for (int i = 0; i < files.Count; i++) {
				indexList [i % IndexThreadCount].Add(files [i]);
			}

			await Task.WhenAll(indexList.Select(l => Task.Run(() => {
				foreach (var file in l) {
					var hash = Md5Hash.Md5HashFile (file.FullName);
					if (!localIndex.ContainsKey (hash)) {
						localIndex.TryAdd (hash, file);
					}
					processedCount++;
					if (processedCount % 10 == 0) {
					OnSyncProgress ((processedCount / (double)files.Count ()) * 0.25, 
						"Indexed " + processedCount + "/" + files.Count () + "...");
					}
				}
			})));


			OnSyncMessage ("Indexed " + files.Count() + " files.");
			return new Dictionary<string, FileInfo>(localIndex);
		}

		private async Task<Dictionary<string,Song>> FetchLibrary()
		{
			OnSyncMessage ("Fetching library from Tunr cloud...");
			OnSyncProgress (0.25, "Connecting to Tunr cloud...");
			using (var client = new HttpClient ()) {
				// New code:
				client.BaseAddress = new Uri (BaseUrl);
				client.DefaultRequestHeaders.Accept.Clear ();
				client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Bearer", Configuration.Current.Authentication.access_token);
				StringBuilder urlparams = new StringBuilder ();
				var library_request = await client.GetAsync (ApiPrefix + "/Library");
				if (!library_request.IsSuccessStatusCode) {
					OnSyncMessage ("Fetching library from Tunr cloud...");
					throw new ApplicationException ("Could not fetch library.");
				}
				var responseString = await library_request.Content.ReadAsStringAsync();
				var songs = JsonConvert.DeserializeObject<List<Song>>(responseString);
				OnSyncMessage ("Found " + songs.Count + " songs.");
				OnSyncProgress (0.3, songs.Count + " songs on Tunr cloud");
				return songs.ToDictionary (k => k.Md5Hash);
			}
		}

		private async Task UploadFiles(List<FileInfo> files)
		{
			// Initialize a list for each thread
			List<FileInfo>[] threadLists = new List<FileInfo>[ThreadCount];
			for (int i = 0; i < ThreadCount; i++) {
				threadLists [i] = new List<FileInfo> ();
			}

			// Evenly distribute files
			for (int i = 0; i < files.Count; i++) {
				threadLists [i % ThreadCount].Add (files [i]);
			}

			// Start threads
			int totalNumber = files.Count;
			int successCount = 0;
			int failCount = 0;
			await Task.WhenAll(threadLists.Select(l => Task.Run(async () => {
				foreach (var file in l) {
					OnSyncMessage("Uploading '" + file.Name + "'...");
					try {
						await UploadFile(file);
					} catch (Exception) {
						OnSyncMessage("! Error uploading '" + file.Name + "'");
						failCount++;
						continue;
					}
					successCount++;
					OnSyncProgress(0.3 + ((successCount + failCount) / (double)totalNumber) * 0.7,
						"Uploading " + (successCount + failCount) + "/" + totalNumber);
				}
			})));
			OnSyncMessage ("Uploaded " + successCount + " files with " + failCount + " failures.");
		}

		private async Task UploadFile(FileInfo file)
		{
			using (var client = new HttpClient ()) {
				client.BaseAddress = new Uri (BaseUrl);
				client.DefaultRequestHeaders.Authorization = 
					new AuthenticationHeaderValue ("Bearer", 
						Configuration.Current.Authentication.access_token);
				var requestContent = new MultipartFormDataContent ();
				var fileContent = new StreamContent (new FileStream (file.FullName, FileMode.Open));
				fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse ("application/octet-stream");
				requestContent.Add (fileContent, "file", file.Name);

				var response = await client.PostAsync (ApiPrefix + "/Library", requestContent);
				if (!response.IsSuccessStatusCode) {
					var e = new HttpRequestException ("Could not upload file '" + file.Name);
					throw e;
				}
			}
		}
	}
}


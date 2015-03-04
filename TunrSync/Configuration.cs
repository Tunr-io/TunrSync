using System;
using TunrSync.Models;
using Newtonsoft.Json;
using System.IO;

namespace TunrSync
{
	public class Configuration
	{
		private static Configuration _Current;
		public static Configuration Current
		{
			get {
				if (_Current == null) {
					_Current = LoadConfiguration ();
				}
				return _Current;
			}
		}

		[JsonIgnore]
		private static string ConfigPath
		{
			get { 
				var pathConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".TunrSync");
				if (!Directory.Exists (pathConfig)) {
					Directory.CreateDirectory (pathConfig);
				}
				return Path.Combine (pathConfig, "config.json");
			}
		}

		[JsonIgnore]
		private static bool IsLoading = false;

		public static Configuration LoadConfiguration()
		{
			IsLoading = true;
			Configuration newConfig = null;
			if (File.Exists(ConfigPath)) {
				using (var sr = new StreamReader (ConfigPath))
				using (var jsonTextReader = new JsonTextReader (sr)) {
					newConfig = new JsonSerializer ().Deserialize<Configuration> (jsonTextReader);
				}
			}
			IsLoading = false;
			if (newConfig == null) {
				newConfig = new Configuration ();
			}
			return newConfig;
		}

		public static void SaveConfiguration(Configuration config)
		{
			if (IsLoading) {
				return;
			}
			string json = JsonConvert.SerializeObject (config, Formatting.Indented);
			File.WriteAllText (ConfigPath, json);
		}

		public static string DefaultMusicPath 
		{
			get {
				string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
				                 Environment.OSVersion.Platform == PlatformID.MacOSX)
				? Environment.GetEnvironmentVariable ("HOME")
				: Environment.ExpandEnvironmentVariables ("%HOMEDRIVE%%HOMEPATH%");
				return Path.Combine (homePath, "Music");
			}
		}

		public enum SyncTypeEnum
		{
			UploadOnly = 1,
			DownloadOnly = 2,
			TwoWay = 3
		};

		public Configuration()
		{
			_SyncDirectory = DefaultMusicPath;
		}

		private AuthResponse _Authentication;
		[JsonProperty("Authentication")]
		public AuthResponse Authentication
		{
			get {
				return _Authentication;
			}
			set {
				_Authentication = value;
				SaveConfiguration(this);
			}
		}

		private string _SyncDirectory;
		[JsonProperty("SyncDirectory")]
		public string SyncDirectory
		{ 
			get {
				return _SyncDirectory;
			}
			set {
				_SyncDirectory = value;
				SaveConfiguration (this);
			}
		}

		private SyncTypeEnum _SyncType;
		[JsonProperty("SyncType")]
		public SyncTypeEnum SyncType
		{
			get { 
				return _SyncType;
			}
			set {
				_SyncType = value;
				SaveConfiguration (this);
			}
		}
	}
}


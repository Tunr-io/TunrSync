using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TunrSync.Models
{
	public class Song
	{
		/// <summary>
		/// I set the rowkey
		/// </summary>
		[JsonProperty("songId")]
		public Guid SongId { get; set; }

		/// <summary>
		/// Audio data fingerprint of the audio file.
		/// </summary>
		public string Fingerprint { get; set; }

		/// <summary>
		/// MD5 hash of the first few KB of the file. Used to prevent duplicates.
		/// </summary>
		[JsonProperty("md5Hash")]
		public string Md5Hash { get; set; }

		/// <summary>
		/// Full name of the file originally uploaded.
		/// </summary>
		[JsonProperty("fileName")]
		public string FileName { get; set; }

		/// <summary>
		/// Type of this file - 'mp3', 'flac', etc.
		/// </summary>
		[JsonProperty("fileType")]
		public string FileType { get; set; }

		/// <summary>
		/// Size of the file in bytes.
		/// </summary>
		[JsonProperty("fileSize")]
		public long FileSize { get; set; }

		/// <summary>
		/// Number of audio channels.
		/// </summary>
		[JsonProperty("audioChannels")]
		public int AudioChannels { get; set; }

		/// <summary>
		/// Bitrate of the audio.
		/// </summary>
		[JsonProperty("audioBitrate")]
		public int AudioBitrate { get; set; }

		/// <summary>
		/// Sample rate of the audio.
		/// </summary>
		[JsonProperty("audioSampleRate")]
		public int AudioSampleRate { get; set; }

		/// <summary>
		/// Duration of the song in seconds.
		/// </summary>
		[JsonProperty("duration")]
		public double Duration { get; set; }

		/// <summary>
		/// Tag: Track title.
		/// </summary>
		[JsonProperty("tagTitle")]
		public string TagTitle { get; set; }

		/// <summary>
		/// Tag: Track album.
		/// </summary>
		[JsonProperty("tagAlbum")]
		public string TagAlbum { get; set; }

		/// <summary>
		/// Tag: List of track performers.
		/// </summary>
		[JsonProperty("tagPerformers")]
		public List<string> TagPerformers { get; set; }

		/// <summary>
		/// Tag: List of track album artists.
		/// </summary>
		[JsonProperty("tagAlbumArtists")]
		public List<string> TagAlbumArtists { get; set; }

		/// <summary>
		/// Tag: List of track composers.
		/// </summary>
		[JsonProperty("tagComposers")]
		public List<string> TagComposers { get; set; }

		/// <summary>
		/// Tag: List of track genres.
		/// </summary>
		[JsonProperty("tagGenres")]
		public List<string> TagGenres { get; set; }

		/// <summary>
		/// Tag: Track year.
		/// </summary>
		[JsonProperty("tagYear")]
		public int TagYear { get; set; }

		/// <summary>
		/// Tag: Track number.
		/// </summary>
		[JsonProperty("tagTrack")]
		public int TagTrack { get; set; }

		/// <summary>
		/// Tag: Total album track count.
		/// </summary>
		[JsonProperty("tagTrackCount")]
		public int TagTrackCount { get; set; }

		/// <summary>
		/// Tag: Track disc number.
		/// </summary>
		[JsonProperty("tagDisc")]
		public int TagDisc { get; set; }

		/// <summary>
		/// Tag: Total album disc count.
		/// </summary>
		[JsonProperty("tagDiscCount")]
		public int TagDiscCount { get; set; }

		/// <summary>
		/// Tag: Track comment.
		/// </summary>
		[JsonProperty("tagComment")]
		public string TagComment { get; set; }

		/// <summary>
		/// Tag: Track lyrics.
		/// </summary>
		[JsonProperty("tagLyrics")]
		public string TagLyrics { get; set; }

		/// <summary>
		/// Tag: Track conductor.
		/// </summary>
		[JsonProperty("tagConductor")]
		public string TagConductor { get; set; }

		/// <summary>
		/// Tag: Track BPM.
		/// </summary>
		[JsonProperty("tagBeatsPerMinute")]
		public int TagBeatsPerMinute { get; set; }

		/// <summary>
		/// Tag: Track grouping.
		/// </summary>
		[JsonProperty("tagGrouping")]
		public string TagGrouping { get; set; }
	}
}


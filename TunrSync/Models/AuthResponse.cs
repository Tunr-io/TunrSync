using System;

namespace TunrSync.Models
{
	public class AuthResponse
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
		public long expires_in { get; set; }
		public string userName { get; set; }
		public Guid Id { get; set; }
		public string DisplayName { get; set; }
	}
}


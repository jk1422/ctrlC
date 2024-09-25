using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctrlC.Utils.DiscordUtils
{
	[Serializable]
	public class DiscordMessage
	{
		public int type { get; set; }
		public string content { get; set; }

		public string id { get; set; }
		public string channel_id { get; set; }
		public bool pinned { get; set; }
		public bool mention_everyone { get; set; }
		public bool tts { get; set; }
	}


	[Serializable]
	public class TokenData
	{
		public string access_token { get; set; }
		public string refresh_token { get; set; }
		public int expires_in { get; set; }
		public bool is_validated { get; set; }
		public string token_type { get; set; }
	}

	[Serializable]
	public class TokenResponse
	{
		public string access_token { get; set; }
		public string refresh_token { get; set; }
		public int expires_in { get; set; }
		public bool is_validated { get; set; }
		public string token_type { get; set; }
	}
	[Serializable]
	public class DiscordMember
	{
		public string avatar { get; set; }
		public string communication_disabled_until { get; set; } // DateTime or string, depending on your requirements
		public int flags { get; set; }
		public DateTime joined_at { get; set; } // If you prefer to store this as a DateTime
		public string nick { get; set; }
		public bool pending { get; set; }
		public string premium_since { get; set; } // DateTime or string, depending on your requirements
		public List<string> roles { get; set; }
		public string unusual_dm_activity_until { get; set; } // DateTime or string, depending on your requirements
		public DiscordUser user { get; set; }
		public bool mute { get; set; }
		public bool deaf { get; set; }
	}

	[Serializable]
	public class DiscordUser
	{
		public string id { get; set; }
		public string username { get; set; }
		public string avatar { get; set; }
		public string discriminator { get; set; }
		public int public_flags { get; set; }
		public int flags { get; set; }
		public string banner { get; set; }
		public string accent_color { get; set; }
		public string global_name { get; set; }
		public string avatar_decoration_data { get; set; }
		public string banner_color { get; set; }
		public string clan { get; set; }
	}
}

using Newtonsoft.Json;
using System.IO;

namespace Frequency3.Core
{
	public abstract class BotConfiguration
	{
		private static BotConfiguration? config = null;

		public abstract string RichPresence { get; }
		public abstract string Token { get; }
		public abstract string ApexApiKey { get; }
		public abstract string FortniteApiKey { get; }
		public abstract string WolframApiKey { get; }
		public abstract LavaLinkConfiguration LavaLinkSettings { get; }
		public static BotConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = GetConfig();
				}
				return config;
			}
		}

		private static BotConfiguration GetConfig()
		{
			string a = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "/Configuration.json";

			return JsonConvert.DeserializeObject<_config>(File.ReadAllText(a));
		}
		public static BotConfiguration ReloadConfig()
		{
			config = GetConfig();
			return config;
		}

		private class _config : BotConfiguration
		{
			[JsonProperty("Token")]
			private readonly string Token_ = "";
			[JsonProperty("RichPresence")]
			private readonly string RichPresence_ = "";
			[JsonProperty("FortniteApiKey")]
			private readonly string FortniteApiKey_ = "";
			[JsonProperty("WolframApiKey")]
			private readonly string WolframApiKey_ = "";
			[JsonProperty("ApexApiKey")]
			private readonly string ApexApiKey_ = "";
			[JsonProperty("LavaLinkSettings")]
			private readonly LavaLinkConfiguration.lavalinkconfig? Lavalinkconfig_;

			[JsonIgnore]
			public override string RichPresence { get => RichPresence_; }
			[JsonIgnore]
			public override string Token { get => Token_; }
			[JsonIgnore]
			public override string FortniteApiKey { get => FortniteApiKey_; }
			[JsonIgnore]
			public override string ApexApiKey { get => ApexApiKey_; }
			[JsonIgnore]
			public override string WolframApiKey { get => WolframApiKey_; }
			[JsonIgnore]
			public override LavaLinkConfiguration? LavaLinkSettings { get => Lavalinkconfig_; }
		}

	}
}

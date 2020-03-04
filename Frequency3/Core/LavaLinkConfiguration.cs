using Newtonsoft.Json;

namespace Frequency3.Core
{
	public abstract class LavaLinkConfiguration
	{
		public abstract string Host { get; }
		public abstract ushort Port { get; }
		public abstract string Password { get; }

		internal class lavalinkconfig : LavaLinkConfiguration
		{
			[JsonProperty("Host")]
			private readonly string _Host = "";
			[JsonProperty("Port")]
			private readonly ushort _Port = 0;
			[JsonProperty("Password")]
			private readonly string _Password = "";

			[JsonIgnore]
			public override string Host => _Host;
			[JsonIgnore]
			public override ushort Port => _Port;
			[JsonIgnore]
			public override string Password => _Password;
		}
	}
}

using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace KeePassXC_API.Messages
{
    abstract class Message
	{

		[JsonIgnore]
		public Actions Action { get; protected set; }

		/// <summary>
		/// use Message.Action
		/// </summary>
		[JsonPropertyName("action")]
		public string action { get { return Action.Type; } set { Action = Actions.TryConvert(value); } }

		[JsonPropertyName("nonce")]
		public string _nonce { get; set; }
		[JsonIgnore]
		public byte[] Nonce { get { return _nonce.FromBase64(); } set { _nonce = value.ToBase64(); } }

		[JsonPropertyName("clientID")]
		public string ClientId { get; set; }

		public Message() {}
	}

	static class BigIntExt
	{
		public static string ToBase64(this BigInteger num)
		{
			byte[] ar = num.ToByteArray();
			return Convert.ToBase64String(ar);
		}
	}
}

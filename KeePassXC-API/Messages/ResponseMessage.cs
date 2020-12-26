using System;
using System.Text.Json.Serialization;

namespace KeePassXC_API.Messages
{
    class ResponseMessage : Message
	{
		[JsonIgnore]
		public const string SupportedVersion = "2.6.2";

		[JsonPropertyName("version")]
		public string Version { get { return _version; } set { if (SupportedVersion != value) { throw new InvalidCastException("wrong version"); } _version = value; } }
		[JsonIgnore]
		private string _version;

		[JsonPropertyName("error")]
		public string Error { get; set; } = null;

		[JsonIgnore]
		public bool IsError { get { return !string.IsNullOrEmpty(Error) || ErrorCode != 0; } }

		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		[JsonPropertyName("errorCode")]
		public int ErrorCode { get; set; } = 0;

		public ResponseMessage() : base() { }
	}
}

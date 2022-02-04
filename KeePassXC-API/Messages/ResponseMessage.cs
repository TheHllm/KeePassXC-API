using System;
using System.Text.Json.Serialization;

namespace KeePassXC_API.Messages
{
    class ResponseMessage : Message
	{
		[JsonIgnore]
		public static readonly Version SupportedVersion = new Version(2, 6, 2);

		[JsonPropertyName("version")]
		public string Version 
		{ 
			get { return _version; } 
			set {
				_version = value;
				VersionNumber = new Version(value.Split('-')[0]);
				if (SupportedVersion > VersionNumber)
				{
					throw new InvalidCastException("wrong version");
				}
			} 
		}
		[JsonIgnore]
		private string _version;

		[JsonIgnore]
		public Version VersionNumber { get; private set; }



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

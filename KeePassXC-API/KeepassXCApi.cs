using KeePassXC_API.Messages;
using NeoSmart.AsyncLock;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeePassXC_API
{
    public class KeepassXCApi : IDisposable
	{
		private AsyncLock communicationLock { get; } = new();
		private CommunicationHelper communicatior { get; init; }
		private IDatabaseInformationSaver Saver { get; init; }

		private DatabaseInformation[] databases { get; set; }

		private bool Associated { get; set; } = false;

		public KeepassXCApi() : this(new DefaultDatabaseInformationSaver()) {}

		public KeepassXCApi(IDatabaseInformationSaver saver)
        {
			try
			{
				Saver = saver;
				communicatior = new();
			}
			catch
            {
				((IDisposable)this).Dispose();
				throw;
            }
		}


		private class TestAssocicateMessage : Message
		{
			[JsonPropertyName("id")]
			public string DatabaseHash { get; set; }

			[JsonPropertyName("key")]
			public string Key { get; set; }

			public TestAssocicateMessage(string key, string hash)
			{
				Action = Actions.TestAssociate;
				DatabaseHash = hash;
				Key = key;
			}
		}
		private class TestAssocicateResponseMessage : ResponseMessage
		{
			[JsonPropertyName("hash")]
			public string Hash { get; set; }
			[JsonPropertyName("id")]
			public string Name { get; set; }
		}
		private class AssociateResponse : HashMessage {[JsonPropertyName("id")] public string Id { get; set; } }

		/// <summary>
		/// You don't need to call this, if it is needed it will be called automatically.
		/// </summary>
		public async Task AssociateIfNeeded(bool tryUnlockIfClosed=true)
        {
			if (!Associated)
			{
				//Try to load -> if failed associate
				try
				{
					databases = await Saver.LoadAsync();
					if (databases.Length == 0)
					{
						throw new KeePassXCException();
					}

					//Try test-associate -> if failed associate
					using IDisposable ll = await communicationLock.LockAsync();
					foreach (DatabaseInformation db in databases)
					{
						await communicatior.SendEncrypted(new TestAssocicateMessage(db.ClientIdentificationKey, db.ClientName)); 
						try
						{
							TestAssocicateResponseMessage res = await communicatior.ReadEncrypted<TestAssocicateResponseMessage>(Actions.TestAssociate);
                        }
                        catch (KXCDatabaseNotOpenException)
                        {
                            if (tryUnlockIfClosed)
                            {
								await this.UnlockDatabase();
								await communicatior.SendEncrypted(new TestAssocicateMessage(db.ClientIdentificationKey, db.ClientName));
								TestAssocicateResponseMessage res = await communicatior.ReadEncrypted<TestAssocicateResponseMessage>(Actions.TestAssociate);
							}
                            else
                            {
								throw;
                            }
                        }
					}

					Associated = true;
					return;
				}
				catch (KeePassXCException) { }

				//associate
				using IDisposable l = await communicationLock.LockAsync();
				string clientIdentificationKey = await communicatior.SendAssociateMessage();
				AssociateResponse resp = await communicatior.ReadEncrypted<AssociateResponse>(Actions.Associate);
				databases = new DatabaseInformation[] { new DatabaseInformation(clientIdentificationKey, resp.Id) };

				await Saver.SaveAsync(databases);
				Associated = true;
			}
		}

		private class GetLoginMessage : Message
        {
			[JsonPropertyName("url")]
			public string Url { get; set; }

			[JsonPropertyName("keys")]
			public DatabaseInformation[] Databases { get; set; }
			
			public GetLoginMessage()
            {
				Action = Actions.GetLogins;
            }
		}
		private class GetLoginsResponse : ResponseMessage
        {
			[JsonPropertyName("entries")]
			public AccountInformation[] Logins { get; set; }
        }
		public async Task<AccountInformation[]> GetLogins(string url)
        {
			await AssociateIfNeeded();
            GetLoginMessage msg = new GetLoginMessage()
			{
				Databases = databases,
				Url = url
				
			};
			using IDisposable l = await communicationLock.LockAsync();
			await communicatior.SendEncrypted(msg);
            GetLoginsResponse resp = await communicatior.ReadEncrypted<GetLoginsResponse>(Actions.GetLogins);
			return resp.Logins;
        }

		class GeneratePasswordResponse : ResponseMessage
		{
			[JsonPropertyName("entries")]
			public PasswordObject[] Entries { get; set; }
			public GeneratePasswordResponse() { }
		}
		class PasswordObject
		{
			[JsonPropertyName("password")]
			public string Password { get; set; }

			public PasswordObject() { }
		}
		public async Task<string> GeneratePassword()
		{
			using IDisposable l = await communicationLock.LockAsync();
			await communicatior.SendMessage(new BasicMessage(Actions.GeneratePassword));
			return (await communicatior.ReadEncrypted<GeneratePasswordResponse>(Actions.GeneratePassword)).Entries[0].Password;
		}

		public async Task LockDatabase()
		{
			using IDisposable l = await communicationLock.LockAsync();
			await communicatior.SendEncrypted(new BasicMessage(Actions.LockDatabase));
			ResponseMessage msg = await communicatior.ReadMessage<ResponseMessage>(Actions.DatabaseLocked);
		}

		public async Task UnlockDatabase()
		{
			using IDisposable l = await communicationLock.LockAsync();
			await communicatior.SendEncrypted(new BasicMessage(Actions.GetDatabaseHash), true);
			ResponseMessage msg = await communicatior.ReadMessage<ResponseMessage>(null, waitForUnlook: true);
			if (msg.Action != Actions.DatabaseUnlocked && msg.Action != Actions.GetDatabaseHash)
				throw new KXCWrongMessageException();
		}

		private class HashMessage : ResponseMessage {[JsonPropertyName("hash")] public string Hash { get; set; } }
		public async Task<string> GetDatabaseHash()
		{
			using IDisposable l = await communicationLock.LockAsync();
			await communicatior.SendEncrypted(new BasicMessage(Actions.GetDatabaseHash));
			return (await communicatior.ReadEncrypted<HashMessage>(Actions.GetDatabaseHash)).Hash;
		}

        void IDisposable.Dispose()
        {
            ((IDisposable)communicatior)?.Dispose();
        }
    }
}

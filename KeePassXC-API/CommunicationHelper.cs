using KeePassXC_API.Messages;
using NaCl;
using NativeMessaging;
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeePassXC_API
{
    class CommunicationHelper : IDisposable
    {
        /// <summary>
        /// clientID field send with every message, is unique to each session
        /// </summary>
        private string clientId { get; }
        private byte[] clientPublicKey { get; } = new byte[32];
        private byte[] clientPrivateKey { get; } = new byte[32];
        private BigInteger nonce { get; set; }

        private Client connection { get; set; }
        private Curve25519XSalsa20Poly1305 cryptoBox { get; set; }

        public CommunicationHelper()
        {
            try
            {
                //Generate Crypto stuff
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    clientId = rng.GetBytes(24).ToBase64();
                    nonce = new BigInteger(rng.GetBytes(24));
                    //generate public and private key
                    Curve25519XSalsa20Poly1305.KeyPair(clientPrivateKey, clientPublicKey);

                    //start the key exchange
                    ExchangeKeys().Wait();
                }
            }
            catch (AggregateException e)
            {
                ((IDisposable)this).Dispose();
                throw e.InnerException;
            }
            catch
            {
                ((IDisposable)this).Dispose();
                throw;
            }
        }

        class ExchangeKeysMessage : Message
        {
            [JsonPropertyName("publicKey")]
            public string _publicKey { get { return PublicKey.ToBase64(); } set { PublicKey = value.FromBase64(); } }
            [JsonIgnore]
            public byte[] PublicKey;

            public ExchangeKeysMessage(string clientId, byte[] publicKey) : base()
            {
                Action = Actions.ExchangePublicKeys;
                PublicKey = publicKey;
            }
        }
        class KeyExchangeResponseMessage : ResponseMessage
        {
            [JsonPropertyName("publicKey")]
            public string _publicKey { get; set; }
            [JsonIgnore]
            public byte[] PublicKey { get { return _publicKey.FromBase64(); } }

            public KeyExchangeResponseMessage() { }
        }
        private async Task ExchangeKeys()
        {
            connection = await Client.GetByName("org.keepassxc.keepassxc_browser");
            await SendMessage(new ExchangeKeysMessage(clientId, clientPublicKey));
            KeyExchangeResponseMessage kexmsg = await ReadMessage<KeyExchangeResponseMessage>(Actions.ExchangePublicKeys, TimeSpan.FromSeconds(5));
            cryptoBox = new Curve25519XSalsa20Poly1305(clientPrivateKey, kexmsg.PublicKey);
        }


        private class AssociateMessage : Message
        {
            /// <summary>
            /// Clients public key
            /// </summary>
            [JsonPropertyName("key")]
            public string Key { get; set; }

            /// <summary>
            /// Identification public key
            /// </summary>
            [JsonPropertyName("idKey")]
            public string IdKey { get; set; }

            public AssociateMessage()
            {
                Action = Actions.Associate;
            }
        }
        public async Task<string> SendAssociateMessage()
        {
            string idKey = clientPublicKey.ToBase64();

            AssociateMessage asocMsg = new AssociateMessage()
            {
                Key = clientPublicKey.ToBase64(), 
                IdKey = idKey
            };
            await SendEncrypted(asocMsg);

            return idKey;
        }

        private class EncryptedResponse : ResponseMessage
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
        public async Task<T> ReadEncrypted<T>(Actions actionType, bool waitForUnlock = false) where T : ResponseMessage
        {
            //read outer message
            EncryptedResponse enc = await ReadMessage<EncryptedResponse>(actionType, waitForUnlook: waitForUnlock);
            //decrypt the 'message' field
            byte[] msg = enc.Message.FromBase64();
            byte[] plain = new byte[msg.Length - Curve25519XSalsa20Poly1305.TagLength];
            bool isVerified = cryptoBox.TryDecrypt(plain, msg, enc.Nonce);
            if (!isVerified)
            {
                throw new KXCCannotDecryptException();
            }
#if DEBUG
            string json = Encoding.UTF8.GetString(plain);
#endif
            //convert to object and return
            return JsonSerializer.Deserialize<T>(plain);
        }

        private class EncryptedMessage<T> : Message where T : Message
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("triggerUnlock")]
            public string TriggerUnlock { get; set; }
            public EncryptedMessage(T inner, Curve25519XSalsa20Poly1305 box, byte[] nonce) : base()
            {
                this.ClientId = inner.ClientId;
                this.Nonce = nonce;
                this.Action = inner.Action;
                inner.Nonce = nonce;
                byte[] msgP = JsonSerializer.SerializeToUtf8Bytes(inner);
                byte[] msgE = new byte[msgP.Length + Curve25519XSalsa20Poly1305.TagLength];
                box.Encrypt(msgE, msgP, nonce);

                this.Message = msgE.ToBase64();
            }
        }
        public async Task SendEncrypted<T>(T msg, bool triggerUnlock = false) where T : Message
        {
            await SendMessage(new EncryptedMessage<T>(msg, cryptoBox, nonce.ToByteArray()) { TriggerUnlock = triggerUnlock ? "true" : null  });
        }


        /// <summary>
        /// Waits for a message and checks it for errors and given type.
        /// </summary>
        public async Task<T> ReadMessage<T>(Actions type, TimeSpan? timeOut = null, bool waitForUnlook = false) where T : ResponseMessage
        {
            while (true)
            {
                T msg;
                if (timeOut != null)
                {
                    var readTask = connection.ReadMessage<T>();
                    if (!readTask.Wait(timeOut.Value))
                        throw new KXCTimeoutException();
                    msg = readTask.Result;
                }
                else
                {
                    msg = await connection.ReadMessage<T>();
                }
                if (msg.IsError)
                {
                    if (msg.ErrorCode == 1 && waitForUnlook)
                        continue;
                    throw KeePassXCException.Exceptions[msg.ErrorCode];
                }
                else if (type != null && msg.Action != type)
                {
                    throw new KXCWrongMessageException();
                }
                else
                {
                    return msg;
                }
            }
        }

        /// <summary>
        /// Sends a message, automatically handles the nonce and clientId.
        /// </summary>
        public async Task SendMessage<T>(T msg) where T : Message
        {
            msg.ClientId = clientId;
            msg.Nonce = nonce.ToByteArray();
            await connection.SendMessage(msg);
            nonce = nonce++;
        }

        void IDisposable.Dispose()
        {
            cryptoBox?.Dispose();
            ((IDisposable)connection)?.Dispose();
        }
    }

}

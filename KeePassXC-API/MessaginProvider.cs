using NativeMessaging;
using System;
using System.Threading.Tasks;

namespace KeePassXC_API
{
    class MessaginProvider : IDisposable
    {
        private Task<Client> _client = Client.GetByName("org.keepassxc.keepassxc_browser");
        private Client Client { get { if (!_client.IsCompleted) { _client.Wait(); } return _client.Result; } }

        public const int KeySize = 24;

        public MessaginProvider(){}
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

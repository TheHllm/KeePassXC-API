using NUnit.Framework;
using KeePassXC_API;
using System.Threading.Tasks;

namespace Tests
{
    public class TestGets
    {
        private KeepassXCApi api;

        [SetUp]
        public void Setup()
        {
            api = new();
        }

        [Test, Order(0)]
        public async Task TestAssoc()
        {
            await api.AssociateIfNeeded();
        }

        [Test]
        public async Task TestGetLogins()
        {
            var login = await api.GetLogins("https://example.com");
            Assert.IsTrue(login.Length > 0);
        }

        [Test]
        public async Task TestGenPassword()
        {
            Assert.IsNotEmpty(await api.GeneratePassword());
        }
        [Test]
        public async Task TestGetHash()
        {
            Assert.IsNotEmpty(await api.GetDatabaseHash());
        }

        [Ignore("Will lock your db"), Test, Order(100)]
        public async Task TestLock()
        {
            await api.LockDatabase();
        }
    }
}
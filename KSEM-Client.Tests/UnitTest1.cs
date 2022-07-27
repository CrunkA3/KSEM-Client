using KSEM_Client;

namespace KSEM_Client.Tests
{
    [TestClass]
    public class UnitTest1
    {
        KSEMClient? client;

        [AssemblyInitialize]
        public async Task Init()
        {
            client = new KSEMClient("http://ksem-76555758");
            await client.LoginAsync("password");
        }



        [TestMethod]
        public async Task TestDeviceStatus()
        {
            var status = await client!.GetDeviceStatusAsync();
            Assert.IsNotNull(status);
        }
    }
}
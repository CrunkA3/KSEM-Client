using KSEM_Client;

namespace KSEM_Client.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private static KSEMClient? client;
        private static TestContext? testContext;

        [AssemblyInitialize]
        public static async Task Init(TestContext testContext)
        {
            UnitTest1.testContext = testContext;
            client = new KSEMClient("http://ksem-76555758");

            var password = testContext.Properties["KsemPassword"]?.ToString();
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password), "Please set 'KsemPassword' in your .runsettings file");
            await client.LoginAsync(password);
        }



        [TestMethod]
        public async Task TestDeviceStatus()
        {
            var status = await client!.GetDeviceStatusAsync();
            Assert.IsNotNull(status);
        }
    }
}
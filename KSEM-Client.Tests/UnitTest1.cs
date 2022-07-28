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
            var password = testContext.Properties["KsemPassword"]?.ToString();
            var host = testContext.Properties["KsemHost"]?.ToString();

            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password), "Please set 'KsemPassword' in your .runsettings file");
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(password), "Please set 'KsemHost' in your .runsettings file");

            UnitTest1.testContext = testContext;
            client = new KSEMClient(host);


            await client.LoginAsync(password);
        }



        [TestMethod]
        public async Task TestDeviceStatus()
        {
            var status = await client!.GetDeviceStatusAsync();
            Assert.IsNotNull(status);
        }



        [TestMethod]
        public async Task TestSocket()
        {
            //http://ksem-76555758/api/data-transfer/protobuf/gdr/local/config/smart-meter
            var cancellationTokenSource = new CancellationTokenSource();

            await client!.StartSocketAsync(cancellationTokenSource.Token);
        }
    }
}
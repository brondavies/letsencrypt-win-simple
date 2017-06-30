using letsencrypt;
using letsencrypt.Support;
using letsencrypt_tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace letsencrypt_tests.Tests
{
    /// <summary>
    /// This class is used mainly to test error conditions since the AzureRestApi is covered in AzureWebAppPluginTests
    /// </summary>
    [TestClass]
    [DeploymentItem("localhost22233-all.pfx")]
    public class AzureRestApiTests : TestBase
    {
        private Options options = new Options {
            Test = true
        };
        private string hostName = "localhost";
        private string pfxFilename = Path.Combine(Plugin.BaseDirectory, "localhost22233-all.pfx");
        private string subscriptionId = "test-subscription-id";
        private JToken webApp = new JObject
        {
            ["id"] = "/1234",
            ["location"] = "test-location",
            ["properties"] = new JObject
            {
                ["name"] = "test",
                ["resourceGroup"] = "Default",
                ["serverFarmId"] = "test-serverFarmId"
            }
        };

        [TestInitialize]
        public override void Initialize()
        {
            StartHTTPProxy = true;
            AzureRestApi.ApiRootUrl =
            AzureRestApi.AuthRootUrl = removeLastSlash(ProxyUrl("/"));
            base.Initialize();
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.WebException))]
        public void AzureRestApi_GetPublishingCredentialsTest()
        {
            MockResponse("/1234/publishxml", new MockHttpResponse
            {
                StatusCode = "500",
                StatusDescription = "Internal Server Error",
                ResponseBody = "null"
            });

            AzureRestApi.GetPublishingCredentials("test-token", "/1234");
        }

        [TestMethod]
        public void AzureRestApi_InstallCertificateTest()
        {
            MockResponse("/subscriptions/test-subscription-id/resourceGroups/Default/providers/Microsoft.Web/certificates/test", new MockHttpResponse
            {
                StatusCode = "500",
                StatusDescription = "Internal Server Error",
                ResponseBody = "null"
            });

            AzureRestApi.InstallCertificate(options, "test-token", subscriptionId, hostName, webApp, pfxFilename);
        }

        [TestMethod]
        public void AzureRestApi_SetCertificateHostNameTest()
        {
            MockResponse("/1234\\?api-version=2016-08-01", new MockHttpResponse
            {
                StatusCode = "500",
                StatusDescription = "Internal Server Error",
                ResponseBody = "null"
            });

            AzureRestApi.SetCertificateHostName("test-token", subscriptionId, hostName, webApp, "ABCDEF123456");
        }
    }
}

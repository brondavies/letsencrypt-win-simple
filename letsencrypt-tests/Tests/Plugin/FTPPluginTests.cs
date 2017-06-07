using Microsoft.VisualStudio.TestTools.UnitTesting;
using letsencrypt;
using System;

using letsencrypt_tests.Support;
using letsencrypt.Support;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace letsencrypt_tests
{
    [TestClass()]
    [DeploymentItem("ACMESharp.PKI.Providers.OpenSslLib32.dll")]
    [DeploymentItem("ACMESharp.PKI.Providers.OpenSslLib64.dll")]
    [DeploymentItem("FTP.json")]
    [DeploymentItem("FTP-without-path.json")]
    [DeploymentItem("localhost22233-all.pfx")]
    [DeploymentItem("ManagedOpenSsl.dll")]
    [DeploymentItem("ManagedOpenSsl64.dll")]
    [DeploymentItem("Registration")]
    [DeploymentItem("Signer")]
    [DeploymentItem("test-cert.der")]
    [DeploymentItem("test-cert.pem")]
    [DeploymentItem("web_config.xml")]
    [DeploymentItem("x64\\libeay32.dll", "x64")]
    [DeploymentItem("x64\\ssleay32.dll", "x64")]
    [DeploymentItem("x86\\libeay32.dll", "x86")]
    [DeploymentItem("x86\\ssleay32.dll", "x86")]
    public class FTPPluginTests : TestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            StartHTTPProxy = true;
            StartFTPProxy = true;
            AllowInsecureSSLRequests = true;
            base.Initialize();
        }

        private void CreatePlugin(out FTPPlugin plugin, out Options options)
        {
            plugin = new FTPPlugin();
            AzureRestApi.ApiRootUrl =
            AzureRestApi.AuthRootUrl = removeLastSlash(ProxyUrl("/"));
            options = MockOptions();
            options.Plugin = R.FTP;
            options.CertOutPath = options.ConfigPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        [TestMethod]
        public void FTPPlugin_ValidateTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsFalse(plugin.RequiresElevated);
            Assert.IsTrue(plugin.Validate(options));
        }

        [TestMethod]
        public void FTPPlugin_GetSelectedTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.GetSelected(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false)));
            Assert.IsFalse(plugin.GetSelected(new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false)));
        }

        [TestMethod]
        public void FTPPlugin_SelectOptionsTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.Validate(options);
            Assert.IsTrue(plugin.SelectOptions(options));
        }

        [TestMethod]
        public void FTPPlugin_SelectOptionsFailsTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            options.PluginConfig = "EmptyConfig.json";
            plugin.Validate(options);
            AssertThrows<ArgumentNullException>(() =>
            {
                plugin.SelectOptions(options);
            });
            options.PluginConfig = "FTP-without-path.json";
            plugin.Validate(options);
            AssertThrows<ArgumentNullException>(() =>
            {
                plugin.SelectOptions(options);
            });
        }

        [TestMethod]
        public void FTPPlugin_DeleteAuthorizationTest()
        {
            FTPPlugin plugin;
            Options options;
            var webRoot = "/site/wwwroot";
            var token = "this-is-a-test-token";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var challengeFile = $"{MockFtpServer.localPath}{webRoot}{challengeLocation}".Replace('/', Path.DirectorySeparatorChar);
            Directory.CreateDirectory(Path.GetDirectoryName(challengeFile));
            File.WriteAllText(challengeFile, token);
            var rootPath = $"{FTPServerUrl}{webRoot}";
            CreatePlugin(out plugin, out options);
            options.CleanupFolders = true;
            plugin.FtpCredentials = new System.Net.NetworkCredential("testuser", "testpassword");
            plugin.DeleteAuthorization(options, rootPath + challengeLocation, token, webRoot, challengeLocation);
            Assert.IsFalse(File.Exists(challengeFile));
        }

        [TestMethod]
        public void FTPPlugin_InstallTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            options.BaseUri = ProxyUrl("/");
            plugin.client = MockAcmeClient(options);
            var target = new Target
            {
                Host = HTTPProxyServer,
                PluginName = R.FTP,
                WebRootPath = $"{FTPServerUrl}/site/wwwroot/"
            };
            plugin.hostName = HTTPProxyServer;
            Directory.CreateDirectory(Path.Combine(MockFtpServer.localPath, "site", "wwwroot"));
            plugin.Renew(target, options);
            plugin.FtpCredentials = new System.Net.NetworkCredential("test", "test");
            plugin.Renew(target, options);
        }

        [TestMethod]
        public void FTPPlugin_GetTargetsTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            var targets = plugin.GetTargets(options);

            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual(R.FTP, targets[0].PluginName);
        }

        [TestMethod]
        public void FTPPlugin_PrintMenuTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.PrintMenu();
        }

        [TestMethod]
        public void FTPPlugin_BeforeAuthorizeTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);

            plugin.FtpCredentials = new System.Net.NetworkCredential("testuser", "testpassword");
            var webRoot = "/site/wwwroot";
            var token = "this-is-a-test";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var rootPath = $"{FTPServerUrl}{webRoot}";
            var target = new Target
            {
                PluginName = R.FTP,
                WebRootPath = rootPath
            };
            plugin.BeforeAuthorize(target, rootPath + challengeLocation, token);
            var webconfigFile = Path.Combine(MockFtpServer.localPath, "site", "wwwroot",".well-known", "acme-challenge", "web.config");
            Assert.IsTrue(File.Exists(webconfigFile));
        }

        [TestMethod]
        public void FTPPlugin_CreateAuthorizationFileTest()
        {
            FTPPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);

            plugin.FtpCredentials = new System.Net.NetworkCredential("testuser", "testpassword");
            var webRoot = "/site/wwwroot";
            var token = "this-is-a-test";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var challengeFile = $"{MockFtpServer.localPath}{webRoot}{challengeLocation}".Replace('/', Path.DirectorySeparatorChar);
            var rootPath = $"{FTPServerUrl}{webRoot}";
            plugin.CreateAuthorizationFile(rootPath + challengeLocation, token);
            Assert.IsTrue(File.Exists(challengeFile));
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using letsencrypt;
using System;

using letsencrypt_tests.Support;
using letsencrypt.Support;
using System.IO;
using System.Reflection;

namespace letsencrypt_tests
{
    [TestClass()]
    [DeploymentItem("ACMESharp.PKI.Providers.OpenSslLib32.dll")]
    [DeploymentItem("ACMESharp.PKI.Providers.OpenSslLib64.dll")]
    [DeploymentItem("IIS.json")]
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
    public class IISPluginTests : TestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            StartHTTPProxy = true;
            StartFTPProxy = false;
            AllowInsecureSSLRequests = true;
            base.Initialize();
        }

        private void CreatePlugin(out IISPlugin plugin, out Options options)
        {
            IISPlugin.RegisterServerManager<MockIISServerManager>();
            plugin = new IISPlugin();
            options = MockOptions();
            options.Plugin = R.IIS;
            options.HideHttps = true;
            options.CertOutPath = options.ConfigPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.IsTrue(plugin.RequiresElevated);
        }

        [TestMethod()]
        public void IISPlugin_ValidateTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.Validate(options));
        }

        [TestMethod()]
        public void IISPlugin_ValidateFailsTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            var oldVersion = MockIISServerManager.MajorVersion;
            MockIISServerManager.MajorVersion = 0;
            Assert.IsFalse(plugin.Validate(options));
            MockIISServerManager.MajorVersion = oldVersion;
        }
        
        [TestMethod()]
        public void IISPlugin_ValidateFails2Test()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            MockIISServerManager.ReturnMockSites = false;

            Assert.IsFalse(plugin.Validate(options));
            MockIISServerManager.ReturnMockSites = true;
        }

        [TestMethod()]
        public void IISPlugin_GetSelectedTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.GetSelected(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false)));
            Assert.IsFalse(plugin.GetSelected(new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false)));
        }

        [TestMethod()]
        public void IISPlugin_SelectOptionsTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.Validate(options);
            Assert.IsTrue(plugin.SelectOptions(options));
        }

        [TestMethod()]
        public void IISPlugin_SelectOptionsFailsTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);

            MockIISServerManager.ReturnMockSites = false;
            options.PluginConfig = "EmptyConfig.json";
            plugin.Validate(options);
            Assert.IsFalse(plugin.SelectOptions(options));
            MockIISServerManager.ReturnMockSites = true;
        }

        [TestMethod()]
        public void IISPlugin_DeleteAuthorizationTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);

            var token = "this-is-a-test";
            var webRoot = "/";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var rootPath = plugin.BaseDirectory;
            var challengeFile = $"{rootPath}{challengeLocation}".Replace('/', Path.DirectorySeparatorChar);

            Directory.CreateDirectory(Path.GetDirectoryName(challengeFile));
            File.WriteAllText(challengeFile, token);
            plugin.DeleteAuthorization(options, rootPath + challengeLocation, token, webRoot, challengeLocation);
            Assert.IsFalse(File.Exists(challengeFile));
        }

        [TestMethod()]
        public void IISPlugin_InstallTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            options.BaseUri = ProxyUrl("/");
            plugin.client = MockAcmeClient(options);
            var target = new Target
            {
                PluginName = R.IIS,
                Host = HTTPProxyServer,
                SiteId = 0,
                WebRootPath = plugin.BaseDirectory
            };
            plugin.Install(target, options);
        }

        [TestMethod()]
        public void IISPlugin_GetTargetsTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            var targets = plugin.GetTargets(options);

            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual(R.IIS, targets[0].PluginName);
            Assert.AreEqual("localhost", targets[0].Host);
        }

        [TestMethod()]
        public void IISPlugin_PrintMenuTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.PrintMenu();
        }

        [TestMethod()]
        public void IISPlugin_BeforeAuthorizeTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            var token = "this-is-a-test";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var rootPath = plugin.BaseDirectory;
            var target = new Target
            {
                PluginName = R.IIS,
                WebRootPath = rootPath,
                Host = HTTPProxyServer
            };
            plugin.BeforeAuthorize(target, rootPath + challengeLocation, token);
            var webconfigFile = Path.Combine(rootPath, ".well-known", "acme-challenge", "web.config");
            Assert.IsTrue(File.Exists(webconfigFile));
        }

        [TestMethod()]
        public void IISPlugin_CreateAuthorizationFileTest()
        {
            IISPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            var token = "this-is-a-test";
            var rootPath = plugin.BaseDirectory;
            var challengeFile = Path.Combine(rootPath, ".well-known", "acme-challenge", token);
            plugin.CreateAuthorizationFile(challengeFile, token);
            Assert.IsTrue(File.Exists(challengeFile));
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
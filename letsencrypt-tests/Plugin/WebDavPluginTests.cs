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
    [DeploymentItem("localhost22233-all.pfx")]
    [DeploymentItem("ManagedOpenSsl.dll")]
    [DeploymentItem("ManagedOpenSsl64.dll")]
    [DeploymentItem("WebDAV.json")]
    [DeploymentItem("Registration")]
    [DeploymentItem("Signer")]
    [DeploymentItem("test-cert.der")]
    [DeploymentItem("test-cert.pem")]
    [DeploymentItem("web_config.xml")]
    [DeploymentItem("x64\\libeay32.dll", "x64")]
    [DeploymentItem("x64\\ssleay32.dll", "x64")]
    [DeploymentItem("x86\\libeay32.dll", "x86")]
    [DeploymentItem("x86\\ssleay32.dll", "x86")]
    public class WebDAVPluginTests : TestBase
    {
        private string proxyWebDavUrl = "/webdav/";

        [TestInitialize]
        public override void Initialize()
        {
            StartHTTPProxy = true;
            StartFTPProxy = false;
            AllowInsecureSSLRequests = true;
            base.Initialize();
        }

        private void CreatePlugin(out WebDAVPlugin plugin, out Options options)
        {
            plugin = new WebDAVPlugin();
            plugin.webDAVClientType = typeof(MockWebDAVClient);
            options = MockOptions();
            options.Plugin = R.WebDAV;
            options.CertOutPath = options.ConfigPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        [TestMethod()]
        public void WebDAVPlugin_ValidateTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.Validate(options));
        }

        [TestMethod()]
        public void WebDAVPlugin_GetSelectedTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.GetSelected(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false)));
            Assert.IsFalse(plugin.GetSelected(new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false)));
        }

        [TestMethod()]
        public void WebDAVPlugin_SelectOptionsTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.WebDAVPath = ProxyUrl(proxyWebDavUrl);
            plugin.Validate(options);
            Assert.IsTrue(plugin.SelectOptions(options));
        }
        
        [TestMethod()]
        public void WebDAVPlugin_InstallTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            options.BaseUri = ProxyUrl(proxyWebDavUrl);
            plugin.client = MockAcmeClient(options);
            var target = new Target
            {
                PluginName = R.WebDAV,
                Host = HTTPProxyServer,
                WebRootPath = plugin.BaseDirectory
            };
            plugin.Install(target, options);
        }

        [TestMethod()]
        public void WebDAVPlugin_GetTargetsTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.hostName = "localhost";
            var rootPath = plugin.WebDAVPath = ProxyUrl(proxyWebDavUrl);
            var targets = plugin.GetTargets(options);

            Assert.AreEqual(targets.Count, 1);
            Assert.AreEqual(targets[0].PluginName, R.WebDAV);
            Assert.AreEqual(targets[0].Host, "localhost");
            Assert.AreEqual(targets[0].WebRootPath, rootPath);
        }

        [TestMethod()]
        public void WebDAVPlugin_PrintMenuTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.PrintMenu();
        }

        [TestMethod()]
        public void WebDAVPlugin_BeforeAuthorizeTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.hostName = HTTPProxyServer;
            
            var token = "this-is-a-test";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var rootPath = plugin.WebDAVPath = ProxyUrl(proxyWebDavUrl);
            var target = new Target
            {
                PluginName = R.WebDAV,
                Host = "localhost",
                WebRootPath = rootPath
            };
            plugin.BeforeAuthorize(target, rootPath + challengeLocation, token);
        }

        [TestMethod()]
        public void WebDAVPlugin_CreateAuthorizationFileTest()
        {
            WebDAVPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.hostName = HTTPProxyServer;
            var rootPath = plugin.BaseDirectory;
            plugin.WebDAVPath = ProxyUrl(proxyWebDavUrl);
            
            var token = "this-is-a-test";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var challengeFile = $"{rootPath}{challengeLocation}".Replace('/', Path.DirectorySeparatorChar);
            plugin.CreateAuthorizationFile(challengeFile, token);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
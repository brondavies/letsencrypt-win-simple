﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using letsencrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using letsencrypt_tests.Support;
using letsencrypt.Support;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Specialized;
using ACMESharp;
using ACMESharp.Messages;
using System.Reflection;
using ACMESharp.JOSE;

namespace letsencrypt_tests
{
    [TestClass()]
    public class AzureWebAppPluginTests : TestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            StartHTTPProxy = true;
            StartFTPProxy = true;
            AllowInsecureSSLRequests = true;
            base.Initialize();
        }

        private void CreatePlugin(out AzureWebAppPlugin plugin, out Options options)
        {
            plugin = new AzureWebAppPlugin();
            AzureRestApi.ApiRootUrl =
            AzureRestApi.AuthRootUrl = ProxyUrl("/");
            options = MockOptions();
            options.Plugin = R.AzureWebApp;
            options.CertOutPath = options.ConfigPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MockResponse("/test-tenant-id/oauth2/token", new MockHttpResponse
            {
                ResponseBody = toJson(new { access_token = "test_token" })
            });
        }

        [TestMethod()]
        public void AzureWebAppPlugin_ValidateTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.Validate(options));
        }

        [TestMethod()]
        public void AzureWebAppPlugin_GetSelectedTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            Assert.IsTrue(plugin.GetSelected(new ConsoleKeyInfo('a', ConsoleKey.Z, false, false, false)));
            Assert.IsFalse(plugin.GetSelected(new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false)));
        }

        [TestMethod()]
        public void AzureWebAppPlugin_SelectOptionsTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            MockResponse(@"/subscriptions\?", new MockHttpResponse
            {
                ResponseBody = toJson(new
                {
                    value = new[] { new {
                    subscriptionId = "test-subscription-id",
                    displayName = "test-subscription"
                } }
                })
            });
            MockResponse(@"/subscriptions/test-subscription-id/resourcegroups\?", new MockHttpResponse
            {
                ResponseBody = toJson(new { value = new[] { new { name = "Default" } } })
            });
            MockResponse("/subscriptions/test-subscription-id/resourcegroups/Default/providers/Microsoft.Web/sites", new MockHttpResponse
            {
                ResponseBody = toJson(new
                {
                    value = new[] {
                        new {
                            name = "test",
                            properties = new {
                                hostNames = new[] { "test.example.com" }
                            }
                        }
                    }
                })
            });
            plugin.Validate(options);
            Assert.IsTrue(plugin.SelectOptions(options));
        }

        //TODO: Skipping this test until the kinks are worked out with the "fake" certificate
        //[TestMethod()]
        public void AzureWebAppPlugin_InstallTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            options.BaseUri = ProxyUrl("/");
            plugin.client = MockAcmeClient(options);
            var target = new Target
            {
                PluginName = R.AzureWebApp
            };
            var id = "subscriptions/test-subscription-id/resourceGroups/Default/providers/Microsoft.Web/sites/test";
            plugin.hostName = "localhost:" + Settings.HTTPProxyPort;
            plugin.webApp = new JObject
            {
                ["id"] = id
            };
            MockResponse($"/{id}/publishxml", new MockHttpResponse
            {
                ResponseBody = toXML(new publishData
                {
                    Items = new[] {
                    new publishDataPublishProfile
                    {
                        publishMethod = "FTP",
                        userName = "testuser",
                        userPWD = "testpassword",
                        publishUrl = $"{FTPServerUrl}/site/wwwroot"
                    } }
                })
            });
            Directory.CreateDirectory(Path.Combine(MockFtpServer.localPath, "site", "wwwroot"));
            plugin.Install(target, options);
        }

        [TestMethod()]
        public void AzureWebAppPlugin_GetTargetsTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            var targets = plugin.GetTargets(options);

            Assert.AreEqual(targets.Count, 1);
            Assert.AreEqual(targets[0].PluginName, R.AzureWebApp);
        }

        [TestMethod()]
        public void AzureWebAppPlugin_PrintMenuTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);
            plugin.PrintMenu();
        }

        [TestMethod()]
        public void AzureWebAppPlugin_DeleteAuthorizationTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            var webRoot = "/deletetest/wwwroot";
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

        [TestMethod()]
        public void AzureWebAppPlugin_BeforeAuthorizeTest()
        {
            AzureWebAppPlugin plugin;
            Options options;
            CreatePlugin(out plugin, out options);

            plugin.FtpCredentials = new System.Net.NetworkCredential("testuser", "testpassword");
            var webRoot = "/site/wwwroot";
            var token = "this-is-a-test";
            var challengeLocation = $"/.well-known/acme-challenge/{token}";
            var rootPath = $"{FTPServerUrl}{webRoot}";
            var target = new Target
            {
                PluginName = R.AzureWebApp,
                WebRootPath = rootPath
            };
            plugin.BeforeAuthorize(target, rootPath + challengeLocation, token);
            var webconfigFile = Path.Combine(MockFtpServer.localPath, "site", "wwwroot",".well-known", "acme-challenge", "web.config");
            Assert.IsTrue(File.Exists(webconfigFile));
        }

        [TestMethod()]
        public void AzureWebAppPlugin_CreateAuthorizationFileTest()
        {
            AzureWebAppPlugin plugin;
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
    }
}
using letsencrypt;
using letsencrypt.Support;
using letsencrypt_tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSSL.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace letsencrypt_tests
{
    [TestClass()]
    public class PluginTests : TestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void RequireNotNull_Test()
        {
            AssertThrows<ArgumentNullException>(() => { Plugin.RequireNotNull("test", ""); });
            AssertThrows<ArgumentNullException>(() => { Plugin.RequireNotNull("test", null); });
            AssertDoesNotThrow<ArgumentNullException>(() => { Plugin.RequireNotNull("test", "notnull"); });
        }

        [TestMethod]
        public void RunScript_Test()
        {
            var target = new Target
            {
            };
            var batfile = Plugin.BaseDirectory + "\\test.bat";
            var store = IISPlugin.OpenCertificateStore("My");
            X509Certificate2 certificate = store.Certificates[0];
            var options = new Options {
                Script = "cmd.exe",
                ScriptParameters = "/c echo {0} {1} {2} {3} {4} {5} > \"" + batfile + "\""
            };
            Plugin.RunScript(target, "test.pfx", store, certificate, options);

            options.Script = batfile;
            options.ScriptParameters = "";
            Plugin.RunScript(target, "test.pfx", store, certificate, options);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}

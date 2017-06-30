using letsencrypt;
using letsencrypt.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letsencrypt_tests.Tests
{
    [TestClass]
    public class LetsEncryptTests
    {
        [TestMethod]
        public void LetsEncrypt_GetContactsTest()
        {
            Assert.IsTrue(
                LetsEncrypt.GetContacts("test@example.com")
                    .SequenceEqual(new string[] { "mailto:test@example.com" }));
        }

        [TestMethod]
        public void LetsEncrypt_GetStringTest()
        {
            var sdict = new Dictionary<string, string>();
            Assert.AreEqual("test",
                LetsEncrypt.GetString(sdict, "something", "test"));

            sdict["something"] = "another";
            Assert.AreEqual("another",
                LetsEncrypt.GetString(sdict, "something", "test"));

            var dict = new ObjectDictionary();
            Assert.AreEqual("test",
                LetsEncrypt.GetString(dict, "something", "test"));

            dict["something"] = "another";
            Assert.AreEqual("another",
                LetsEncrypt.GetString(sdict, "something", "test"));

            var token = new JObject { };
            Assert.AreEqual("test",
                LetsEncrypt.GetString(token, "something", "test"));

            token["something"] = "another";
            Assert.AreEqual("another",
                LetsEncrypt.GetString(sdict, "something", "test"));

            token = null;
            Assert.AreEqual("test",
                LetsEncrypt.GetString(token, "something", "test"));
        }

        [TestMethod]
        public void LetsEncrypt_PadTest()
        {
            Assert.AreEqual("   5", LetsEncrypt.Pad(5, 4));
            Assert.AreEqual("  15", LetsEncrypt.Pad(15, 4));
        }

        [TestMethod]
        public void LetsEncrypt_ScheduleRenewalTest()
        {
            var target = new Target {
                Host = "localhost",
                PluginName = R.Manual,
                SiteId = 0,
                WebRootPath = Plugin.BaseDirectory
            };
            var options = new Options {
                Plugin = R.Manual,
                ConfigPath = Plugin.BaseDirectory,
                Silent = true,
                Test = true
            };
            var taskService = new Mock<ITaskService>();
            var mockTask = new Mock<ITask>();
            taskService.Setup(m => m.NewTask()).Returns(mockTask.Object);
            LetsEncrypt.ScheduleRenewal(target, options, taskService.Object);
        }

        [TestMethod]
        public void Target_Test()
        {
            var target = new Target
            {
                Host = "testhost",
                PluginName = "testplugin",
                WebRootPath = "testpath"
            };

            Assert.AreEqual("testplugin testhost (testpath)", target.ToString());
        }

        [TestMethod]
        public void Options_Test()
        {
            var a = new Options();
            var b = new Options();

            Assert.AreEqual(JsonConvert.SerializeObject(a), JsonConvert.SerializeObject(b));
            JsonConvert.DeserializeObject<Options>(JsonConvert.SerializeObject(a));
        }
    }
}

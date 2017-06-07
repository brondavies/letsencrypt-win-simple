using letsencrypt;
using letsencrypt.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
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

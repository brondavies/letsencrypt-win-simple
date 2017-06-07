using letsencrypt;
using letsencrypt.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace letsencrypt_tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void Settings_SaveTest()
        {
            var settings = new Settings(Plugin.BaseDirectory);
            settings.Renewals = new List<ScheduledRenewal>();
            var renewal = new ScheduledRenewal
            {
                Date = DateTime.Now
            };
            settings.Renewals.Add(renewal);
            settings.Save();
            Assert.IsTrue(File.Exists(settings.settingsPath));
            var reloaded = new Settings(Plugin.BaseDirectory);
            Assert.AreEqual(settings.Renewals.Count, reloaded.Renewals.Count);
            Assert.AreEqual(settings.Renewals.First().ToString(), renewal.ToString());
        }

        [TestCleanup]
        public void Cleanup()
        {
            var f = Path.Combine(Plugin.BaseDirectory, "settings.json");
            if (File.Exists(f))
            {
                File.Delete(f);
            }
        }
    }
}

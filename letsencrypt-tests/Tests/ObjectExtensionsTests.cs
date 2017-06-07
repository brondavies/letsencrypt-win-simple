using letsencrypt_tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ArrayExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letsencrypt_tests
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        [TestMethod]
        public void ObjectExtensions_IsPrimitiveTest()
        {
            Assert.IsTrue(typeof(string).IsPrimitive());
            Assert.IsFalse(typeof(DateTime).IsPrimitive());
        }

        [TestMethod]
        public void ObjectExtensions_CopyTest()
        {
            var now = DateTime.Now;
            var copy = now.Copy();
            Assert.AreEqual(now, copy);
        }

        [TestMethod]
        public void ReferenceEqualityComparer_Test()
        {
            var comparer = new ReferenceEqualityComparer();
            var a = new { test = "test" };
            Assert.IsTrue(comparer.Equals(a, a));
            Assert.AreNotEqual(0, comparer.GetHashCode(a));
            Assert.AreEqual(0, comparer.GetHashCode(null));
        }

        [TestMethod]
        public void ArrayExtensions_Test()
        {
            var hit = 0;
            var test = new string[] { };
            test.ForEach((a, e) =>
            {
                hit++;
            });
            Assert.AreEqual(0, hit);

            test = new[] { "test", "again" };
            test.ForEach((a, e) => {
                hit++;
            });
            Assert.AreEqual(2, hit);
        }
    }
}

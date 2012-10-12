using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Io.Rz.FlywheelComponents.Chunking
{
    [TestClass]
    public class RabinHashFunctionTests
    {
        [TestMethod]
        public void TestRabinHashBasic()
        {
            RabinHashFunction r = new RabinHashFunction();
            Assert.AreNotEqual(r.Hash("foo"), r.Hash("bar"));
            Assert.AreNotEqual(r.Hash("foo"), r.Hash("oof"));
            Assert.AreNotEqual(r.Hash("1234567890"), r.Hash("1134567890"));

            Assert.AreEqual(15671748242107854578, r.Hash("aaabbb12334567rp6ejflskxnjclzjflaksjfdlaksjlaksjdasd"));

        }
    }
}

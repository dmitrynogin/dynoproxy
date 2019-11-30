using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;

namespace Dynoproxy.Tests
{
    [TestClass]
    public class Proxy_Should
    {
        [TestMethod]
        public void Call()
        {
            dynamic c = new ExpandoObject();
            c.Add = (Func<int, int, int>)((a, b) => a + b);

            ICalculator proxy = Proxy.Create<ICalculator>(c);
            Assert.AreEqual(3, proxy.Add(1, 2));
        }
    }

    public interface ICalculator
    {
        int Add(int a, int b);
    }
}

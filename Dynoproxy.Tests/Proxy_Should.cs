using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.Linq;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Dynoproxy.Tests
{
    [TestClass]
    public class Proxy_Should
    {
        [TestMethod]
        public void Call_ExpandoObject()
        {
            dynamic c = new ExpandoObject();
            c.Add = (Func<int, int, int>)((a, b) => a + b);

            ICalculator proxy = Proxy.Create<ICalculator>(c);
            Assert.AreEqual(3, proxy.Add(1, 2));
        }

        [TestMethod]
        public void Call_Delegate()
        {
            object Add(ProxyCall call)
            {
                Assert.AreEqual("Add", call.Name);
                Assert.AreEqual(2, call.Args.Count);
                Assert.AreEqual(typeof(int), call.Result);
                Assert.AreEqual("Sum", call.Description);
                return call.Args.OfType<int>().Sum();
            }

            ICalculator proxy = Proxy.Create<ICalculator>(Add);
            Assert.AreEqual(3, proxy.Add(1, 2));
        }
    }

    public interface ICalculator
    {
        [Description("Sum")]
        int Add(int a, int b);
    }    
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Dynoproxy.Tests
{
    [TestClass]
    public class Regress_Api_Should_Login
    {
        [TestMethod]
        public async Task Successfully()
        {
            using (var proxy = Proxy.Create<IReqres>("https://reqres.in"))
            {
                var session = await proxy.LoginAsync(new ReqresCredentials
                {
                    Email = "eve.holt@reqres.in",
                    Password = "cityslicka"
                });

                Assert.AreEqual("QpwL5tke4Pnpja7X4", session.Token);
            }
        }
        
        [TestMethod]
        public async Task Unsuccessfully()
        {
            try
            {
                using (var proxy = Proxy.Create<IReqres, ReqresError>("https://reqres.in"))
                    await proxy.LoginAsync(new ReqresCredentials
                    {
                        Email = "eve.holt@reqres.in"                        
                    });
            }
            catch(RestException<ReqresError> ex) when (ex.Error.Error == "Missing password")
            {
            }
        }
    }

    public interface IReqres : IDisposable
    {
        [Description("POST api/login {0}")]
        Task<ReqresSession> LoginAsync(ReqresCredentials credentials);
    }

    public class ReqresCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ReqresSession
    {
        public string Token { get; set; }
    }

    public class ReqresError
    {
        public string Error { get; set; }
    }
}

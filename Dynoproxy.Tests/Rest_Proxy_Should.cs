using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Dynoproxy.Tests
{
    [TestClass]
    public class Rest_Proxy_Should
    {
        [TestMethod]
        public async Task Call_REST_API()
        {
            using (var proxy = Proxy.Create<ITypicode>("http://jsonplaceholder.typicode.com"))
            {
                var posts = await proxy.GetAsync();
                Assert.AreEqual(100, posts.Length);

                var post = await proxy.GetAsync(1, "test");
                Assert.AreEqual(1, post.Id);

                post.Title = "XYZ";
                post = await proxy.PutAsync(1, post);
                Assert.AreEqual("XYZ", post.Title);
            }
        }
    }

    public interface ITypicode : IDisposable
    {
        [Description("GET posts")]
        Task<BlogPost[]> GetAsync();

        [Description("GET posts/{0}")]
        Task<BlogPost> GetAsync(int id, string test);

        [Description("POST posts {0}")]
        Task<BlogPost> PostAsync(BlogPost data);

        [Description("PUT posts/{0} {1}")]
        Task<BlogPost> PutAsync(int id, BlogPost data);

        [Description("DELETE posts/{0}")]
        Task DeleteAsync(int id);
    }

    public class BlogPost
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}

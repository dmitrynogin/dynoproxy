# Dynoproxy
Represents dynamic sources through strictly typed proxies:

[GitHub](https://github.com/dmitrynogin/dynoproxy) and [NuGet](https://www.nuget.org/packages/Dynoproxy)

```csharp
        [TestMethod]
        public async Task Call_REST_API()
        {
            using (var proxy = Proxy.Create<ITypicode>("http://jsonplaceholder.typicode.com"))
            {
                var posts = await proxy.GetAsync();
                Assert.AreEqual(100, posts.Length);

                var post = await proxy.GetAsync(1);
                Assert.AreEqual(1, post.Id);

                post.Title = "XYZ";
                post = await proxy.PutAsync(1, post);
                Assert.AreEqual("XYZ", post.Title);
            }
        }
```

where:

```csharp
    public interface ITypicode : IDisposable
    {
        [Description("GET posts")]
        Task<BlogPost[]> GetAsync();

        [Description("GET posts/{0}")]
        Task<BlogPost> GetAsync(int id);

        [Description("PUT posts/{0} {1}")]
        Task<BlogPost> PutAsync(int id, BlogPost data);
    }

    public class BlogPost
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
```

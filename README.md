# dynoproxy
Represents dynamic sources through strictly typed proxies:

```csharp
[TestMethod]
public void Call()
{
     dynamic c = new ExpandoObject();
     c.Add = (Func<int, int, int>)((a, b) => a + b);

     ICalculator proxy = Proxy.Create<ICalculator>(c);
     Assert.AreEqual(3, proxy.Add(1, 2));
}
```

where:

```csharp
public interface ICalculator
{
     int Add(int a, int b);
}
```

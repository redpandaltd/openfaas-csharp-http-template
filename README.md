# OpenFaaS C# Template

This template for OpenFaaS makes use of ASP.NET Core. This allows more control over the request (by providing an `HttpRequest` instance) and better handling of the response by returning an `IActionResult`.

## Using the template

A file named `Function.cs` is generated when you create a new function with this template. In this file is a class named `Function` that implements `HttpFunction`. This is what it looks like:

``` csharp
namespace OpenFaaS
{
    public class Function : HttpFunction
    {
        public override Task<IActionResult> HandleAsync( HttpRequest request )
        {
            var result = new
            {
                Message = "Hello!"
            };

            return Task.FromResult( Ok( result ) );
        }
    }
}
```

This is just an example. You can now start implementing your function.

If you want to restrict function execution to a particular HTTP method (or methods) you can decorate `HandleAsync` with HTTP method attributes.

```csharp
public class Function : HttpFunction
{
    [HttpPost]
    public override Task<IActionResult> HandleAsync( HttpRequest request )
    {
        // this will only execute with a POST method
    }
}
```

You can also implement only a specific HTTP method (or multiple, each with its own logic) if you want. Instead of overriding the public method `HandleAsync`, override the protected methods `HandleGetAsync` or `HandlePostAsync` for example.

``` csharp
namespace OpenFaaS
{
    public class Function : HttpFunction
    {
        // not overriding HandleAsync so that I can use the other handlers

        protected override Task<IActionResult> HandlePostAsync( HttpRequest request )
        {
            var result = new
            {
                Message = "Hello with ASP.NET Core!"
            };

            return Task.FromResult( Ok( result ) );
        }
    }
}
```

When we use this, all methods that are not handled return a 405.

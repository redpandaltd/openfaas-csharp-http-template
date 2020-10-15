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

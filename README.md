# TheNevix.Utils.Auth

![NuGet](https://img.shields.io/nuget/v/TheNevix.Utils.Auth) ![NuGet Downloads](https://img.shields.io/nuget/dt/TheNevix.Utils.Auth)

A simple nuget package to easily manage multiple auth ways for an api endpoint.

## Features

- Add multiple authentication methods on 1 api endpoint
- Easy to setup

## Installation

You can install the package via NuGet Package Manager, .NET CLI, or by adding it to your project file.

### NuGet Package Manager

```bash
Install-Package TheNevix.Utils.Auth
```

## Configuration in program.cs

The NuGet package currently supports Jwt tokens and Api keys. In your code, add the following code:

```csharp
builder.Services.AddAuth(options =>
{
    options.Methods.Add(new AuthMethodConfig
    {
        Name = "TestApiKey",
        Value = "1234",
        Type = AuthMethod.ApiKey,
        HeaderName = "X-Api-Key"
    });

    options.Methods.Add(new AuthMethodConfig
    {
        Name = "UserJwt",
        Value = "user-api",
        Type = AuthMethod.Jwt
    });

    options.Methods.Add(new AuthMethodConfig
    {
        Name = "AdminJwt",
        Value = "admin-api",
        Type = AuthMethod.Jwt
    });
});
```

In this example, there is 1 Api key configured and 2 Jwt token schemes. The name property should be the name of your Api key or Jwt scheme. The value should be the value of the auth method. For now, only Api keys use values. Type indicates if the auth method is an Api key or Jwt token. There is also a nullable HeaderName property to pass the header name of the auth Method. This is required for auth methods that are Api keys.

## Usage

```csharp
[HttpGet("anyauth")]
[Auth("UserJwt", "AdminJwt", "TestApiKey")]
public IActionResult AnyAuthEndpoint()
{
    var validatedMethod = HttpContext.Items["ValidatedAuthMethod"] as string ?? "unknown";
    var username = User.Identity?.Name ?? "anonymous";

    return Ok($"Authenticated via: {validatedMethod}. User: {username}");
}
```

You use the "Auth" attribute above an endpoint passing down the auth methods that should be checked on. The attribute will check from left to right. I suggest making the names you pass not hardcorded but in a variable. "using TheNevix.Utils.Auth;" needs to imported.

To see which auth method was used to authenticate the request, you can find out using the "ValidatedAuthMethod" which contains the auth method name of the auth method which validated the request.

If no valid auth method has been provided, the caller gets an 401.

## License

This project is licensed under the MIT License.

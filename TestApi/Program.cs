using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TheNevix.Utils.Auth.Configuration;

namespace TestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ? Add custom AuthOptions (API Keys & JWT schemes config)
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

            // ? Add Authentication with NO DefaultScheme (so your [Auth] attribute decides)
            builder.Services.AddAuthentication(options =>
            {
                // ? Geen DefaultScheme! ASP.NET doet niets automatisch
                options.DefaultChallengeScheme = "DummyJwt"; // Swagger werkt, maar APIKey endpoints worden NIET automatisch geauthenticeerd
            })
.AddJwtBearer("UserJwt", options =>
{
    options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false
    };
})
.AddJwtBearer("AdminJwt", options =>
{
    options.Authority = "https://demo.identityserver.io";
    options.Audience = "admin-api";
}).AddJwtBearer("DummyJwt", options =>
{
    // Deze handler zal nooit echt gebruikt worden
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.NoResult(); // skip auth
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.NoResult(); // skip errors
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse(); // stop default challenge
            return Task.CompletedTask;
        }
    };
}); 

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();

            // ? Swagger: Bearer + APIKey support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestApi", Version = "v1" });

                // Bearer token support
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {token}'"
                });

                // API Key support
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Name = "X-Api-Key",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Provide API Key"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() },
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }, Array.Empty<string>() }
                });
            });

            var app = builder.Build();

            // ? Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); // Needed for JWT endpoints (per your [Auth] attribute)
            app.UseAuthorization();  // Needed for standard policies

            app.MapControllers();

            app.Run();
        }
    }
}

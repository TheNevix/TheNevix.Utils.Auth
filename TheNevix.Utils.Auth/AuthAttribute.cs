using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheNevix.Utils.Auth.Configuration;

namespace TheNevix.Utils.Auth
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _methods;
        private const string AuthMethodKey = "ValidatedAuthMethod";

        public AuthAttribute(params string[] methods)
        {
            _methods = methods;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            var options = httpContext.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;

            var authenticationService = httpContext.RequestServices.GetRequiredService<IAuthenticationService>();

            string validatedMethod = await ValidateAuthAsync(httpContext, options, authenticationService);

            if (string.IsNullOrEmpty(validatedMethod))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            
            httpContext.Items[AuthMethodKey] = validatedMethod;
        }

        private async Task<string> ValidateAuthAsync(HttpContext context, AuthOptions authOptions, IAuthenticationService authenticationService)
        {
            //Get all the authOptions based on provided methods
            var foundAuthMethods = authOptions.Methods
                .Where(m => _methods.Contains(m.Name))
                .ToList();

            foreach (var method in foundAuthMethods)
            {
                if (method.Type == AuthMethod.Jwt)
                {
                    var authResult = await authenticationService.AuthenticateAsync(context, method.Name);

                    if (authResult.Succeeded && authResult.Principal?.Identity?.IsAuthenticated == true)
                    {
                        context.User = authResult.Principal;
                        return method.Name;
                    }
                }
                else
                {
                    if (context.Request.Headers.TryGetValue(method.HeaderName, out var potentialApiKey))
                    {
                        if (potentialApiKey.Equals(method.Value))
                        {
                            return method.Name;
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}

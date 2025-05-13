using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheNevix.Utils.Auth;

namespace TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("user")]
        [Auth("UserJwt")]
        public IActionResult UserJwtEndpoint()
        {
            var username = User.Identity?.Name ?? "anonymous";
            return Ok($"✅ Access via UserJwt scheme. User: {username}");
        }

        [HttpGet("admin")]
        [Auth("AdminJwt")]
        public IActionResult AdminJwtEndpoint()
        {
            var username = User.Identity?.Name ?? "anonymous";
            return Ok($"✅ Access via AdminJwt scheme. User: {username}");
        }

        [HttpGet("apikey")]
        [Auth("TestApiKey")]
        public IActionResult ApiKeyEndpoint()
        {
            return Ok("✅ Access granted via API Key authentication.");
        }

        [HttpGet("anyauth")]
        [Auth("UserJwt", "AdminJwt", "TestApiKey")]
        public IActionResult AnyAuthEndpoint()
        {
            var validatedMethod = HttpContext.Items["ValidatedAuthMethod"] as string ?? "unknown";
            var username = User.Identity?.Name ?? "anonymous";

            return Ok($"✅ Authenticated via: {validatedMethod}. User: {username}");
        }
    }
}

using System.Security.Claims;
using KeyCloakTest.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KeyCloakTest.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class AuthDemoController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("public")]
    public IActionResult Public() => Ok(new
    {
        message = "Public endpoint: no token required.",
        utcNow = DateTimeOffset.UtcNow
    });

    [Authorize(Policy = AuthPolicies.Authenticated)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims
            .Where(claim => claim.Type is DemoClaimTypes.PreferredUsername or ClaimTypes.Email or DemoClaimTypes.Scope or DemoClaimTypes.Scp or DemoClaimTypes.Role)
            .Select(claim => new { claim.Type, claim.Value })
            .ToArray();

        return Ok(new
        {
            message = "Authenticated endpoint.",
            user = User.Identity?.Name ?? User.FindFirst(DemoClaimTypes.PreferredUsername)?.Value,
            claims
        });
    }

    [Authorize(Policy = AuthPolicies.ReadScope)]
    [HttpGet("reports")]
    public IActionResult GetReports() => Ok(new
    {
        message = "Read endpoint authorized by api.read scope or admin role.",
        data = new[] { "report-2026-01", "report-2026-02" }
    });

    [Authorize(Policy = AuthPolicies.WriteScope)]
    [EnableRateLimiting(RateLimiterPolicies.WriteOperations)]
    [HttpPost("reports")]
    public IActionResult CreateReport([FromBody] CreateReportRequest request)
    {
        return CreatedAtAction(nameof(GetReports), new
        {
            id = Guid.NewGuid(),
            request.Name,
            request.Description,
            createdAt = DateTimeOffset.UtcNow
        });
    }

    [Authorize(Policy = AuthPolicies.AdminRole)]
    [HttpGet("admin")]
    public IActionResult Admin() => Ok(new
    {
        message = "Admin endpoint. Realm role admin is required."
    });
}

public sealed record CreateReportRequest(string Name, string Description);

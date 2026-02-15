using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KeyCloakTest.Tests;

public sealed class AuthDemoEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PublicEndpoint_ShouldBeAccessibleWithoutToken()
    {
        var response = await _client.GetAsync("/api/demo/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MeEndpoint_ShouldReturnUnauthorizedWithoutToken()
    {
        var response = await _client.GetAsync("/api/demo/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReadEndpoint_ShouldReturnUnauthorizedWithoutToken()
    {
        var response = await _client.GetAsync("/api/demo/reports");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

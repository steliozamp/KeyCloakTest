namespace KeyCloakTest.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string Authority { get; init; } = "http://localhost:8080/realms/auth-demo";

    public string? ValidIssuer { get; init; }

    public string[] ValidAudiences { get; set; } = [];

    public bool RequireHttpsMetadata { get; init; } = false;

    public string ApiClientId { get; init; } = "auth-demo-api";

    public string SwaggerClientId { get; init; } = "auth-demo-swagger";
}

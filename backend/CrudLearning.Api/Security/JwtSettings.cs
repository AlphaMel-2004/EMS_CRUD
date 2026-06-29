namespace CrudLearning.Api.Security;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "CrudLearning.Api";
    public string Audience { get; set; } = "CrudLearning.Web";
    public string Key { get; set; } = "change-this-development-key-to-a-long-random-value";
    public int ExpiresMinutes { get; set; } = 480;
}
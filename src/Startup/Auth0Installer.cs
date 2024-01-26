using Auth0.AspNetCore.Authentication;

namespace MyUglyChat.Startup;

public static class Auth0Installer
{
    public static void InstallAuth0(this IServiceCollection services, IConfigurationRoot config)
    {
        var auth0Domain = config["Auth0:Domain"];
        if (string.IsNullOrWhiteSpace(auth0Domain))
        {
            throw new ArgumentException(nameof(auth0Domain));
        }
        var auth0ClientId = config["Auth0:ClientId"];
        if (string.IsNullOrWhiteSpace(auth0ClientId))
        {
            throw new ArgumentException(nameof(auth0ClientId));
        }

        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = auth0Domain;
            options.ClientId = auth0ClientId;
            options.Scope = "openid profile email";
        });
    }
}

namespace GameLending.Auth.Domain;

public sealed record ClientCredential(string ClientId, string ClientSecret)
{
    public bool IsWellFormed => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret)
        && ClientId.Length <= 200 && ClientSecret.Length <= 1000;
}

using System.Net.Http.Headers;
using System.Text.Json;
using OsuKeyPresses.Models;

namespace OsuKeyPresses.Stores;

internal class OAuthStore(string osuClientId, string osuClientSecret)
{
    private static string? _accessToken;
    
    public string GetAccessToken()
    {
        if (_accessToken is not null)
            return _accessToken;

        var request = new HttpRequestMessage(HttpMethod.Post, "https://osu.ppy.sh/oauth/token");

        request.Content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", osuClientId),
            new KeyValuePair<string, string>("client_secret", osuClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "public"),
        ]);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        using var httpClient = new HttpClient();
        var response = httpClient.Send(request);
        
        response.EnsureSuccessStatusCode();

        using var jsonResponse = response.Content.ReadAsStream();
        var accessToken = JsonSerializer.Deserialize<OsuOAuthResponse>(jsonResponse);

        _accessToken = accessToken!.AccessToken;

        return accessToken.AccessToken;
    }
}
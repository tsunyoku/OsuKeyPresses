using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ManiaKeyPresses.Models;

namespace ManiaKeyPresses;

public class OsuApiClient(string osuClientId, string osuClientSecret)
{
    private readonly OAuthStore _oauthStore = new(osuClientId, osuClientSecret);
    
    private static readonly Dictionary<(long, string), Score> Scores = new();
    private static readonly Dictionary<(int, string), User> Users = new();

    public Score? GetLegacyScore(long scoreId, string rulesetName)
    {
        if (Scores.TryGetValue((scoreId, rulesetName), out var score))
            return score;

        var accessToken = _oauthStore.GetAccessToken();
        
        using var httpClient = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/scores/{rulesetName}/{scoreId}");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("x-api-version", "99999999");

        var response = httpClient.Send(request);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        
        using var jsonResponse = response.Content.ReadAsStream();

        score = JsonSerializer.Deserialize<Score>(jsonResponse);
        Scores[(scoreId, rulesetName)] = score!;

        return score!;
    }

    public User? GetUser(int userId, string rulesetName)
    {
        if (Users.TryGetValue((userId, rulesetName), out var user))
            return user;

        var accessToken = _oauthStore.GetAccessToken();
        
        using var httpClient = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{userId}/{rulesetName}");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("x-api-version", "99999999");

        var response = httpClient.Send(request);
        
        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        
        using var jsonResponse = response.Content.ReadAsStream();

        user = JsonSerializer.Deserialize<User>(jsonResponse);
        Users[(userId, rulesetName)] = user!;

        return user!;
    }
}
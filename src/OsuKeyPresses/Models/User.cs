using System.Text.Json.Serialization;

namespace OsuKeyPresses.Models;

public class User
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("username")]
    public required string Username { get; init; }
    
    [JsonPropertyName("country")]
    public required UserCountry Country { get; init; }
    
    [JsonPropertyName("statistics")]
    public UserStatistics? Statistics { get; init; }
}

public class UserStatistics
{
    [JsonPropertyName("pp")]
    public required double Pp { get; init; }

    [JsonPropertyName("global_rank")]
    public int? GlobalRank { get; init; }
}

public class UserCountry
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
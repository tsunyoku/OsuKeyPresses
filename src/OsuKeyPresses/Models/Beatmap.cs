using System.Text.Json.Serialization;

namespace OsuKeyPresses.Models;

public class Beatmap
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
}
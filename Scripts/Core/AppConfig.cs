namespace Code_Game.Scripts.Core;

public static class AppConfig
{
    // Replace with your real API endpoint when available.
    public const string ApiBaseUrl = "https://your-game-api.example.com";
    public const string PlayerProfileEndpoint = "api/playerprofiles";
    public const string DataFolder = "Data";
    public const string SaveFolder = "Saves";
    public const string PlayerProfileFile = "player_profiles.json";

    // Steam configuration - Replace with your actual Steam App ID
    public const uint SteamAppId = 480; // Spacewar test app - replace with your game's App ID
}

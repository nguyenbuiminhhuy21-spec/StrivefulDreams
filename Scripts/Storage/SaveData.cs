using System;

namespace Code_Game.Scripts.Storage;

public class SaveData
{
    // Player
    public string PlayerName { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string FavoriteThing { get; set; } = string.Empty;
    public string AnimalPreference { get; set; } = string.Empty;

    // Meta
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
    public string GameVersion { get; set; } = "1.0.0";
}

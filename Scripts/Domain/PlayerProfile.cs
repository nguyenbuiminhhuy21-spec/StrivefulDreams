using System;

namespace Code_Game.Domain;

public class PlayerProfile
{
    public string Name { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string FavoriteThing { get; set; } = string.Empty;
    public string AnimalPreference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

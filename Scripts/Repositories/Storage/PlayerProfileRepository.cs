using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code_Game.Domain;
using Code_Game.Scripts.Storage;

namespace Code_Game.Scripts.Repositories.Storage;

public class PlayerProfileRepository : IPlayerProfileRepository
{
    public void Save(PlayerProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
            profile.Name = "Player";

        profile.CreatedAt = DateTime.UtcNow;

        var saveData = MapToSaveData(profile);
        var farmName = string.IsNullOrWhiteSpace(profile.FarmName) ? "DefaultFarm" : profile.FarmName;

        SaveManager.Save(saveData, farmName);
    }

    public IEnumerable<PlayerProfile> LoadAll()
    {
        var profiles = new List<PlayerProfile>();
        var farmNames = SaveManager.GetExistingFarmNames();

        foreach (var farmName in farmNames)
        {
            var data = SaveManager.Load(farmName);
            if (data != null)
                profiles.Add(MapToProfile(data));
        }

        return profiles;
    }

    private static SaveData MapToSaveData(PlayerProfile profile)
    {
        return new SaveData
        {
            PlayerName = profile.Name,
            FarmName = profile.FarmName,
            FavoriteThing = profile.FavoriteThing,
            AnimalPreference = profile.AnimalPreference,
            CreatedAt = profile.CreatedAt,
            LastSaved = DateTime.UtcNow,
            GameVersion = "1.0.0"
        };
    }

    private static PlayerProfile MapToProfile(SaveData data)
    {
        return new PlayerProfile
        {
            Name = data.PlayerName,
            FarmName = data.FarmName,
            FavoriteThing = data.FavoriteThing,
            AnimalPreference = data.AnimalPreference,
            CreatedAt = data.CreatedAt
        };
    }
}

using System.Collections.Generic;
using Code_Game.Domain;

namespace Code_Game.Scripts.Repositories.Storage;

public interface IPlayerProfileRepository
{
    void Save(PlayerProfile profile);
    IEnumerable<PlayerProfile> LoadAll();
}

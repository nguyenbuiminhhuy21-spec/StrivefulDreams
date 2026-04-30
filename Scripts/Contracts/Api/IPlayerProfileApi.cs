using System.Threading.Tasks;
using Code_Game.Domain;

namespace Code_Game.Scripts.Contracts.Api;

public interface IPlayerProfileApi
{
    Task<bool> SendProfileAsync(PlayerProfile profile);
}

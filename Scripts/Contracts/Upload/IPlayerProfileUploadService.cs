using System.Threading.Tasks;
using Code_Game.Domain;

namespace Code_Game.Scripts.Contracts.Upload;

public interface IPlayerProfileUploadService
{
    Task<bool> UploadProfileAsync(PlayerProfile profile);
}

using System;
using System.Threading.Tasks;
using Code_Game.Domain;
using Code_Game.Scripts.Contracts.CharacterCreation;
using Code_Game.Scripts.Contracts.Upload;
using Code_Game.Scripts.Repositories.Storage;
using Code_Game.Scripts.Services.Upload;

namespace Code_Game.Scripts.Services.CharacterCreation;

public class CharacterCreationService : ICharacterCreationService
{
    private readonly IPlayerProfileRepository _repository;
    private readonly IPlayerProfileUploadService _uploadService;
    private readonly string[] _animalOptions = { "Cow", "Chicken", "Cat", "Dog" };
    private int _selectedAnimalIndex;

    public string Name { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string FavoriteThing { get; set; } = string.Empty;
    public string SelectedAnimal => _animalOptions[_selectedAnimalIndex];

    public CharacterCreationService(IPlayerProfileRepository repository, IPlayerProfileUploadService uploadService)
    {
        _repository = repository;
        _uploadService = uploadService;
    }

    public CharacterCreationService()
        : this(new PlayerProfileRepository(), new PlayerProfileUploadService())
    {
    }

    public void SelectPreviousAnimal()
    {
        _selectedAnimalIndex = (_selectedAnimalIndex + _animalOptions.Length - 1) % _animalOptions.Length;
    }

    public void SelectNextAnimal()
    {
        _selectedAnimalIndex = (_selectedAnimalIndex + 1) % _animalOptions.Length;
    }

    public string Confirm()
    {
        var profile = new PlayerProfile
        {
            Name = Name,
            FarmName = FarmName,
            FavoriteThing = FavoriteThing,
            AnimalPreference = SelectedAnimal
        };

        try
        {
            _repository.Save(profile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save profile locally: {ex.Message}");
            return "Failed to save profile. Please try again.";
        }

        try
        {
            var uploadSuccess = Task.Run(() => _uploadService.UploadProfileAsync(profile)).Result;
            if (uploadSuccess)
            {
                return "Profile saved locally and uploaded successfully.";
            }
        }
        catch
        {
            // Upload failed, but local save succeeded
        }

        return "Profile saved locally.";
    }

    public void Cancel()
    {
        // Nothing stored, just discard entered values.
    }
}

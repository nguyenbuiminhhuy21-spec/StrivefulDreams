using Microsoft.Xna.Framework;
using Code_Game.Scripts.Contracts.CharacterCreation;

namespace Code_Game.Scripts.Services.CharacterCreation;

public class CharacterCreationService : ICharacterCreationService
{
    private static CharacterCreationService _instance;
    public static CharacterCreationService Instance => _instance ??= new CharacterCreationService();

    public string Name { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string FavoriteThing { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";
    public string SelectedAnimal => "Dog"; // Placeholder

    public int BirthdaySeason { get; set; } = 0;
    public int BirthdayDay { get; set; } = 1;

    // Appearance Indices
    public int HairIndex { get; set; } = 0;
    public int ShirtIndex { get; set; } = 0;
    public int PantsIndex { get; set; } = 0;

    // Max counts
    public int MaxHairStyles { get; set; } = 10;
    public int MaxShirtStyles { get; set; } = 10;
    public int MaxPantsStyles { get; set; } = 10;

    // Colors
    public Color HairColor { get; set; } = Color.White;
    public Color ShirtColor { get; set; } = Color.White;
    public Color PantsColor { get; set; } = Color.White;

    public void SelectPreviousAnimal() { }
    public void SelectNextAnimal() { }

    public string Confirm()
    {
        return $"Character Created: {Name} of {FarmName} [{Gender}] (Hair:{HairIndex}, Shirt:{ShirtIndex}, Pants:{PantsIndex})";
    }

    public void Cancel() { }
}

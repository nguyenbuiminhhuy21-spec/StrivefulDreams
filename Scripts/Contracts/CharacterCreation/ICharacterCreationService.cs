using Microsoft.Xna.Framework;

namespace Code_Game.Scripts.Contracts.CharacterCreation;

public interface ICharacterCreationService
{
    string Name { get; set; }
    string FarmName { get; set; }
    string FavoriteThing { get; set; }
    string Gender { get; set; }
    string SelectedAnimal { get; }

    // Birthday
    int BirthdaySeason { get; set; }
    int BirthdayDay { get; set; }

    // Appearance Indices
    int HairIndex { get; set; }
    int ShirtIndex { get; set; }
    int PantsIndex { get; set; }

    // Max counts
    int MaxHairStyles { get; set; }
    int MaxShirtStyles { get; set; }
    int MaxPantsStyles { get; set; }

    // Colors
    Color HairColor { get; set; }
    Color ShirtColor { get; set; }
    Color PantsColor { get; set; }

    void SelectPreviousAnimal();
    void SelectNextAnimal();
    string Confirm();
    void Cancel();
}

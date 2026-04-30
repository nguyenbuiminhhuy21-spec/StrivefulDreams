namespace Code_Game.Scripts.Services.CharacterCreation.Contracts;

public interface ICharacterCreationService
{
    string Name { get; set; }
    string FarmName { get; set; }
    string FavoriteThing { get; set; }
    string SelectedAnimal { get; }

    void SelectPreviousAnimal();
    void SelectNextAnimal();
    string Confirm();
    void Cancel();
}

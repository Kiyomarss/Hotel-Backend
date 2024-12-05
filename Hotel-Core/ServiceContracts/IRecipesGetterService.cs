using ContactsManager.Core.DTO;

namespace ServiceContracts;

public interface IRecipesGetterService
{
    Task<CabinResponse?> GetRecipeByRecipeID(Guid? RecipeID);

    Task<List<CabinResponse>> GetAllRecipes();

    Task<List<CabinResponse>> GetRecipeByRecipeName(string title);
}
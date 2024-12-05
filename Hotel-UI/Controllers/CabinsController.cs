using ContactsManager.Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

[Route("[controller]")]
public class CabinsController  : Controller
{
    private readonly ICabinsDeleterService _cabinsDeleterService;

    public CabinsController(ICabinsDeleterService cabinsDeleterService)
    {
        _cabinsDeleterService = cabinsDeleterService;
    }
    
    [HttpDelete]
    [Route("[action]")]
    public async Task<IActionResult> Delete([FromBody] GuidDto dto)
    {
        var deleteCabin = await _cabinsDeleterService.DeleteCabin(dto.Id);
        return Json(new { Cabins = deleteCabin });
    }

}
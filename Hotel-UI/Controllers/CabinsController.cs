using ContactsManager.Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

[Route("[controller]")]
public class CabinsController  : Controller
{
    private readonly ICabinsDeleterService _cabinsDeleterService;
    private readonly ICabinsGetterService _cabinsGetterService;

    public CabinsController(ICabinsDeleterService cabinsDeleterService, ICabinsGetterService cabinsGetterService)
    {
        _cabinsDeleterService = cabinsDeleterService;
        _cabinsGetterService = cabinsGetterService;
    }
    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetCabins()
    {
        var cabins = await _cabinsGetterService.GetCabins();
        return Json(new { Cabins = cabins });
    }
    
    [HttpDelete]
    [Route("[action]")]
    public async Task<IActionResult> Delete([FromBody] GuidDto dto)
    {
        var deleteCabin = await _cabinsDeleterService.DeleteCabin(dto.Id);
        return Json(new { Cabins = deleteCabin });
    }
}
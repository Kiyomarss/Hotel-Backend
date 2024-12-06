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
    private readonly ICabinsAdderService _cabinsAdderService;
    private readonly ICabinsUpdaterService _cabinsUpdaterService;

    public CabinsController(ICabinsDeleterService cabinsDeleterService, ICabinsGetterService cabinsGetterService, ICabinsAdderService cabinsAdderService, ICabinsUpdaterService cabinsUpdaterService)
    {
        _cabinsDeleterService = cabinsDeleterService;
        _cabinsGetterService = cabinsGetterService;
        _cabinsAdderService = cabinsAdderService;
        _cabinsUpdaterService = cabinsUpdaterService;
    }
    
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Create([FromBody] CabinUpsertRequest dto)
    {
        var cabinResponse = await _cabinsAdderService.AddCabin(dto);
        return Json(new { Cabin = cabinResponse });
    }
    
    [Route("[action]")]
    [HttpPut]
    public async Task<IActionResult> Edit([FromBody] CabinUpsertRequest dto)
    {
        try
        {
            CabinResponse? existingCabin = await _cabinsGetterService.GetCabinByCabinId(dto.Id);
            if (existingCabin == null)
            {
                return NotFound(new { Message = "Cabin not found" });
            }

            CabinResponse updatedCabin = await _cabinsUpdaterService.UpdateCabin(dto);

            return Ok(new
            {
                Message = "Cabin updated successfully",
                Cabin = updatedCabin
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "An error occurred while updating the cabin",
                Error = ex.Message
            });
        }
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
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
    public async Task<IActionResult> Create([FromForm] CabinUpsertRequest dto)
    {
        try
        {
            if (dto.Image is { Length: > 0 })
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(dto.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { Message = "Invalid file type. Only images are allowed." });
                }

                dto.ImagePath = await SaveNewImageAsync(dto.Image);
            }

            var cabinResponse = await _cabinsAdderService.AddCabin(dto);

            return Json(new
            {
                Message = "Cabin created successfully",
                Cabin = cabinResponse
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error while creating cabin: {ex.Message}");

            return StatusCode(500, new
            {
                Message = "An error occurred while creating the cabin",
                Error = ex.Message
            });
        }
    }

    [HttpPut]
    [Route("[action]")]
    public async Task<IActionResult> Edit([FromForm] CabinUpsertRequest dto)
    {
        try
        {
            CabinResponse? existingCabin = await _cabinsGetterService.GetCabinByCabinId(dto.Id);
            if (existingCabin == null)
            {
                return NotFound(new { Message = "Cabin not found" });
            }

            if (dto.Image is { Length: > 0 })
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(dto.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { Message = "Invalid file type. Only images are allowed." });
                }

                DeleteOldImage(existingCabin.ImagePath);

                dto.ImagePath = await SaveNewImageAsync(dto.Image);
            }
            else
            {
                // اگر تصویر جدید آپلود نشده، تصویر قبلی حفظ شود
                dto.ImagePath = existingCabin.ImagePath;
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
            Console.Error.WriteLine($"Error while updating cabin: {ex.Message}");

            return StatusCode(500, new
            {
                Message = "An error occurred while updating the cabin",
                Error = ex.Message
            });
        }
    }

    private void DeleteOldImage(string? imagePath)
    {
        if (!string.IsNullOrEmpty(imagePath))
        {
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }
    }

    private async Task<string> SaveNewImageAsync(IFormFile image)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
        var imagesFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        if (!Directory.Exists(imagesFolderPath))
        {
            Directory.CreateDirectory(imagesFolderPath);
        }

        var filePath = Path.Combine(imagesFolderPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/images/{fileName}";
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
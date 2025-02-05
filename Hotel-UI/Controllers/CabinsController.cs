using ContactsManager.Core.DTO;
using Hotel_Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

public class CabinsController  : BaseController
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
    public async Task<IActionResult> Create(CabinUpsertRequest dto)
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

        return Ok(new
        {
            Message = "Cabin created successfully",
            Cabin = cabinResponse
        });
    }

    [HttpPut]
    public async Task<IActionResult> Edit(CabinUpsertRequest dto)
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
    public async Task<IActionResult> GetCabins()
    {
        var cabins = await _cabinsGetterService.GetCabins();
        return Ok(new { Cabins = cabins });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteCabin = await _cabinsDeleterService.DeleteCabin(id);
        return Ok(new { isDeleted = deleteCabin });
    }
}
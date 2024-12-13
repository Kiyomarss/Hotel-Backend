using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

[Route("[controller]")]
public class BookingsController  : Controller
{
    private readonly IBookingsDeleterService _bookingsDeleterService;
    private readonly IBookingsGetterService _bookingsGetterService;
    private readonly IBookingsUpdaterService _bookingsUpdaterService;

    public BookingsController(IBookingsDeleterService bookingsDeleterService, IBookingsGetterService bookingsGetterService, IBookingsUpdaterService bookingsUpdaterService)
    {
        _bookingsDeleterService = bookingsDeleterService;
        _bookingsGetterService = bookingsGetterService;
        _bookingsUpdaterService = bookingsUpdaterService;
    }

    [HttpPatch]
    [Route("[action]/{id}")]
    public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] JsonPatchDocument<Booking> patchDoc)
    {
        try
        {
            var updatedBooking = await _bookingsUpdaterService.UpdateBooking(id, patchDoc);
            return Ok(new
            {
                Message = "Booking updated successfully",
                Booking = updatedBooking
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
        }
    }

    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _bookingsGetterService.GetAllBookings();
        return Json(new { data = bookings });
    }
    
    
    [HttpDelete]
    [Route("[action]/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteBooking = await _bookingsDeleterService.DeleteBooking(id);
        return Json(new { isDeleted = deleteBooking });
    }
}
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

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetBookings(
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var bookingsResult = await _bookingsGetterService.GetBookings(status, sortBy, sortDirection, page, pageSize);

        return Ok(new
        {
            Bookings = bookingsResult.Items,
            TotalCount = bookingsResult.TotalCount
        });
    }

    [HttpGet]
    [Route("[action]/{id}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var bookingResponse = await _bookingsGetterService.GetBookingByBookingId(id);

        if (bookingResponse == null)
        {
            return NotFound(new { Message = "Booking not found" });
        }

        return Json(new { booking = bookingResponse });
    }
    
    [HttpPatch]
    [Route("[action]/{id}")]
    public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] JsonPatchDocument<Booking> patchDoc)
    {
        var updatedBooking = await _bookingsUpdaterService.UpdateBooking(id, patchDoc);
        return Ok(new
        {
            Message = "Booking updated successfully",
            Booking = updatedBooking
        });
    }
    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetBookingsAfterDate([FromQuery] DateTime date)
    {
        var bookingResponsesList = await _bookingsGetterService.GetBookingsAfterDate(date);
        return Ok(new { bookings = bookingResponsesList });
    }
    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetStaysAfterDate([FromQuery] DateTime date)
    {
        var bookingResponsesList = await _bookingsGetterService.GetStaysAfterDate(date);
        return Ok(new { bookings = bookingResponsesList });
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetStaysTodayActivity()
    {
        var bookingResponsesList = await _bookingsGetterService.GetStaysTodayActivity();
        return Ok(new { bookings = bookingResponsesList });
    }
    
    [HttpDelete]
    [Route("[action]/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isMessageSent = await _bookingsDeleterService.InitiateDeleteBooking(id);
        return Json(new { isDeleted = isMessageSent });
    }
}
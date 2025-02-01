using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

public class BookingsController  : BaseController
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var bookingResponse = await _bookingsGetterService.GetBookingByBookingId(id);

        if (bookingResponse == null)
        {
            return NotFound(new { Message = "Booking not found" });
        }

        return Ok(new { booking = bookingResponse });
    }
    
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateBooking(Guid id, JsonPatchDocument<Booking> patchDoc)
    {
        var updatedBooking = await _bookingsUpdaterService.UpdateBooking(id, patchDoc);
        return Ok(new
        {
            Message = "Booking updated successfully",
            Booking = updatedBooking
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBookingsAfterDate([FromQuery] DateTime date)
    {
        var bookingResponsesList = await _bookingsGetterService.GetBookingsAfterDate(date);
        return Ok(new { bookings = bookingResponsesList });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetStaysAfterDate([FromQuery] DateTime date)
    {
        var bookingResponsesList = await _bookingsGetterService.GetStaysAfterDate(date);
        return Ok(new { bookings = bookingResponsesList });
    }

    [HttpGet]
    public async Task<IActionResult> GetStaysTodayActivity()
    {
        var bookingResponsesList = await _bookingsGetterService.GetStaysTodayActivity();
        return Ok(new { bookings = bookingResponsesList });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteBooking = await _bookingsDeleterService.DeleteBooking(id);
        return Ok(new { isDeleted = deleteBooking });
    }
}
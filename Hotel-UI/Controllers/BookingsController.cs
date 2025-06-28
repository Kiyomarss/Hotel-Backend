using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
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
    public async Task<IActionResult> GetBookings([FromQuery] GetBookingsQuery query)
    {
        var bookingsResult = await _bookingsGetterService.GetBookings(query.Status, query.SortBy, query.SortDirection, query.Page, query.PageSize);
        return Ok(new BookingsResult(bookingsResult.Items, bookingsResult.TotalCount));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var result = await _bookingsGetterService.GetBookingByBookingId(id);

        if (result == null)
        {
            return NotFound(new MessageResponse("Booking not found"));
        }

        return Ok(result);
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
        return Ok(bookingResponsesList);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetStaysAfterDate([FromQuery] DateTime date)
    {
        var results = await _bookingsGetterService.GetStaysAfterDate(date);

        return Ok(results);
    }

    [HttpGet]
    public async Task<IActionResult> GetStaysTodayActivity()
    {
        var todayActivity = await _bookingsGetterService.GetStaysTodayActivity();

        return Ok(todayActivity);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteBooking = await _bookingsDeleterService.DeleteBooking(id);
        return Ok(new DeleteBookingResult(deleteBooking));
    }
}
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
        try
        {
            var bookingsResult = await _bookingsGetterService.GetBookings(status, sortBy, sortDirection, page, pageSize);

            return Ok(new
            {
                Bookings = bookingsResult.Bookings,
                TotalCount = bookingsResult.TotalCount
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error while fetching bookings: {ex.Message}");

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching bookings",
                Error = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("[action]/{id}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        try
        {
            var  bookingResponse = await _bookingsGetterService.GetBookingByBookingId(id);
            
            if (bookingResponse == null)
            {
                return NotFound(new { Message = "Booking not found" });
            }
            
            return Json(new { booking = bookingResponse });

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error while updating booking: {ex.Message}");

            return StatusCode(500, new
            {
                Message = "An error occurred while updating the booking",
                Error = ex.Message
            });
        }
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
    public async Task<IActionResult> GetBookingsAfterDate([FromQuery] DateTime date)
    {
        try
        {
            var bookingResponsesList = await _bookingsGetterService.GetBookingsAfterDate(date);
            return Ok(new { bookings = bookingResponsesList });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching bookings: {ex.Message}");
            return StatusCode(500, new
            {
                Message = "An error occurred while fetching bookings",
                Error = ex.Message
            });
        }
    }
    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetStaysAfterDate([FromQuery] DateTime date)
    {
        try
        {
            var bookingResponsesList = await _bookingsGetterService.GetStaysAfterDate(date);
            return Ok(new { bookings = bookingResponsesList });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching bookings: {ex.Message}");
            return StatusCode(500, new
            {
                Message = "An error occurred while fetching bookings",
                Error = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetStaysTodayActivity()
    {
        try
        {
            var bookingResponsesList = await _bookingsGetterService.GetStaysTodayActivity();
            return Ok(new { bookings = bookingResponsesList });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching bookings: {ex.Message}");
            return StatusCode(500, new
            {
                Message = "An error occurred while fetching bookings",
                Error = ex.Message
            });
        }
    }

    
    [HttpDelete]
    [Route("[action]/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteBooking = await _bookingsDeleterService.DeleteBooking(id);
        return Json(new { isDeleted = deleteBooking });
    }
}
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.AspNetCore.JsonPatch;

namespace ServiceContracts
{
    public interface IBookingsUpdaterService
    {
        Task<BookingResponse> InitiateUpdateBooking(Guid bookingId, JsonPatchDocument<Booking> patchDoc);
    };
}
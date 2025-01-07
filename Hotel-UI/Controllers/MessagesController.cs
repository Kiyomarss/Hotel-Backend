using Hotel_Core.Services.RabbitMQ;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_UI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly MessagingService _messagingService;

    public MessagesController(MessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    [HttpPost]
    [Route("send")]
    public IActionResult SendMessage([FromBody] string message)
    {
        _messagingService.SendMessageToQueue(message);

        return Ok("Message sent to queue");
    }
}
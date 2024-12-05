using Microsoft.AspNetCore.Mvc;

namespace Hotel_UI.Controllers
{
    [Route("[controller]")]
    public class FakeController : Controller
    {
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetFakeData()
        {
            var fakeData = new
            {
                Message = "This is fake data for testing.",
                Timestamp = DateTime.UtcNow
            };
            return Ok(fakeData);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult CreateFakeData([FromBody] object data)
        {
            return Created("Fake/CreateFakeData", new
            {
                Message = "Fake data created successfully.",
                ReceivedData = data,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
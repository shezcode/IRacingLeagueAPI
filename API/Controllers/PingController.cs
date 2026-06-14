using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        service = "IRacingLeague.API"
    });
}

using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult Get()
    {
        return Ok(new { status = "ok", message = "API está funcionando" });
    }
}

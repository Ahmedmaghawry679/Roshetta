using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Roshetta.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestingController : ControllerBase
    {
        [HttpGet("patient")]
        public async Task<IActionResult> GetPatientValue()
        {
            await Task.Delay(1000); // Simulate some work
            return Ok(new { message = "This is a protected endpoint." });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminValue()
        {
            await Task.Delay(1000); // Simulate some work
            return Ok(new { message = "This is an admin-only endpoint." });
        }
        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("doctor")]
        public async Task<IActionResult> GetDoctorValue()
        {
            await Task.Delay(1000); // Simulate some work
            return Ok(new { message = "This is a doctor-only endpoint." });
        }


    }
}

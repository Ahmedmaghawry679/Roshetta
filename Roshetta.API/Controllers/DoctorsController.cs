using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roshetta.API.Controllers.Helpers;
using Roshetta.API.Models;
using Roshetta.API.Models.DTOs;
using Roshetta.API.Services;

namespace Roshetta.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            this.doctorService = doctorService;
        }
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DoctorCreateResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddDoctor([FromBody] DoctorDTO dto)
        {
            var result = await doctorService.AddDoctorAsync(dto, User.GetUserId());
            return result.ToActionResult();
        }
    }
}

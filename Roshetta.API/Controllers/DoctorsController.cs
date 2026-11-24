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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CreateDoctorResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddDoctor([FromBody] RegisterDoctorDTO dto)
        {
            var result = await doctorService.AddDoctorAsync(dto, User.GetUserId());
            return result.ToActionResult();
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(DoctorDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDoctors([FromQuery] DoctorQueryParameters parameters)
        {
            var result = await doctorService.GetAllDoctorsAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(DoctorDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDoctorById(Guid id)
        {
            var result = await doctorService.GetDoctorByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            var result = await doctorService.DeleteDoctorAsync(id);
            return result.ToActionResult();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CreateDoctorResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] UpdateDoctorDTO dto)
        {
            var result = await doctorService.UpdateDoctorAsync(id, dto);
            return result.ToActionResult();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roshetta.API.Services;

namespace Roshetta.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SpecializationsController : ControllerBase
    {

        private readonly ISpecializationService specializationService;

        public SpecializationsController(ISpecializationService specializationService)
        {
            this.specializationService = specializationService;
        }


        [HttpGet]
        public async Task<IActionResult> GetSpecializations()
        {
            return Ok(await specializationService.GetAllSpecializationsAsync());
        }
    }
}

using AccountMicroservice.Api.Services.RolesServices;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        [Route("all")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
            => Ok(await roleService.GetAllRolesAsync());
    }
}

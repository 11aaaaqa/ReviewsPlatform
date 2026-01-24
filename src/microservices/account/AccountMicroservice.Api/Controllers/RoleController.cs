using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.Services.RolesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [Route("all")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
            => Ok(await roleService.GetAllRolesAsync());
    }
}

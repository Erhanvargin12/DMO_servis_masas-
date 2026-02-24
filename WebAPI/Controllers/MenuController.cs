using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IMenuOperationClaimService _menuOperationClaimService;

        public MenuController(IMenuService menuService, IMenuOperationClaimService menuOperationClaimService)
        {
            _menuService = menuService;
            _menuOperationClaimService = menuOperationClaimService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _menuService.GetAll();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyid/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _menuService.GetById(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getinactiveurls")]
        public IActionResult GetInactiveUrls()
        {
            var result = _menuService.GetInactiveMenuUrls();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyuserid/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var result = _menuService.GetMenusByUserId(userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("add")]
        public IActionResult Add(Menu menu)
        {
            var result = _menuService.Add(menu);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(Menu menu)
        {
            var result = _menuService.Update(menu);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(Menu menu)
        {
            var result = _menuService.Delete(menu);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getclaims/{menuId}")]
        public IActionResult GetClaims(int menuId)
        {
            var result = _menuOperationClaimService.GetByMenuId(menuId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("addclaim")]
        public IActionResult AddClaim(MenuOperationClaim menuOperationClaim)
        {
            var result = _menuOperationClaimService.Add(menuOperationClaim);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("deleteclaim")]
        public IActionResult DeleteClaim(MenuOperationClaim menuOperationClaim)
        {
            var result = _menuOperationClaimService.Delete(menuOperationClaim);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}


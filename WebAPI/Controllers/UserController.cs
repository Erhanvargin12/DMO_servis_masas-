using Business.Abstract;
using Core.Utilities.Security.Hashing;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _userService.GetAll();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyid/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _userService.GetAll(u => u.Id == id);
            if (result.Success && result.Data.Any())
            {
                return Ok(new { Success = true, Data = result.Data.First() });
            }
            return BadRequest(new { Success = false, Message = "Kullanıcı bulunamadı" });
        }
        
        [HttpPost("add")]
        public IActionResult Add(UserAddDto userDto)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(userDto.Password ?? "123456", out passwordHash, out passwordSalt);
            
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Mail = userDto.Mail,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = userDto.Status,
                RegistirationId = userDto.RegistirationId,
                OrganizationUnitId = userDto.OrganizationUnitId,
                PositionId = userDto.PositionId,
                CreateAt = DateTime.Now
            };
            
            var result = _userService.Add(user);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpPost("update")]
        public IActionResult Update(UserUpdateDto userDto)
        {
            var user = new User
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Mail = userDto.Mail,
                Status = userDto.Status,
                RegistirationId = userDto.RegistirationId,
                OrganizationUnitId = userDto.OrganizationUnitId,
                PositionId = userDto.PositionId
            };
            
            var result = _userService.Update(user);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpPost("delete")]
        public IActionResult Delete(UserDeleteDto userDto)
        {
            var user = new User { Id = userDto.Id };
            
            var result = _userService.Delete(user);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}

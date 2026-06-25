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
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetUserAsync(u => u.Id == id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(new { Success = false, Message = "Kullanıcı bulunamadı" });
        }
        
        [HttpPost("add")]
        public async Task<IActionResult> Add(UserAddDto userDto)
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
            
            var result = await _userService.AddAsync(user);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpPost("update")]
        public async Task<IActionResult> Update(UserUpdateDto userDto)
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
            
            var result = await _userService.UpdateAsync(user);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(UserDeleteDto userDto)
        {
            var user = new User { Id = userDto.Id };
            
            var result = await _userService.DeleteAsync(user);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userResult = await _userService.GetUserAsync(u => u.Id == dto.UserId);
            if (!userResult.Success || userResult.Data == null)
            {
                return BadRequest(new { Success = false, Message = "Kullanıcı bulunamadı" });
            }

            var user = userResult.Data;

            if (!HashingHelper.VerifyPasswordHash(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { Success = false, Message = "Mevcut şifre hatalı" });
            }

            HashingHelper.CreatePasswordHash(dto.NewPassword, out byte[] newHash, out byte[] newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.UpdateAt = DateTime.Now;

            var result = await _userService.UpdateAsync(user);
            if (result.Success)
            {
                return Ok(new { Success = true, Message = "Şifre başarıyla değiştirildi" });
            }
            return BadRequest(new { Success = false, Message = result.Message });
        }
    }
}

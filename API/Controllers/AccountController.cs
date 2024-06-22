using System.Security.Cryptography;
using System.Text;
using API.Controllers;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto register)
    {
        if (await UserExists(register.UserName) == true)
        {
            return BadRequest("Username is taken");
        }
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = register.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserDto
        {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto login)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserName == login.UserName.ToLower());

        if (user == null)
            return Unauthorized("Invalid Username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

        for (int i = 0; i < computeHash.Length; i++)
        {
            if (computeHash[i] != user.PasswordHash[i])
                return Unauthorized("Invalid password");
        }
        return new UserDto
        {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists(string userName)
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
    }
}

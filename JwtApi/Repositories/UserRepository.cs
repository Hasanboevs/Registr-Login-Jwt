using _SharedClass.DTOs;
using _SharedClass.Interface;
using _SharedClass;
using JwtApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static _SharedClass.Services.ServiceResponse;

namespace JwtApi.Repositories
{
    public class UserRepository : IUserAccount
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task<GeneralResponse> CreateAccount(UserDto user)
        {
            if (user == null)
                return new GeneralResponse(false, "Model is empty");

            var newUser = new ApplicationUser()
            {
                Name = user.Name,
                Email = user.Email,
                PasswordHash = user.Password,
                UserName = user.Email
            };

            var existingUser = await _userManager.FindByEmailAsync(newUser.Email);
            if (existingUser != null)
                return new GeneralResponse(false, "User already registered");

            var createUserResult = await _userManager.CreateAsync(newUser, user.Password);
            if (!createUserResult.Succeeded)
                return new GeneralResponse(false, "Error occurred. Please try again.");

            // Check if the "Admin" role exists, if not, create it
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _userManager.AddToRoleAsync(newUser, "Admin");
            }
            else
            {
                // If the "Admin" role exists, assign the "User" role to the user
                await _userManager.AddToRoleAsync(newUser, "User");
            }

            return new GeneralResponse(true, "Account created");
        }

        public async Task<LoginResponse> LoginAccount(LogInDto login)
        {
            if (login == null)
                return new LoginResponse(false, null, "Login container is empty");

            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
                return new LoginResponse(false, null, "User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            var userSession = new UserSession(user.Id, user.Name, user.Email, userRoles.FirstOrDefault());

            string token = GenerateToken(userSession);
            return new LoginResponse(true, token, "Login completed!");
        }

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

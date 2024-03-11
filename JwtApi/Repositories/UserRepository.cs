using _SharedClass;
using _SharedClass.DTOs;
using _SharedClass.Interface;
using JwtApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static _SharedClass.Services.ServiceResponse;

namespace JwtApi.Repositories
{
    public class UserRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config
        ) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAccount(UserDto user)
        {
            if (user == null) return new GeneralResponse(false, "Model is empty");
            var newUser = new ApplicationUser()
            {
                Name = user.Name,
                Email = user.Email,
                PasswordHash = user.Password,
                UserName = user.Email
            };

            var usr = await userManager.FindByEmailAsync(newUser.Email);
            if (usr != null) return new GeneralResponse(false, "User registered already");

            var CreateUser = await userManager.CreateAsync(newUser!, user.Password);
            if (!CreateUser.Succeeded) return new GeneralResponse(false, "Error occured ... Please try again");


            var checkAdmin = await roleManager.FindByNameAsync("Admin");
            if (checkAdmin is null)
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
                await userManager.AddToRoleAsync(newUser, "Admin");
                return new GeneralResponse(true, "Account created");
            }
            else
            {

                var checkUser = await roleManager.FindByNameAsync("User");
                if (checkUser is null)
                    await roleManager.CreateAsync(new IdentityRole() { Name = "User" });

                await userManager.AddToRoleAsync(newUser, "User");
                return new GeneralResponse(true, "Account Created");

            }
        }

        public async Task<LoginResponse> LoginAccount(LogInDto login)
        {
            if (login == null)
                return new LoginResponse(false, null!, "Login contianer is empty");

            var getUser = await userManager.FindByEmailAsync(login.Email);
            if (getUser is null)
                return new LoginResponse(false, null!, "User not found");

            var getUserRole = await userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Id, getUser.Name, getUser.Email, getUserRole.First());


            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Login completed!");
        }

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt: Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience : config["Jwt: Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials : credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

using _SharedClass.DTOs;
using static _SharedClass.Services.ServiceResponse;

namespace _SharedClass.Interface;

public interface IUserAccount
{
    Task<GeneralResponse> CreateAccount(UserDto user);
    Task<LoginResponse> LoginAccount(LogInDto login);

}

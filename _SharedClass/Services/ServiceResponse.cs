namespace _SharedClass.Services
{
    public class ServiceResponse
    {
        public record class GeneralResponse(bool Flag, string Message);
        public record class LoginResponse(bool Flag, string Token,  string Message);
    }
}

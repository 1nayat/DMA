using DMA.Entities;

namespace DMA.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginUserDto request);
        Task<User?> RegisterAsync(RegisterUserDto request);
    }
}
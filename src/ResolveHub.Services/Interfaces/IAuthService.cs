namespace ResolveHub.Services.Interfaces;

using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
}

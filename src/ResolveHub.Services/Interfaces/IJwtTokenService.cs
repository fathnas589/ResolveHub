namespace ResolveHub.Services.Interfaces;

using System;
using ResolveHub.Core.Entities;

public interface IJwtTokenService
{
    (string token, DateTime expiresAt) GenerateToken(UserLoginRecord user);
}

namespace ResolveHub.Services.Implementations;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResolveHub.Core.Entities;
using ResolveHub.Services.Interfaces;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, DateTime expiresAt) GenerateToken(UserLoginRecord user)
    {
        ArgumentNullException.ThrowIfNull(user);

        string issuer = _configuration["Jwt:Issuer"] ?? "ResolveHub";
        string audience = _configuration["Jwt:Audience"] ?? "ResolveHubClient";
        string signingKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured in 'Jwt:Key'.");

        int expiryMinutes = 60;
        if (int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int parsedExpiry) && parsedExpiry > 0)
        {
            expiryMinutes = parsedExpiry;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.GId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.Id.ToString()),
            new("userName", user.UserName),
            new("fullName", user.FullName),
            new("email", user.Email),
            new("companyId", user.CompanyId.ToString()),
            new(ClaimTypes.Role, user.RoleCode),
            new("role", user.RoleCode)
        };

        if (user.DepartmentGId.HasValue && user.DepartmentGId.Value != Guid.Empty)
        {
            claims.Add(new Claim("departmentGId", user.DepartmentGId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.DepartmentName))
        {
            claims.Add(new Claim("departmentName", user.DepartmentName));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (tokenString, expiresAt);
    }
}

namespace ResolveHub.Services.Implementations;

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ResolveHub.Services.Interfaces;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public int UserId
    {
        get
        {
            var claimVal = User?.FindFirstValue("userId");
            return int.TryParse(claimVal, out int id) ? id : 0;
        }
    }

    public Guid UserGId
    {
        get
        {
            var claimVal = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");
            return Guid.TryParse(claimVal, out var gid) ? gid : Guid.Empty;
        }
    }

    public int CompanyId
    {
        get
        {
            var claimVal = User?.FindFirstValue("companyId");
            return int.TryParse(claimVal, out int cid) ? cid : 0;
        }
    }

    public string UserName => User?.FindFirstValue("userName") ?? User?.Identity?.Name ?? string.Empty;

    public string FullName => User?.FindFirstValue("fullName") ?? string.Empty;

    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email") ?? string.Empty;

    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? User?.FindFirstValue("role") ?? string.Empty;

    public Guid? DepartmentGId
    {
        get
        {
            var claimVal = User?.FindFirstValue("departmentGId");
            return Guid.TryParse(claimVal, out var deptGId) ? deptGId : null;
        }
    }
}

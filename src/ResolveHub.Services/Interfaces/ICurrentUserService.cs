namespace ResolveHub.Services.Interfaces;

using System;

public interface ICurrentUserService
{
    int UserId { get; }
    Guid UserGId { get; }
    int CompanyId { get; }
    string UserName { get; }
    string FullName { get; }
    string Email { get; }
    string Role { get; }
    Guid? DepartmentGId { get; }
    bool IsAuthenticated { get; }
}

namespace ResolveHub.Core.DTOs.Users;

using System;

public class UserListRequestDto
{
    public Guid? DepartmentGId { get; set; }
    public string? RoleCode { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserListRequest : UserListRequestDto
{
}

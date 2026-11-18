CREATE OR ALTER PROCEDURE dbo.UserMaster_GetForLogin
    @CompanyId          INT,
    @UserNameOrEmail    NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        u.Id,
        u.GId,
        u.CompanyId,
        u.DepartmentId,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        u.UserName,
        u.FullName,
        u.Email,
        u.PasswordHash,
        u.RoleCode,
        u.IsActive,
        u.IsDeleted
    FROM dbo.UserMaster u
    LEFT JOIN dbo.Department d ON u.DepartmentId = d.Id
    WHERE u.CompanyId = @CompanyId
      AND u.IsDeleted = 0
      AND (u.UserName = @UserNameOrEmail OR u.Email = @UserNameOrEmail)
    ORDER BY u.Id DESC;
END;

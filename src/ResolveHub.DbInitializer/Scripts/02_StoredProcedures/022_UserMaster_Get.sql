CREATE OR ALTER PROCEDURE dbo.UserMaster_Get
    @CompanyId  INT,
    @GId        UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id,
        u.GId,
        u.UserName,
        u.FullName,
        u.Email,
        u.Phone,
        u.RoleCode,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        u.CompanyId,
        u.IsActive,
        u.Created,
        u.LastUpd
    FROM dbo.UserMaster u
    LEFT JOIN dbo.Department d ON u.DepartmentId = d.Id
    WHERE u.GId = @GId
      AND u.CompanyId = @CompanyId
      AND u.IsDeleted = 0;
END;

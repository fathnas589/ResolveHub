CREATE OR ALTER PROCEDURE dbo.SlaPolicy_Get
    @CompanyId  INT,
    @GId        UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.GId,
        p.CompanyId,
        p.Name,
        p.Description,
        p.DepartmentId,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        p.CategoryId,
        c.GId AS CategoryGId,
        c.CategoryName,
        p.SubCategoryId,
        sc.GId AS SubCategoryGId,
        sc.SubCategoryName,
        p.Priority,
        p.TicketType,
        p.FirstResponseMinutes,
        p.ResolutionMinutes,
        p.EscalationBeforeMinutes,
        p.IsDefault,
        p.IsActive,
        p.IsDeleted,
        p.Created,
        p.CreatedBy,
        u1.FullName AS CreatedByName,
        p.LastUpd,
        p.LastUpdBy,
        u2.FullName AS LastUpdByName
    FROM dbo.SlaPolicy p
    LEFT JOIN dbo.Department d ON p.DepartmentId = d.Id
    LEFT JOIN dbo.Category c ON p.CategoryId = c.Id
    LEFT JOIN dbo.SubCategory sc ON p.SubCategoryId = sc.Id
    LEFT JOIN dbo.UserMaster u1 ON p.CreatedBy = u1.Id
    LEFT JOIN dbo.UserMaster u2 ON p.LastUpdBy = u2.Id
    WHERE p.GId = @GId
      AND p.CompanyId = @CompanyId
      AND p.IsDeleted = 0;
END;

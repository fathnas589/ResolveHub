CREATE OR ALTER PROCEDURE dbo.Category_Get
    @CompanyId  INT,
    @GId        UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id,
        c.GId,
        c.CategoryName,
        c.Description,
        c.DepartmentId,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        c.CompanyId,
        c.Created,
        c.CreatedBy,
        c.LastUpd,
        c.LastUpdBy,
        c.IsActive,
        c.IsDeleted
    FROM dbo.Category c
    INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
    WHERE c.GId = @GId
      AND c.CompanyId = @CompanyId
      AND c.IsDeleted = 0;
END;

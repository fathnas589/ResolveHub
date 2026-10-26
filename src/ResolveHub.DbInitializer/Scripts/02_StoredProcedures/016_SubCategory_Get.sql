CREATE OR ALTER PROCEDURE dbo.SubCategory_Get
    @CompanyId  INT,
    @GId        UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id,
        s.GId,
        s.SubCategoryName,
        s.Description,
        s.CategoryId,
        c.GId AS CategoryGId,
        c.CategoryName,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        s.CompanyId,
        s.Created,
        s.CreatedBy,
        s.LastUpd,
        s.LastUpdBy,
        s.IsActive,
        s.IsDeleted
    FROM dbo.SubCategory s
    INNER JOIN dbo.Category c ON s.CategoryId = c.Id
    INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
    WHERE s.GId = @GId
      AND s.CompanyId = @CompanyId
      AND s.IsDeleted = 0;
END;

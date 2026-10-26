CREATE OR ALTER PROCEDURE dbo.Category_DuplicateCheck
    @CompanyId      INT,
    @DepartmentGId  UNIQUEIDENTIFIER,
    @CategoryName   NVARCHAR(100),
    @ExcludeGId     UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Category c
            INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
            WHERE c.CompanyId = @CompanyId
              AND d.GId = @DepartmentGId
              AND c.CategoryName = @CategoryName
              AND c.IsDeleted = 0
              AND (@ExcludeGId IS NULL OR c.GId <> @ExcludeGId)
        ) THEN 1 ELSE 0 END AS BIT
    ) AS IsDuplicate;
END;

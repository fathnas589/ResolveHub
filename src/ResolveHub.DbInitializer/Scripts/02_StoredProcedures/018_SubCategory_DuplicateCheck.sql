CREATE OR ALTER PROCEDURE dbo.SubCategory_DuplicateCheck
    @CompanyId          INT,
    @CategoryGId        UNIQUEIDENTIFIER,
    @SubCategoryName    NVARCHAR(100),
    @ExcludeGId         UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.SubCategory s
            INNER JOIN dbo.Category c ON s.CategoryId = c.Id
            WHERE s.CompanyId = @CompanyId
              AND c.GId = @CategoryGId
              AND s.SubCategoryName = @SubCategoryName
              AND s.IsDeleted = 0
              AND (@ExcludeGId IS NULL OR s.GId <> @ExcludeGId)
        ) THEN 1 ELSE 0 END AS BIT
    ) AS IsDuplicate;
END;

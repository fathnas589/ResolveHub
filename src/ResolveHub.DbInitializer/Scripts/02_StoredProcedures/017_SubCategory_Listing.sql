CREATE OR ALTER PROCEDURE dbo.SubCategory_Listing
    @CompanyId      INT,
    @CategoryGId    UNIQUEIDENTIFIER = NULL,
    @DepartmentGId  UNIQUEIDENTIFIER = NULL,
    @SearchTerm     NVARCHAR(100) = NULL,
    @IsActive       BIT = NULL,
    @PageNumber     INT = 1,
    @PageSize       INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalize pagination parameters
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH FilteredSubCategories AS
    (
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
            s.IsDeleted,
            COUNT(1) OVER() AS TotalCount
        FROM dbo.SubCategory s
        INNER JOIN dbo.Category c ON s.CategoryId = c.Id
        INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
        WHERE s.CompanyId = @CompanyId
          AND s.IsDeleted = 0
          AND c.IsDeleted = 0
          AND d.IsDeleted = 0
          AND (@CategoryGId IS NULL OR c.GId = @CategoryGId)
          AND (@DepartmentGId IS NULL OR d.GId = @DepartmentGId)
          AND (@IsActive IS NULL OR s.IsActive = @IsActive)
          AND (
                @SearchTerm IS NULL 
                OR @SearchTerm = '' 
                OR s.SubCategoryName LIKE '%' + @SearchTerm + '%'
                OR s.Description LIKE '%' + @SearchTerm + '%'
                OR c.CategoryName LIKE '%' + @SearchTerm + '%'
                OR d.DepartmentName LIKE '%' + @SearchTerm + '%'
              )
    )
    SELECT
        f.Id,
        f.GId,
        f.SubCategoryName,
        f.Description,
        f.CategoryId,
        f.CategoryGId,
        f.CategoryName,
        f.DepartmentGId,
        f.DepartmentName,
        f.CompanyId,
        f.Created,
        f.CreatedBy,
        f.LastUpd,
        f.LastUpdBy,
        f.IsActive,
        f.IsDeleted,
        f.TotalCount
    FROM FilteredSubCategories f
    ORDER BY f.DepartmentName ASC, f.CategoryName ASC, f.SubCategoryName ASC, f.Id ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

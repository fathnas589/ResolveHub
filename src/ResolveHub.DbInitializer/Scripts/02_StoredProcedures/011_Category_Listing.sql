CREATE OR ALTER PROCEDURE dbo.Category_Listing
    @CompanyId      INT,
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

    WITH FilteredCategories AS
    (
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
            c.IsDeleted,
            COUNT(1) OVER() AS TotalCount
        FROM dbo.Category c
        INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
        WHERE c.CompanyId = @CompanyId
          AND c.IsDeleted = 0
          AND d.IsDeleted = 0
          AND (@DepartmentGId IS NULL OR d.GId = @DepartmentGId)
          AND (@IsActive IS NULL OR c.IsActive = @IsActive)
          AND (
                @SearchTerm IS NULL 
                OR @SearchTerm = '' 
                OR c.CategoryName LIKE '%' + @SearchTerm + '%'
                OR c.Description LIKE '%' + @SearchTerm + '%'
                OR d.DepartmentName LIKE '%' + @SearchTerm + '%'
              )
    )
    SELECT
        f.Id,
        f.GId,
        f.CategoryName,
        f.Description,
        f.DepartmentId,
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
    FROM FilteredCategories f
    ORDER BY f.DepartmentName ASC, f.CategoryName ASC, f.Id ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

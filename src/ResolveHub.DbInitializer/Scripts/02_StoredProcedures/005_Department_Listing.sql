CREATE OR ALTER PROCEDURE dbo.Department_Listing
    @CompanyId   INT,
    @SearchTerm  NVARCHAR(100) = NULL,
    @IsActive    BIT = NULL,
    @PageNumber  INT = 1,
    @PageSize    INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalize pagination parameters
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH FilteredDepartments AS
    (
        SELECT
            d.Id,
            d.GId,
            d.DepartmentName,
            d.Description,
            d.CompanyId,
            d.Created,
            d.CreatedBy,
            d.LastUpd,
            d.LastUpdBy,
            d.IsActive,
            d.IsDeleted,
            COUNT(1) OVER() AS TotalCount
        FROM dbo.Department d
        WHERE d.CompanyId = @CompanyId
          AND d.IsDeleted = 0
          AND (@IsActive IS NULL OR d.IsActive = @IsActive)
          AND (
                @SearchTerm IS NULL 
                OR @SearchTerm = '' 
                OR d.DepartmentName LIKE '%' + @SearchTerm + '%'
                OR d.Description LIKE '%' + @SearchTerm + '%'
              )
    )
    SELECT
        f.Id,
        f.GId,
        f.DepartmentName,
        f.Description,
        f.CompanyId,
        f.Created,
        f.CreatedBy,
        f.LastUpd,
        f.LastUpdBy,
        f.IsActive,
        f.IsDeleted,
        f.TotalCount
    FROM FilteredDepartments f
    ORDER BY f.DepartmentName ASC, f.Id ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

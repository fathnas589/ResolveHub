CREATE OR ALTER PROCEDURE dbo.UserMaster_Listing
    @CompanyId      INT,
    @DepartmentGId  UNIQUEIDENTIFIER = NULL,
    @RoleCode       NVARCHAR(50) = NULL,
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

    WITH FilteredUsers AS
    (
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
            u.LastUpd,
            COUNT(1) OVER() AS TotalCount
        FROM dbo.UserMaster u
        LEFT JOIN dbo.Department d ON u.DepartmentId = d.Id
        WHERE u.CompanyId = @CompanyId
          AND u.IsDeleted = 0
          AND (@DepartmentGId IS NULL OR d.GId = @DepartmentGId)
          AND (@RoleCode IS NULL OR u.RoleCode = @RoleCode)
          AND (@IsActive IS NULL OR u.IsActive = @IsActive)
          AND (
                @SearchTerm IS NULL 
                OR @SearchTerm = '' 
                OR u.UserName LIKE '%' + @SearchTerm + '%'
                OR u.FullName LIKE '%' + @SearchTerm + '%'
                OR u.Email LIKE '%' + @SearchTerm + '%'
                OR u.Phone LIKE '%' + @SearchTerm + '%'
                OR d.DepartmentName LIKE '%' + @SearchTerm + '%'
              )
    )
    SELECT
        f.Id,
        f.GId,
        f.UserName,
        f.FullName,
        f.Email,
        f.Phone,
        f.RoleCode,
        f.DepartmentGId,
        f.DepartmentName,
        f.CompanyId,
        f.IsActive,
        f.Created,
        f.LastUpd,
        f.TotalCount
    FROM FilteredUsers f
    ORDER BY f.FullName ASC, f.UserName ASC, f.Id ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

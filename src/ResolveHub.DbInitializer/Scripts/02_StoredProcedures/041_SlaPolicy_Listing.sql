CREATE OR ALTER PROCEDURE dbo.SlaPolicy_Listing
    @CompanyId      INT,
    @SearchTerm     NVARCHAR(100) = NULL,
    @DepartmentGId  UNIQUEIDENTIFIER = NULL,
    @CategoryGId    UNIQUEIDENTIFIER = NULL,
    @Priority       NVARCHAR(20) = NULL,
    @TicketType     NVARCHAR(30) = NULL,
    @IsActive       BIT = NULL,
    @PageNumber     INT = 1,
    @PageSize       INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH FilteredPolicies AS
    (
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
            u2.FullName AS LastUpdByName,
            COUNT(1) OVER() AS TotalCount
        FROM dbo.SlaPolicy p
        LEFT JOIN dbo.Department d ON p.DepartmentId = d.Id
        LEFT JOIN dbo.Category c ON p.CategoryId = c.Id
        LEFT JOIN dbo.SubCategory sc ON p.SubCategoryId = sc.Id
        LEFT JOIN dbo.UserMaster u1 ON p.CreatedBy = u1.Id
        LEFT JOIN dbo.UserMaster u2 ON p.LastUpdBy = u2.Id
        WHERE p.CompanyId = @CompanyId
          AND p.IsDeleted = 0
          AND (@DepartmentGId IS NULL OR d.GId = @DepartmentGId)
          AND (@CategoryGId IS NULL OR c.GId = @CategoryGId)
          AND (@Priority IS NULL OR p.Priority = @Priority)
          AND (@TicketType IS NULL OR p.TicketType = @TicketType)
          AND (@IsActive IS NULL OR p.IsActive = @IsActive)
          AND (
                @SearchTerm IS NULL 
                OR @SearchTerm = '' 
                OR p.Name LIKE '%' + @SearchTerm + '%'
                OR p.Description LIKE '%' + @SearchTerm + '%'
                OR d.DepartmentName LIKE '%' + @SearchTerm + '%'
                OR c.CategoryName LIKE '%' + @SearchTerm + '%'
                OR sc.SubCategoryName LIKE '%' + @SearchTerm + '%'
              )
    )
    SELECT
        f.Id,
        f.GId,
        f.CompanyId,
        f.Name,
        f.Description,
        f.DepartmentId,
        f.DepartmentGId,
        f.DepartmentName,
        f.CategoryId,
        f.CategoryGId,
        f.CategoryName,
        f.SubCategoryId,
        f.SubCategoryGId,
        f.SubCategoryName,
        f.Priority,
        f.TicketType,
        f.FirstResponseMinutes,
        f.ResolutionMinutes,
        f.EscalationBeforeMinutes,
        f.IsDefault,
        f.IsActive,
        f.IsDeleted,
        f.Created,
        f.CreatedBy,
        f.CreatedByName,
        f.LastUpd,
        f.LastUpdBy,
        f.LastUpdByName,
        f.TotalCount
    FROM FilteredPolicies f
    ORDER BY f.IsDefault DESC, f.Created DESC, f.Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

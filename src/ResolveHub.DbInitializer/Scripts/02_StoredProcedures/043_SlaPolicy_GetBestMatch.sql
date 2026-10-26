CREATE OR ALTER PROCEDURE dbo.SlaPolicy_GetBestMatch
    @CompanyId      INT,
    @DepartmentId   INT = NULL,
    @CategoryId     INT = NULL,
    @SubCategoryId  INT = NULL,
    @Priority       NVARCHAR(20) = NULL,
    @TicketType     NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
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
        p.LastUpd,
        p.LastUpdBy,
        (
            (CASE WHEN p.SubCategoryId IS NOT NULL THEN 32 ELSE 0 END) +
            (CASE WHEN p.CategoryId IS NOT NULL THEN 16 ELSE 0 END) +
            (CASE WHEN p.DepartmentId IS NOT NULL THEN 8 ELSE 0 END) +
            (CASE WHEN p.Priority IS NOT NULL THEN 4 ELSE 0 END) +
            (CASE WHEN p.TicketType IS NOT NULL THEN 2 ELSE 0 END) +
            (CASE WHEN p.IsDefault = 1 THEN 1 ELSE 0 END)
        ) AS SpecificityScore
    FROM dbo.SlaPolicy p
    LEFT JOIN dbo.Department d ON p.DepartmentId = d.Id
    LEFT JOIN dbo.Category c ON p.CategoryId = c.Id
    LEFT JOIN dbo.SubCategory sc ON p.SubCategoryId = sc.Id
    WHERE p.CompanyId = @CompanyId
      AND p.IsActive = 1
      AND p.IsDeleted = 0
      AND (p.DepartmentId IS NULL OR p.DepartmentId = @DepartmentId)
      AND (p.CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (p.SubCategoryId IS NULL OR p.SubCategoryId = @SubCategoryId)
      AND (p.Priority IS NULL OR p.Priority = @Priority)
      AND (p.TicketType IS NULL OR p.TicketType = @TicketType)
    ORDER BY
        SpecificityScore DESC,
        ISNULL(p.LastUpd, p.Created) DESC,
        p.Created DESC,
        p.Id ASC;
END;

CREATE OR ALTER PROCEDURE dbo.Ticket_Listing
    @CompanyId              INT,
    @RequestingUserId       INT,
    @RequestingRole         NVARCHAR(50),
    @PageNumber             INT = 1,
    @PageSize               INT = 10,
    @TicketType             NVARCHAR(30) = NULL,
    @Status                 NVARCHAR(30) = NULL,
    @Priority               NVARCHAR(20) = NULL,
    @DepartmentGId          UNIQUEIDENTIFIER = NULL,
    @CategoryGId            UNIQUEIDENTIFIER = NULL,
    @SubCategoryGId         UNIQUEIDENTIFIER = NULL,
    @AssignedToUserGId      UNIQUEIDENTIFIER = NULL,
    @CreatedByUserGId       UNIQUEIDENTIFIER = NULL,
    @SearchTerm             NVARCHAR(100) = NULL,
    @CreatedFrom            DATETIME2 = NULL,
    @CreatedTo              DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalize pagination parameters
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Agent department lookup
    DECLARE @AgentDepartmentId INT = NULL;
    IF @RequestingRole = 'AGENT'
    BEGIN
        SELECT @AgentDepartmentId = DepartmentId 
        FROM dbo.UserMaster 
        WHERE Id = @RequestingUserId AND CompanyId = @CompanyId;
    END;

    WITH FilteredTickets AS
    (
        SELECT
            t.Id,
            t.GId,
            t.TicketNumber,
            t.TicketType,
            t.Subject,
            t.Description,
            t.Priority,
            t.Status,
            d.GId AS DepartmentGId,
            d.DepartmentName,
            c.GId AS CategoryGId,
            c.CategoryName,
            sc.GId AS SubCategoryGId,
            sc.SubCategoryName,
            uCreator.GId AS CreatedByUserGId,
            uCreator.UserName AS CreatedByUserName,
            uCreator.FullName AS CreatedByFullName,
            uAssignee.GId AS AssignedToUserGId,
            uAssignee.UserName AS AssignedToUserName,
            uAssignee.FullName AS AssignedToFullName,
            t.CompanyId,
            t.Created,
            t.LastUpd,
            t.ClosedAt,
            COUNT(1) OVER() AS TotalCount
        FROM dbo.Ticket t
        INNER JOIN dbo.Department d ON t.DepartmentId = d.Id
        INNER JOIN dbo.Category c ON t.CategoryId = c.Id
        LEFT JOIN dbo.SubCategory sc ON t.SubCategoryId = sc.Id
        INNER JOIN dbo.UserMaster uCreator ON t.CreatedByUserId = uCreator.Id
        LEFT JOIN dbo.UserMaster uAssignee ON t.AssignedToUserId = uAssignee.Id
        WHERE t.CompanyId = @CompanyId
          AND t.IsDeleted = 0
          -- Role Access
          AND (
              @RequestingRole = 'ADMIN'
              OR (@RequestingRole = 'USER' AND t.CreatedByUserId = @RequestingUserId)
              OR (@RequestingRole = 'AGENT' AND (
                    t.AssignedToUserId = @RequestingUserId 
                    OR (t.AssignedToUserId IS NULL AND (@AgentDepartmentId IS NULL OR t.DepartmentId = @AgentDepartmentId))
                 ))
          )
          -- Optional Filters
          AND (@TicketType IS NULL OR t.TicketType = @TicketType)
          AND (@Status IS NULL OR t.Status = @Status)
          AND (@Priority IS NULL OR t.Priority = @Priority)
          AND (@DepartmentGId IS NULL OR d.GId = @DepartmentGId)
          AND (@CategoryGId IS NULL OR c.GId = @CategoryGId)
          AND (@SubCategoryGId IS NULL OR sc.GId = @SubCategoryGId)
          AND (@AssignedToUserGId IS NULL OR uAssignee.GId = @AssignedToUserGId)
          AND (@CreatedByUserGId IS NULL OR uCreator.GId = @CreatedByUserGId)
          AND (@CreatedFrom IS NULL OR t.Created >= @CreatedFrom)
          AND (@CreatedTo IS NULL OR t.Created <= @CreatedTo)
          AND (
              @SearchTerm IS NULL
              OR @SearchTerm = ''
              OR t.TicketNumber LIKE '%' + @SearchTerm + '%'
              OR t.Subject LIKE '%' + @SearchTerm + '%'
              OR t.Description LIKE '%' + @SearchTerm + '%'
              OR uCreator.UserName LIKE '%' + @SearchTerm + '%'
              OR uCreator.FullName LIKE '%' + @SearchTerm + '%'
              OR uAssignee.UserName LIKE '%' + @SearchTerm + '%'
              OR uAssignee.FullName LIKE '%' + @SearchTerm + '%'
          )
    )
    SELECT
        f.Id,
        f.GId,
        f.TicketNumber,
        f.TicketType,
        f.Subject,
        f.Description,
        f.Priority,
        f.Status,
        f.DepartmentGId,
        f.DepartmentName,
        f.CategoryGId,
        f.CategoryName,
        f.SubCategoryGId,
        f.SubCategoryName,
        f.CreatedByUserGId,
        f.CreatedByUserName,
        f.CreatedByFullName,
        f.AssignedToUserGId,
        f.AssignedToUserName,
        f.AssignedToFullName,
        f.CompanyId,
        f.Created,
        f.LastUpd,
        f.ClosedAt,
        f.TotalCount
    FROM FilteredTickets f
    ORDER BY f.Created DESC, f.Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

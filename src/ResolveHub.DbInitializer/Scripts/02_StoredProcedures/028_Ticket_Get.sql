CREATE OR ALTER PROCEDURE dbo.Ticket_Get
    @GId                UNIQUEIDENTIFIER,
    @CompanyId          INT,
    @RequestingUserId   INT,
    @RequestingRole     NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get requesting agent department if AGENT
    DECLARE @AgentDepartmentId INT = NULL;
    IF @RequestingRole = 'AGENT'
    BEGIN
        SELECT @AgentDepartmentId = DepartmentId 
        FROM dbo.UserMaster 
        WHERE Id = @RequestingUserId AND CompanyId = @CompanyId;
    END;

    SELECT
        t.Id,
        t.GId,
        t.TicketNumber,
        t.CompanyId,
        t.CreatedByUserId,
        t.DepartmentId,
        t.CategoryId,
        t.SubCategoryId,
        t.AssignedToUserId,
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
        t.ClosedAt
    FROM dbo.Ticket t
    INNER JOIN dbo.Department d ON t.DepartmentId = d.Id
    INNER JOIN dbo.Category c ON t.CategoryId = c.Id
    LEFT JOIN dbo.SubCategory sc ON t.SubCategoryId = sc.Id
    INNER JOIN dbo.UserMaster uCreator ON t.CreatedByUserId = uCreator.Id
    LEFT JOIN dbo.UserMaster uAssignee ON t.AssignedToUserId = uAssignee.Id
    WHERE t.GId = @GId
      AND t.CompanyId = @CompanyId
      AND t.IsDeleted = 0
      AND (
          @RequestingRole = 'ADMIN'
          OR (@RequestingRole = 'USER' AND t.CreatedByUserId = @RequestingUserId)
          OR (@RequestingRole = 'AGENT' AND (
                t.AssignedToUserId = @RequestingUserId 
                OR (t.AssignedToUserId IS NULL AND (@AgentDepartmentId IS NULL OR t.DepartmentId = @AgentDepartmentId))
             ))
      );
END;

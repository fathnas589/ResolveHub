CREATE OR ALTER PROCEDURE dbo.TicketHistory_Listing
    @TicketGId          UNIQUEIDENTIFIER,
    @CompanyId          INT,
    @RequestingUserId   INT,
    @RequestingRole     NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TicketId INT;
    DECLARE @AgentDepartmentId INT = NULL;

    IF @RequestingRole = 'AGENT'
    BEGIN
        SELECT @AgentDepartmentId = DepartmentId 
        FROM dbo.UserMaster 
        WHERE Id = @RequestingUserId AND CompanyId = @CompanyId;
    END;

    -- Validate Ticket existence and role-level visibility
    SELECT @TicketId = t.Id
    FROM dbo.Ticket t
    WHERE t.GId = @TicketGId
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

    IF @TicketId IS NULL
    BEGIN
        -- Ticket not found or not accessible
        RETURN;
    END;

    SELECT
        th.Id,
        th.GId,
        th.ActionType,
        th.OldValue,
        th.NewValue,
        th.Comments,
        th.Created,
        u.GId AS ActorUserGId,
        u.UserName AS ActorUserName,
        u.FullName AS ActorFullName
    FROM dbo.TicketHistory th
    INNER JOIN dbo.UserMaster u ON th.CreatedByUserId = u.Id
    WHERE th.TicketId = @TicketId
      -- Requesters must not learn that internal (staff-only) comments exist.
      AND NOT (
          @RequestingRole = 'USER'
          AND th.ActionType = 'COMMENT_ADDED'
          AND th.Comments = 'Internal comment added'
      )
    ORDER BY th.Created ASC, th.Id ASC;
END;

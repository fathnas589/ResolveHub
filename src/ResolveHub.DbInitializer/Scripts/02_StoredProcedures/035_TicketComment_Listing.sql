CREATE OR ALTER PROCEDURE dbo.TicketComment_Listing
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
        tc.Id,
        tc.GId,
        tc.CommentText,
        tc.IsInternal,
        tc.Created,
        u.GId AS AuthorUserGId,
        u.UserName AS AuthorUserName,
        u.FullName AS AuthorFullName
    FROM dbo.TicketComment tc
    INNER JOIN dbo.UserMaster u ON tc.CreatedByUserId = u.Id
    WHERE tc.TicketId = @TicketId
      AND tc.IsDeleted = 0
      AND (
          @RequestingRole IN ('ADMIN', 'AGENT')
          OR tc.IsInternal = 0
      )
    ORDER BY tc.Created ASC, tc.Id ASC;
END;

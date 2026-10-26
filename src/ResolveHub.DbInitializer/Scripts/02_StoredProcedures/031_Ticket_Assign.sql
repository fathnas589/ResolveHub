CREATE OR ALTER PROCEDURE dbo.Ticket_Assign
    @TicketGId          UNIQUEIDENTIFIER,
    @CompanyId          INT,
    @AssignedToUserGId  UNIQUEIDENTIFIER,
    @ActorUserId        INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TicketId INT;
    DECLARE @CurrentStatus NVARCHAR(30);
    DECLARE @OldAssignedUserId INT;

    SELECT 
        @TicketId = Id, 
        @CurrentStatus = Status,
        @OldAssignedUserId = AssignedToUserId
    FROM dbo.Ticket
    WHERE GId = @TicketGId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @TicketId IS NULL
    BEGIN
        RAISERROR('Ticket not found.', 16, 1);
        RETURN;
    END;

    -- Validate target user
    DECLARE @TargetUserId INT;
    DECLARE @TargetUserName NVARCHAR(100);
    DECLARE @TargetFullName NVARCHAR(150);

    SELECT 
        @TargetUserId = Id,
        @TargetUserName = UserName,
        @TargetFullName = FullName
    FROM dbo.UserMaster
    WHERE GId = @AssignedToUserGId 
      AND CompanyId = @CompanyId 
      AND RoleCode IN ('AGENT', 'ADMIN')
      AND IsActive = 1 
      AND IsDeleted = 0;

    IF @TargetUserId IS NULL
    BEGIN
        RAISERROR('Target user not found or not eligible for ticket assignment.', 16, 1);
        RETURN;
    END;

    -- Update Ticket: If Status is OPEN, transition to ASSIGNED
    DECLARE @NewStatus NVARCHAR(30) = @CurrentStatus;
    IF @CurrentStatus = 'OPEN'
    BEGIN
        SET @NewStatus = 'ASSIGNED';
    END;

    UPDATE dbo.Ticket
    SET
        AssignedToUserId = @TargetUserId,
        Status = @NewStatus,
        LastUpd = SYSUTCDATETIME(),
        LastUpdBy = @ActorUserId
    WHERE Id = @TicketId;

    -- Audit history
    INSERT INTO dbo.TicketHistory
    (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
    VALUES
    (@TicketId, 'ASSIGNED', NULL, @TargetFullName + ' (' + @TargetUserName + ')', 'Assigned to ' + @TargetUserName, SYSUTCDATETIME(), @ActorUserId);

    IF @CurrentStatus <> @NewStatus
    BEGIN
        INSERT INTO dbo.TicketHistory
        (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
        VALUES
        (@TicketId, 'STATUS_CHANGED', @CurrentStatus, @NewStatus, 'Status changed upon assignment', SYSUTCDATETIME(), @ActorUserId);
    END;

    -- Return updated ticket
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
    WHERE t.Id = @TicketId;
END;

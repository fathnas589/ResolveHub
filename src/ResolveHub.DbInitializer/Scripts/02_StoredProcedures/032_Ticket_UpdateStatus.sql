CREATE OR ALTER PROCEDURE dbo.Ticket_UpdateStatus
    @TicketGId          UNIQUEIDENTIFIER,
    @CompanyId          INT,
    @NewStatus          NVARCHAR(30),
    @Comments           NVARCHAR(1000) = NULL,
    @ActorUserId        INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TicketId INT;
    DECLARE @OldStatus NVARCHAR(30);
    DECLARE @CurrentClosedAt DATETIME2(7);

    SELECT 
        @TicketId = Id, 
        @OldStatus = Status,
        @CurrentClosedAt = ClosedAt
    FROM dbo.Ticket
    WHERE GId = @TicketGId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @TicketId IS NULL
    BEGIN
        RAISERROR('Ticket not found.', 16, 1);
        RETURN;
    END;

    DECLARE @NewClosedAt DATETIME2(7) = @CurrentClosedAt;

    IF @NewStatus = 'CLOSED'
    BEGIN
        SET @NewClosedAt = SYSUTCDATETIME();
    END
    ELSE IF @OldStatus = 'CLOSED' AND @NewStatus <> 'CLOSED'
    BEGIN
        SET @NewClosedAt = NULL;
    END;

    UPDATE dbo.Ticket
    SET
        Status = @NewStatus,
        ClosedAt = @NewClosedAt,
        LastUpd = SYSUTCDATETIME(),
        LastUpdBy = @ActorUserId
    WHERE Id = @TicketId;

    -- Audit history
    INSERT INTO dbo.TicketHistory
    (
        TicketId,
        ActionType,
        OldValue,
        NewValue,
        Comments,
        Created,
        CreatedByUserId
    )
    VALUES
    (
        @TicketId,
        'STATUS_CHANGED',
        @OldStatus,
        @NewStatus,
        @Comments,
        SYSUTCDATETIME(),
        @ActorUserId
    );

    IF @NewStatus = 'CLOSED'
    BEGIN
        INSERT INTO dbo.TicketHistory
        (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
        VALUES
        (@TicketId, 'CLOSED', @OldStatus, 'CLOSED', @Comments, SYSUTCDATETIME(), @ActorUserId);
    END
    ELSE IF @OldStatus = 'CLOSED' AND @NewStatus = 'OPEN'
    BEGIN
        INSERT INTO dbo.TicketHistory
        (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
        VALUES
        (@TicketId, 'REOPENED', 'CLOSED', @NewStatus, @Comments, SYSUTCDATETIME(), @ActorUserId);
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

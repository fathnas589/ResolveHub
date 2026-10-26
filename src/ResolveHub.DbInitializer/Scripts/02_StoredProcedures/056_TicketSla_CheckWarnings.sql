CREATE OR ALTER PROCEDURE dbo.TicketSla_CheckWarnings
    @CurrentUtc DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CurrentUtc IS NULL
        SET @CurrentUtc = SYSUTCDATETIME();

    DECLARE @WarningEvents TABLE
    (
        TicketId            INT NOT NULL,
        TicketGId           UNIQUEIDENTIFIER NOT NULL,
        TicketNumber        NVARCHAR(30) NOT NULL,
        Subject             NVARCHAR(200) NOT NULL,
        CompanyId           INT NOT NULL,
        AssignedToUserId    INT NULL,
        EscalationDueAt     DATETIME2(7) NOT NULL,
        TriggeredAt         DATETIME2(7) NOT NULL
    );

    ;WITH ToTriggerWarning AS
    (
        SELECT ts.TicketId, t.GId AS TicketGId, t.TicketNumber, t.Subject, t.CompanyId, t.AssignedToUserId, ts.EscalationDueAt
        FROM dbo.TicketSla ts
        INNER JOIN dbo.Ticket t ON ts.TicketId = t.Id
        WHERE t.IsDeleted = 0
          AND t.Status NOT IN (N'RESOLVED', N'CLOSED', N'REJECTED')
          AND ts.ResolvedAt IS NULL
          AND ts.ResolutionBreached = 0
          AND ts.EscalationDueAt IS NOT NULL
          AND ts.EscalationTriggeredAt IS NULL
          AND ts.EscalationDueAt <= @CurrentUtc
    )
    UPDATE ts
    SET ts.EscalationTriggeredAt = @CurrentUtc,
        ts.LastUpd = @CurrentUtc
    OUTPUT
        inserted.TicketId,
        u.TicketGId,
        u.TicketNumber,
        u.Subject,
        u.CompanyId,
        u.AssignedToUserId,
        u.EscalationDueAt,
        @CurrentUtc
    INTO @WarningEvents (TicketId, TicketGId, TicketNumber, Subject, CompanyId, AssignedToUserId, EscalationDueAt, TriggeredAt)
    FROM dbo.TicketSla ts
    INNER JOIN ToTriggerWarning u ON ts.TicketId = u.TicketId;

    SELECT
        TicketId,
        TicketGId,
        TicketNumber,
        Subject,
        CompanyId,
        AssignedToUserId,
        EscalationDueAt,
        TriggeredAt
    FROM @WarningEvents;
END;

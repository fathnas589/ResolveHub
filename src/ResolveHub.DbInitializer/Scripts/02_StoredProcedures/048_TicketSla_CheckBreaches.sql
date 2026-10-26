CREATE OR ALTER PROCEDURE dbo.TicketSla_CheckBreaches
    @CurrentUtc DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CurrentUtc IS NULL
        SET @CurrentUtc = SYSUTCDATETIME();

    DECLARE @BreachedEvents TABLE
    (
        TicketId        INT NOT NULL,
        TicketNumber    NVARCHAR(30) NOT NULL,
        CompanyId       INT NOT NULL,
        BreachType      NVARCHAR(30) NOT NULL,
        DueAt           DATETIME2(7) NOT NULL,
        BreachedAt      DATETIME2(7) NOT NULL
    );

    -- 1. First Response Breaches
    ;WITH ToUpdateFirstResponse AS
    (
        SELECT ts.TicketId, t.TicketNumber, t.CompanyId, ts.FirstResponseDueAt
        FROM dbo.TicketSla ts
        INNER JOIN dbo.Ticket t ON ts.TicketId = t.Id
        WHERE t.IsDeleted = 0
          AND t.Status NOT IN (N'RESOLVED', N'CLOSED', N'REJECTED')
          AND ts.FirstRespondedAt IS NULL
          AND ts.FirstResponseBreached = 0
          AND ts.FirstResponseDueAt < @CurrentUtc
    )
    UPDATE ts
    SET ts.FirstResponseBreached = 1,
        ts.LastUpd = @CurrentUtc
    OUTPUT
        inserted.TicketId,
        u.TicketNumber,
        u.CompanyId,
        N'FIRST_RESPONSE',
        u.FirstResponseDueAt,
        @CurrentUtc
    INTO @BreachedEvents (TicketId, TicketNumber, CompanyId, BreachType, DueAt, BreachedAt)
    FROM dbo.TicketSla ts
    INNER JOIN ToUpdateFirstResponse u ON ts.TicketId = u.TicketId;

    -- 2. Resolution Breaches
    ;WITH ToUpdateResolution AS
    (
        SELECT ts.TicketId, t.TicketNumber, t.CompanyId, ts.ResolutionDueAt
        FROM dbo.TicketSla ts
        INNER JOIN dbo.Ticket t ON ts.TicketId = t.Id
        WHERE t.IsDeleted = 0
          AND t.Status NOT IN (N'RESOLVED', N'CLOSED', N'REJECTED')
          AND ts.ResolvedAt IS NULL
          AND ts.ResolutionBreached = 0
          AND ts.ResolutionDueAt < @CurrentUtc
    )
    UPDATE ts
    SET ts.ResolutionBreached = 1,
        ts.LastUpd = @CurrentUtc
    OUTPUT
        inserted.TicketId,
        u.TicketNumber,
        u.CompanyId,
        N'RESOLUTION',
        u.ResolutionDueAt,
        @CurrentUtc
    INTO @BreachedEvents (TicketId, TicketNumber, CompanyId, BreachType, DueAt, BreachedAt)
    FROM dbo.TicketSla ts
    INNER JOIN ToUpdateResolution u ON ts.TicketId = u.TicketId;

    -- 3. Atomically record TicketHistory for all newly detected breaches
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
    SELECT
        b.TicketId,
        CASE WHEN b.BreachType = N'FIRST_RESPONSE' THEN N'FIRST_RESPONSE_SLA_BREACHED' ELSE N'RESOLUTION_SLA_BREACHED' END,
        N'ON_TRACK',
        N'BREACHED',
        N'Automated SLA breach detected: ' + b.BreachType + N' SLA exceeded deadline (' + CONVERT(NVARCHAR(30), b.DueAt, 126) + N').',
        b.BreachedAt,
        t.CreatedByUserId
    FROM @BreachedEvents b
    INNER JOIN dbo.Ticket t ON b.TicketId = t.Id;

    SELECT
        b.TicketId,
        t.GId AS TicketGId,
        b.TicketNumber,
        t.Subject,
        b.CompanyId,
        t.AssignedToUserId,
        b.BreachType,
        b.DueAt,
        b.BreachedAt
    FROM @BreachedEvents b
    INNER JOIN dbo.Ticket t ON b.TicketId = t.Id;
END;

CREATE OR ALTER PROCEDURE dbo.TicketSla_UpdateResolution
    @TicketId   INT,
    @Status     NVARCHAR(30), -- 'RESOLVED' or 'REOPENED'
    @EventTime  DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status = 'RESOLVED'
    BEGIN
        UPDATE dbo.TicketSla
        SET ResolvedAt = @EventTime,
            ResolutionBreached = CASE WHEN @EventTime > ResolutionDueAt THEN 1 ELSE ResolutionBreached END,
            LastUpd = SYSUTCDATETIME()
        WHERE TicketId = @TicketId;
    END
    ELSE IF @Status = 'REOPENED'
    BEGIN
        UPDATE ts
        SET ts.ResolvedAt = NULL,
            ts.ResolutionBreached = 0,
            ts.ResolutionDueAt = DATEADD(MINUTE, ts.ResolutionMinutes, @EventTime),
            ts.EscalationDueAt = CASE 
                WHEN p.EscalationBeforeMinutes > 0 THEN DATEADD(MINUTE, -p.EscalationBeforeMinutes, DATEADD(MINUTE, ts.ResolutionMinutes, @EventTime))
                ELSE NULL 
            END,
            ts.EscalationTriggeredAt = NULL,
            ts.LastUpd = SYSUTCDATETIME()
        FROM dbo.TicketSla ts
        INNER JOIN dbo.SlaPolicy p ON ts.SlaPolicyId = p.Id
        WHERE ts.TicketId = @TicketId;
    END;

    SELECT
        ts.Id,
        ts.GId,
        ts.TicketId,
        ts.SlaPolicyId,
        ts.FirstResponseMinutes,
        ts.ResolutionMinutes,
        ts.FirstResponseDueAt,
        ts.FirstRespondedAt,
        ts.FirstResponseBreached,
        ts.ResolutionDueAt,
        ts.ResolvedAt,
        ts.ResolutionBreached,
        ts.EscalationDueAt,
        ts.EscalationTriggeredAt,
        ts.Created,
        ts.LastUpd
    FROM dbo.TicketSla ts
    WHERE ts.TicketId = @TicketId;
END;

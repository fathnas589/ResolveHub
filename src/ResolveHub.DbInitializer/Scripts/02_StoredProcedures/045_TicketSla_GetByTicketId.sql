CREATE OR ALTER PROCEDURE dbo.TicketSla_GetByTicketId
    @CompanyId  INT,
    @TicketId   INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ts.Id,
        ts.GId,
        ts.TicketId,
        t.TicketNumber,
        ts.SlaPolicyId,
        p.GId AS SlaPolicyGId,
        p.Name AS SlaPolicyName,
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
    INNER JOIN dbo.Ticket t ON ts.TicketId = t.Id
    INNER JOIN dbo.SlaPolicy p ON ts.SlaPolicyId = p.Id
    WHERE ts.TicketId = @TicketId
      AND t.CompanyId = @CompanyId
      AND t.IsDeleted = 0;
END;

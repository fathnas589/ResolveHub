CREATE OR ALTER PROCEDURE dbo.TicketSla_Insert
    @TicketId               INT,
    @SlaPolicyId            INT,
    @FirstResponseMinutes   INT,
    @ResolutionMinutes      INT,
    @FirstResponseDueAt     DATETIME2(7),
    @ResolutionDueAt        DATETIME2(7),
    @EscalationDueAt        DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedDate DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.TicketSla
    (
        GId,
        TicketId,
        SlaPolicyId,
        FirstResponseMinutes,
        ResolutionMinutes,
        FirstResponseDueAt,
        FirstRespondedAt,
        FirstResponseBreached,
        ResolutionDueAt,
        ResolvedAt,
        ResolutionBreached,
        EscalationDueAt,
        EscalationTriggeredAt,
        Created,
        LastUpd
    )
    VALUES
    (
        @NewGId,
        @TicketId,
        @SlaPolicyId,
        @FirstResponseMinutes,
        @ResolutionMinutes,
        @FirstResponseDueAt,
        NULL,
        0,
        @ResolutionDueAt,
        NULL,
        0,
        @EscalationDueAt,
        NULL,
        @CreatedDate,
        NULL
    );

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT
        ts.Id,
        ts.GId,
        ts.TicketId,
        ts.SlaPolicyId,
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
    INNER JOIN dbo.SlaPolicy p ON ts.SlaPolicyId = p.Id
    WHERE ts.Id = @NewId;
END;

CREATE OR ALTER PROCEDURE dbo.TicketSla_UpdateFirstResponse
    @TicketId           INT,
    @FirstRespondedAt   DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TicketSla
    SET FirstRespondedAt = @FirstRespondedAt,
        FirstResponseBreached = CASE WHEN @FirstRespondedAt > FirstResponseDueAt THEN 1 ELSE 0 END,
        LastUpd = SYSUTCDATETIME()
    WHERE TicketId = @TicketId
      AND FirstRespondedAt IS NULL;

    SELECT
        @@ROWCOUNT AS RowsAffected,
        ts.FirstResponseBreached,
        ts.FirstResponseDueAt
    FROM dbo.TicketSla ts
    WHERE ts.TicketId = @TicketId;
END;

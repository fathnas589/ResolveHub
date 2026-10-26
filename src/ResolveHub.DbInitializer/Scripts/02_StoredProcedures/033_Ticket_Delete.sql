CREATE OR ALTER PROCEDURE dbo.Ticket_Delete
    @TicketGId      UNIQUEIDENTIFIER,
    @CompanyId      INT,
    @ActorUserId    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TicketId INT;

    SELECT @TicketId = Id 
    FROM dbo.Ticket 
    WHERE GId = @TicketGId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @TicketId IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS IsSuccess;
        RETURN;
    END;

    UPDATE dbo.Ticket
    SET
        IsDeleted = 1,
        LastUpd = SYSUTCDATETIME(),
        LastUpdBy = @ActorUserId
    WHERE Id = @TicketId;

    DECLARE @RowsAffected INT = @@ROWCOUNT;

    IF @RowsAffected > 0
    BEGIN
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
            'DELETED',
            'ACTIVE',
            'DELETED',
            'Ticket soft-deleted',
            SYSUTCDATETIME(),
            @ActorUserId
        );
    END;

    SELECT CAST(CASE WHEN @RowsAffected > 0 THEN 1 ELSE 0 END AS BIT) AS IsSuccess;
END;

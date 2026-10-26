CREATE OR ALTER PROCEDURE dbo.NotificationDelivery_UpdateStatus
    @DeliveryId         INT,
    @IsSuccess          BIT,
    @LastError          NVARCHAR(1000) = NULL,
    @NextAttemptAt      DATETIME2(7) = NULL,
    @MaxAttempts        INT = 5,
    @CurrentUtc         DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CurrentUtc IS NULL
        SET @CurrentUtc = SYSUTCDATETIME();

    IF @IsSuccess = 1
    BEGIN
        UPDATE dbo.NotificationDelivery
        SET Status = N'DELIVERED',
            DeliveredAt = @CurrentUtc,
            LastError = NULL,
            LastUpd = @CurrentUtc
        WHERE Id = @DeliveryId;
    END
    ELSE
    BEGIN
        DECLARE @CurrentAttempts INT;
        SELECT @CurrentAttempts = AttemptCount
        FROM dbo.NotificationDelivery
        WHERE Id = @DeliveryId;

        IF @CurrentAttempts >= @MaxAttempts
        BEGIN
            UPDATE dbo.NotificationDelivery
            SET Status = N'DEAD_LETTER',
                LastError = LEFT(@LastError, 1000),
                LastUpd = @CurrentUtc
            WHERE Id = @DeliveryId;
        END
        ELSE
        BEGIN
            UPDATE dbo.NotificationDelivery
            SET Status = N'FAILED',
                NextAttemptAt = ISNULL(@NextAttemptAt, DATEADD(MINUTE, 5, @CurrentUtc)),
                LastError = LEFT(@LastError, 1000),
                LastUpd = @CurrentUtc
            WHERE Id = @DeliveryId;
        END;
    END;

    SELECT @@ROWCOUNT AS RowsAffected;
END;

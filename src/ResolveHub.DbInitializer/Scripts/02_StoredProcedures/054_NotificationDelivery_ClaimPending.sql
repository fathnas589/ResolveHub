CREATE OR ALTER PROCEDURE dbo.NotificationDelivery_ClaimPending
    @BatchSize      INT = 25,
    @CurrentUtc     DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CurrentUtc IS NULL
        SET @CurrentUtc = SYSUTCDATETIME();

    IF @BatchSize < 1 SET @BatchSize = 1;
    IF @BatchSize > 100 SET @BatchSize = 100;

    DECLARE @ClaimedIds TABLE (DeliveryId INT PRIMARY KEY);

    -- Deliveries whose recipient has since been deactivated/deleted can never be sent.
    -- Retire them up front; otherwise they'd be claimed (PROCESSING) but never returned to the worker.
    UPDATE nd
    SET nd.Status = N'DEAD_LETTER',
        nd.LastUpd = @CurrentUtc
    FROM dbo.NotificationDelivery nd
    INNER JOIN dbo.NotificationRecipient nr ON nd.NotificationRecipientId = nr.Id
    INNER JOIN dbo.UserMaster u ON nr.RecipientUserId = u.Id
    WHERE nd.Status IN (N'PENDING', N'FAILED')
      AND (u.IsActive = 0 OR u.IsDeleted = 1);

    -- Atomically select and update eligible deliveries with lock hints
    ;WITH EligibleDeliveries AS
    (
        SELECT TOP (@BatchSize) nd.Id
        FROM dbo.NotificationDelivery nd WITH (UPDLOCK, READPAST)
        WHERE (
                nd.Status IN (N'PENDING', N'FAILED')
                AND nd.NextAttemptAt <= @CurrentUtc
              )
              -- Reclaim rows left in PROCESSING by a crash/shutdown mid-batch.
              OR (
                nd.Status = N'PROCESSING'
                AND nd.LastAttemptAt < DATEADD(MINUTE, -15, @CurrentUtc)
              )
        ORDER BY nd.NextAttemptAt ASC, nd.Id ASC
    )
    UPDATE nd
    SET nd.Status = N'PROCESSING',
        nd.LastAttemptAt = @CurrentUtc,
        nd.AttemptCount = nd.AttemptCount + 1,
        nd.LastUpd = @CurrentUtc
    OUTPUT inserted.Id INTO @ClaimedIds (DeliveryId)
    FROM dbo.NotificationDelivery nd
    INNER JOIN EligibleDeliveries ed ON nd.Id = ed.Id;

    -- Return full recipient, notification, and ticket details for claimed deliveries
    SELECT
        nd.Id AS DeliveryId,
        nd.GId AS DeliveryGId,
        nd.Channel,
        nd.AttemptCount,
        u.Id AS RecipientUserId,
        u.Email AS RecipientEmail,
        u.FullName AS RecipientName,
        n.Id AS NotificationId,
        n.GId AS NotificationGId,
        n.CompanyId,
        n.EventType,
        n.Title,
        n.Message,
        n.ActionUrl,
        t.GId AS TicketGId,
        t.TicketNumber
    FROM dbo.NotificationDelivery nd
    INNER JOIN @ClaimedIds c ON nd.Id = c.DeliveryId
    INNER JOIN dbo.NotificationRecipient nr ON nd.NotificationRecipientId = nr.Id
    INNER JOIN dbo.Notification n ON nr.NotificationId = n.Id
    INNER JOIN dbo.UserMaster u ON nr.RecipientUserId = u.Id
    LEFT JOIN dbo.Ticket t ON n.TicketId = t.Id
    WHERE u.IsActive = 1
      AND u.IsDeleted = 0;
END;

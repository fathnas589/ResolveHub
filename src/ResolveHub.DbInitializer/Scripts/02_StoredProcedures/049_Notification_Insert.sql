CREATE OR ALTER PROCEDURE dbo.Notification_Insert
    @CompanyId              INT,
    @EventType              NVARCHAR(50),
    @Title                  NVARCHAR(150),
    @Message                NVARCHAR(500),
    @TicketId               INT = NULL,
    @ActorUserId            INT = NULL,
    @ActionUrl              NVARCHAR(250) = NULL,
    @MetadataJson           NVARCHAR(MAX) = NULL,
    @CorrelationKey         NVARCHAR(150) = NULL,
    @CreatedBy              INT = NULL,
    @RecipientUserIdsCsv    NVARCHAR(MAX),
    @QueueEmail             BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- 1. Idempotency check: if CorrelationKey exists for this company, return existing notification
    IF @CorrelationKey IS NOT NULL AND RTRIM(LTRIM(@CorrelationKey)) <> N''
    BEGIN
        DECLARE @ExistingNotificationId INT = NULL;
        SELECT @ExistingNotificationId = Id
        FROM dbo.Notification
        WHERE CompanyId = @CompanyId
          AND CorrelationKey = @CorrelationKey
          AND IsDeleted = 0;

        IF @ExistingNotificationId IS NOT NULL
        BEGIN
            SELECT
                n.Id,
                n.GId,
                n.CompanyId,
                n.EventType,
                n.Title,
                n.Message,
                n.TicketId,
                t.TicketNumber,
                t.GId AS TicketGId,
                n.ActorUserId,
                u.FullName AS ActorFullName,
                n.ActionUrl,
                n.MetadataJson,
                n.CorrelationKey,
                n.Created,
                n.CreatedBy,
                n.IsDeleted
            FROM dbo.Notification n
            LEFT JOIN dbo.Ticket t ON n.TicketId = t.Id
            LEFT JOIN dbo.UserMaster u ON n.ActorUserId = u.Id
            WHERE n.Id = @ExistingNotificationId;

            RETURN;
        END;
    END;

    -- 2. Validate recipients
    IF @RecipientUserIdsCsv IS NULL OR RTRIM(LTRIM(@RecipientUserIdsCsv)) = N''
    BEGIN
        RAISERROR(N'At least one recipient user ID must be provided.', 16, 1);
        RETURN;
    END;

    DECLARE @ValidRecipients TABLE (UserId INT PRIMARY KEY);
    INSERT INTO @ValidRecipients (UserId)
    SELECT DISTINCT u.Id
    FROM STRING_SPLIT(@RecipientUserIdsCsv, N',') s
    INNER JOIN dbo.UserMaster u ON TRY_CAST(LTRIM(RTRIM(s.value)) AS INT) = u.Id
    WHERE u.CompanyId = @CompanyId
      AND u.IsActive = 1
      AND u.IsDeleted = 0;

    IF NOT EXISTS (SELECT 1 FROM @ValidRecipients)
    BEGIN
        RAISERROR(N'No active, valid recipients found for this company.', 16, 1);
        RETURN;
    END;

    -- 3. Atomic Transaction for Notification, Recipients, and Deliveries
    BEGIN TRANSACTION;

    DECLARE @NewNotificationGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @NewNotificationId INT;
    DECLARE @NowUtc DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.Notification
    (
        GId,
        CompanyId,
        EventType,
        Title,
        Message,
        TicketId,
        ActorUserId,
        ActionUrl,
        MetadataJson,
        CorrelationKey,
        Created,
        CreatedBy,
        IsDeleted
    )
    VALUES
    (
        @NewNotificationGId,
        @CompanyId,
        @EventType,
        @Title,
        @Message,
        @TicketId,
        @ActorUserId,
        @ActionUrl,
        @MetadataJson,
        NULLIF(LTRIM(RTRIM(@CorrelationKey)), N''),
        @NowUtc,
        @CreatedBy,
        0
    );

    SET @NewNotificationId = SCOPE_IDENTITY();

    -- Insert Recipients
    DECLARE @InsertedRecipients TABLE (RecipientId INT NOT NULL, UserId INT NOT NULL);

    INSERT INTO dbo.NotificationRecipient (NotificationId, RecipientUserId, IsRead, Created)
    OUTPUT inserted.Id, inserted.RecipientUserId INTO @InsertedRecipients (RecipientId, UserId)
    SELECT @NewNotificationId, UserId, 0, @NowUtc
    FROM @ValidRecipients;

    -- Insert Deliveries for Email if requested
    IF @QueueEmail = 1
    BEGIN
        INSERT INTO dbo.NotificationDelivery
        (
            NotificationRecipientId,
            Channel,
            Status,
            AttemptCount,
            NextAttemptAt,
            Created
        )
        SELECT
            RecipientId,
            N'EMAIL',
            N'PENDING',
            0,
            @NowUtc,
            @NowUtc
        FROM @InsertedRecipients;
    END;

    COMMIT TRANSACTION;

    -- 4. Return the created notification record
    SELECT
        n.Id,
        n.GId,
        n.CompanyId,
        n.EventType,
        n.Title,
        n.Message,
        n.TicketId,
        t.TicketNumber,
        t.GId AS TicketGId,
        n.ActorUserId,
        u.FullName AS ActorFullName,
        n.ActionUrl,
        n.MetadataJson,
        n.CorrelationKey,
        n.Created,
        n.CreatedBy,
        n.IsDeleted
    FROM dbo.Notification n
    LEFT JOIN dbo.Ticket t ON n.TicketId = t.Id
    LEFT JOIN dbo.UserMaster u ON n.ActorUserId = u.Id
    WHERE n.Id = @NewNotificationId;
END;

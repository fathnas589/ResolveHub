-- ============================================================================
-- Script: 007_CreateNotificationTables.sql
-- Description: Creates dbo.Notification, dbo.NotificationRecipient,
--              and dbo.NotificationDelivery tables, FKs, constraints, and indexes.
-- ============================================================================

-- 1. dbo.Notification Table
IF OBJECT_ID(N'dbo.Notification', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notification
    (
        Id              INT IDENTITY(1,1)   NOT NULL,
        GId             UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_Notification_GId DEFAULT (NEWID()),
        CompanyId       INT                 NOT NULL,
        EventType       NVARCHAR(50)        NOT NULL,
        Title           NVARCHAR(150)       NOT NULL,
        Message         NVARCHAR(500)       NOT NULL,
        TicketId        INT                 NULL,
        ActorUserId     INT                 NULL,
        ActionUrl       NVARCHAR(250)       NULL,
        MetadataJson    NVARCHAR(MAX)       NULL,
        CorrelationKey  NVARCHAR(150)       NULL,
        Created         DATETIME2(7)        NOT NULL CONSTRAINT DF_Notification_Created DEFAULT (SYSUTCDATETIME()),
        CreatedBy       INT                 NULL,
        IsDeleted       BIT                 NOT NULL CONSTRAINT DF_Notification_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Notification_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys for dbo.Notification
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notification_Ticket' AND parent_object_id = OBJECT_ID(N'dbo.Notification'))
BEGIN
    ALTER TABLE dbo.Notification
    ADD CONSTRAINT FK_Notification_Ticket
    FOREIGN KEY (TicketId) REFERENCES dbo.Ticket (Id) ON DELETE SET NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notification_ActorUser' AND parent_object_id = OBJECT_ID(N'dbo.Notification'))
BEGIN
    ALTER TABLE dbo.Notification
    ADD CONSTRAINT FK_Notification_ActorUser
    FOREIGN KEY (ActorUserId) REFERENCES dbo.UserMaster (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notification_CreatedBy' AND parent_object_id = OBJECT_ID(N'dbo.Notification'))
BEGIN
    ALTER TABLE dbo.Notification
    ADD CONSTRAINT FK_Notification_CreatedBy
    FOREIGN KEY (CreatedBy) REFERENCES dbo.UserMaster (Id);
END;

-- Indexes for dbo.Notification
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Notification_GId' AND object_id = OBJECT_ID(N'dbo.Notification'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Notification_GId
    ON dbo.Notification (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Notification_Company_CorrelationKey' AND object_id = OBJECT_ID(N'dbo.Notification'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Notification_Company_CorrelationKey
    ON dbo.Notification (CompanyId, CorrelationKey)
    WHERE CorrelationKey IS NOT NULL AND IsDeleted = 0;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notification_Company_Created' AND object_id = OBJECT_ID(N'dbo.Notification'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Notification_Company_Created
    ON dbo.Notification (CompanyId, Created DESC)
    INCLUDE (EventType, Title, TicketId);
END;

-- 2. dbo.NotificationRecipient Table
IF OBJECT_ID(N'dbo.NotificationRecipient', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationRecipient
    (
        Id              INT IDENTITY(1,1)   NOT NULL,
        GId             UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NotificationRecipient_GId DEFAULT (NEWID()),
        NotificationId  INT                 NOT NULL,
        RecipientUserId INT                 NOT NULL,
        IsRead          BIT                 NOT NULL CONSTRAINT DF_NotificationRecipient_IsRead DEFAULT (0),
        ReadAt          DATETIME2(7)        NULL,
        Created         DATETIME2(7)        NOT NULL CONSTRAINT DF_NotificationRecipient_Created DEFAULT (SYSUTCDATETIME()),
        LastUpd         DATETIME2(7)        NULL,

        CONSTRAINT PK_NotificationRecipient_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys & Constraints for dbo.NotificationRecipient
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NotificationRecipient_Notification' AND parent_object_id = OBJECT_ID(N'dbo.NotificationRecipient'))
BEGIN
    ALTER TABLE dbo.NotificationRecipient
    ADD CONSTRAINT FK_NotificationRecipient_Notification
    FOREIGN KEY (NotificationId) REFERENCES dbo.Notification (Id) ON DELETE CASCADE;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NotificationRecipient_RecipientUser' AND parent_object_id = OBJECT_ID(N'dbo.NotificationRecipient'))
BEGIN
    ALTER TABLE dbo.NotificationRecipient
    ADD CONSTRAINT FK_NotificationRecipient_RecipientUser
    FOREIGN KEY (RecipientUserId) REFERENCES dbo.UserMaster (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_NotificationRecipient_Notification_User' AND object_id = OBJECT_ID(N'dbo.NotificationRecipient'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_NotificationRecipient_Notification_User
    ON dbo.NotificationRecipient (NotificationId, RecipientUserId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_NotificationRecipient_GId' AND object_id = OBJECT_ID(N'dbo.NotificationRecipient'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_NotificationRecipient_GId
    ON dbo.NotificationRecipient (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotificationRecipient_User_Inbox' AND object_id = OBJECT_ID(N'dbo.NotificationRecipient'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_NotificationRecipient_User_Inbox
    ON dbo.NotificationRecipient (RecipientUserId, IsRead, Created DESC)
    INCLUDE (NotificationId, ReadAt);
END;

-- 3. dbo.NotificationDelivery Table
IF OBJECT_ID(N'dbo.NotificationDelivery', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationDelivery
    (
        Id                          INT IDENTITY(1,1)   NOT NULL,
        GId                         UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NotificationDelivery_GId DEFAULT (NEWID()),
        NotificationRecipientId     INT                 NOT NULL,
        Channel                     NVARCHAR(20)        NOT NULL, -- 'EMAIL'
        Status                      NVARCHAR(20)        NOT NULL CONSTRAINT DF_NotificationDelivery_Status DEFAULT (N'PENDING'), -- 'PENDING', 'PROCESSING', 'DELIVERED', 'FAILED', 'DEAD_LETTER'
        AttemptCount                INT                 NOT NULL CONSTRAINT DF_NotificationDelivery_AttemptCount DEFAULT (0),
        NextAttemptAt               DATETIME2(7)        NOT NULL CONSTRAINT DF_NotificationDelivery_NextAttemptAt DEFAULT (SYSUTCDATETIME()),
        LastAttemptAt               DATETIME2(7)        NULL,
        DeliveredAt                 DATETIME2(7)        NULL,
        LastError                   NVARCHAR(1000)      NULL,
        Created                     DATETIME2(7)        NOT NULL CONSTRAINT DF_NotificationDelivery_Created DEFAULT (SYSUTCDATETIME()),
        LastUpd                     DATETIME2(7)        NULL,

        CONSTRAINT PK_NotificationDelivery_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys & Constraints for dbo.NotificationDelivery
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NotificationDelivery_Recipient' AND parent_object_id = OBJECT_ID(N'dbo.NotificationDelivery'))
BEGIN
    ALTER TABLE dbo.NotificationDelivery
    ADD CONSTRAINT FK_NotificationDelivery_Recipient
    FOREIGN KEY (NotificationRecipientId) REFERENCES dbo.NotificationRecipient (Id) ON DELETE CASCADE;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_NotificationDelivery_GId' AND object_id = OBJECT_ID(N'dbo.NotificationDelivery'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_NotificationDelivery_GId
    ON dbo.NotificationDelivery (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotificationDelivery_WorkerQueue' AND object_id = OBJECT_ID(N'dbo.NotificationDelivery'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_NotificationDelivery_WorkerQueue
    ON dbo.NotificationDelivery (Status, NextAttemptAt, Created)
    INCLUDE (NotificationRecipientId, AttemptCount);
END;

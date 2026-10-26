-- ============================================================================
-- Script: 005_CreateTicketTables.sql
-- Description: Creates Seq_TicketNumber sequence, dbo.Ticket, dbo.TicketHistory,
--              dbo.TicketComment tables, foreign keys, and supporting indexes.
-- ============================================================================

-- 1. Sequence for human-readable TicketNumber (RH-YYYY-000001)
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'Seq_TicketNumber' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE SEQUENCE dbo.Seq_TicketNumber
    AS BIGINT
    START WITH 1
    INCREMENT BY 1;
END;

-- 2. dbo.Ticket Table
IF OBJECT_ID(N'dbo.Ticket', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ticket
    (
        Id                  INT IDENTITY(1,1)   NOT NULL,
        GId                 UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_Ticket_GId DEFAULT (NEWID()),
        TicketNumber        NVARCHAR(30)        NOT NULL,
        CompanyId           INT                 NOT NULL,
        CreatedByUserId     INT                 NOT NULL,
        DepartmentId        INT                 NOT NULL,
        CategoryId          INT                 NOT NULL,
        SubCategoryId       INT                 NULL,
        TicketType          NVARCHAR(30)        NOT NULL, -- 'COMPLAINT', 'SERVICE_REQUEST'
        Subject             NVARCHAR(200)       NOT NULL,
        Description         NVARCHAR(MAX)       NOT NULL,
        Priority            NVARCHAR(20)        NOT NULL, -- 'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'
        Status              NVARCHAR(30)        NOT NULL, -- 'OPEN', 'ASSIGNED', 'IN_PROGRESS', 'PENDING_USER', 'RESOLVED', 'CLOSED', 'REJECTED'
        AssignedToUserId    INT                 NULL,
        Created             DATETIME2(7)        NOT NULL CONSTRAINT DF_Ticket_Created DEFAULT (SYSUTCDATETIME()),
        LastUpd             DATETIME2(7)        NULL,
        LastUpdBy           INT                 NULL,
        ClosedAt            DATETIME2(7)        NULL,
        IsDeleted           BIT                 NOT NULL CONSTRAINT DF_Ticket_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Ticket_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys for dbo.Ticket
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ticket_CreatedByUser' AND parent_object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    ALTER TABLE dbo.Ticket
    ADD CONSTRAINT FK_Ticket_CreatedByUser
    FOREIGN KEY (CreatedByUserId) REFERENCES dbo.UserMaster (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ticket_AssignedToUser' AND parent_object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    ALTER TABLE dbo.Ticket
    ADD CONSTRAINT FK_Ticket_AssignedToUser
    FOREIGN KEY (AssignedToUserId) REFERENCES dbo.UserMaster (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ticket_Department' AND parent_object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    ALTER TABLE dbo.Ticket
    ADD CONSTRAINT FK_Ticket_Department
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Department (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ticket_Category' AND parent_object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    ALTER TABLE dbo.Ticket
    ADD CONSTRAINT FK_Ticket_Category
    FOREIGN KEY (CategoryId) REFERENCES dbo.Category (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ticket_SubCategory' AND parent_object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    ALTER TABLE dbo.Ticket
    ADD CONSTRAINT FK_Ticket_SubCategory
    FOREIGN KEY (SubCategoryId) REFERENCES dbo.SubCategory (Id);
END;

-- Unique Indexes on dbo.Ticket
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Ticket_GId' AND object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Ticket_GId
    ON dbo.Ticket (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Ticket_TicketNumber' AND object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Ticket_TicketNumber
    ON dbo.Ticket (TicketNumber);
END;

-- Supporting Non-Clustered Indexes on dbo.Ticket
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ticket_Company_CreatedBy_IsDeleted' AND object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Ticket_Company_CreatedBy_IsDeleted
    ON dbo.Ticket (CompanyId, CreatedByUserId, IsDeleted)
    INCLUDE (TicketNumber, Subject, Priority, Status, Created);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ticket_Company_Assigned_Status_IsDeleted' AND object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Ticket_Company_Assigned_Status_IsDeleted
    ON dbo.Ticket (CompanyId, AssignedToUserId, Status, IsDeleted)
    INCLUDE (TicketNumber, Subject, Priority, Created);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ticket_Company_Dept_Status_IsDeleted' AND object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Ticket_Company_Dept_Status_IsDeleted
    ON dbo.Ticket (CompanyId, DepartmentId, Status, IsDeleted)
    INCLUDE (TicketNumber, Subject, Priority, Created);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ticket_Company_Created_IsDeleted' AND object_id = OBJECT_ID(N'dbo.Ticket'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Ticket_Company_Created_IsDeleted
    ON dbo.Ticket (CompanyId, Created, IsDeleted)
    INCLUDE (TicketNumber, Subject, Priority, Status, AssignedToUserId);
END;

-- 3. dbo.TicketHistory Table
IF OBJECT_ID(N'dbo.TicketHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketHistory
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL,
        GId                 UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_TicketHistory_GId DEFAULT (NEWID()),
        TicketId            INT                 NOT NULL,
        ActionType          NVARCHAR(50)        NOT NULL, -- 'CREATED', 'ASSIGNED', 'STATUS_CHANGED', 'PRIORITY_CHANGED', 'COMMENT_ADDED', 'UPDATED', 'CLOSED', 'REOPENED', 'DELETED'
        OldValue            NVARCHAR(500)       NULL,
        NewValue            NVARCHAR(500)       NULL,
        Comments            NVARCHAR(1000)      NULL,
        Created             DATETIME2(7)        NOT NULL CONSTRAINT DF_TicketHistory_Created DEFAULT (SYSUTCDATETIME()),
        CreatedByUserId     INT                 NOT NULL,

        CONSTRAINT PK_TicketHistory_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys for dbo.TicketHistory
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TicketHistory_Ticket' AND parent_object_id = OBJECT_ID(N'dbo.TicketHistory'))
BEGIN
    ALTER TABLE dbo.TicketHistory
    ADD CONSTRAINT FK_TicketHistory_Ticket
    FOREIGN KEY (TicketId) REFERENCES dbo.Ticket (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TicketHistory_CreatedByUser' AND parent_object_id = OBJECT_ID(N'dbo.TicketHistory'))
BEGIN
    ALTER TABLE dbo.TicketHistory
    ADD CONSTRAINT FK_TicketHistory_CreatedByUser
    FOREIGN KEY (CreatedByUserId) REFERENCES dbo.UserMaster (Id);
END;

-- Indexes for dbo.TicketHistory
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_TicketHistory_GId' AND object_id = OBJECT_ID(N'dbo.TicketHistory'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TicketHistory_GId
    ON dbo.TicketHistory (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TicketHistory_TicketId_Created' AND object_id = OBJECT_ID(N'dbo.TicketHistory'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TicketHistory_TicketId_Created
    ON dbo.TicketHistory (TicketId, Created);
END;

-- 4. dbo.TicketComment Table
IF OBJECT_ID(N'dbo.TicketComment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketComment
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL,
        GId                 UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_TicketComment_GId DEFAULT (NEWID()),
        TicketId            INT                 NOT NULL,
        CommentText         NVARCHAR(2000)      NOT NULL,
        Created             DATETIME2(7)        NOT NULL CONSTRAINT DF_TicketComment_Created DEFAULT (SYSUTCDATETIME()),
        CreatedByUserId     INT                 NOT NULL,
        IsInternal          BIT                 NOT NULL CONSTRAINT DF_TicketComment_IsInternal DEFAULT (0),
        IsDeleted           BIT                 NOT NULL CONSTRAINT DF_TicketComment_IsDeleted DEFAULT (0),

        CONSTRAINT PK_TicketComment_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys for dbo.TicketComment
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TicketComment_Ticket' AND parent_object_id = OBJECT_ID(N'dbo.TicketComment'))
BEGIN
    ALTER TABLE dbo.TicketComment
    ADD CONSTRAINT FK_TicketComment_Ticket
    FOREIGN KEY (TicketId) REFERENCES dbo.Ticket (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TicketComment_CreatedByUser' AND parent_object_id = OBJECT_ID(N'dbo.TicketComment'))
BEGIN
    ALTER TABLE dbo.TicketComment
    ADD CONSTRAINT FK_TicketComment_CreatedByUser
    FOREIGN KEY (CreatedByUserId) REFERENCES dbo.UserMaster (Id);
END;

-- Indexes for dbo.TicketComment
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_TicketComment_GId' AND object_id = OBJECT_ID(N'dbo.TicketComment'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TicketComment_GId
    ON dbo.TicketComment (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TicketComment_TicketId_IsDeleted_IsInternal' AND object_id = OBJECT_ID(N'dbo.TicketComment'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TicketComment_TicketId_IsDeleted_IsInternal
    ON dbo.TicketComment (TicketId, IsDeleted, IsInternal, Created);
END;

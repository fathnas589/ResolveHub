-- ============================================================================
-- Script: 006_CreateSlaTables.sql
-- Description: Creates dbo.SlaPolicy and dbo.TicketSla tables, foreign keys,
--              check constraints, and supporting indexes for SLA tracking.
-- ============================================================================

-- 1. dbo.SlaPolicy Table
IF OBJECT_ID(N'dbo.SlaPolicy', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SlaPolicy
    (
        Id                      INT IDENTITY(1,1)   NOT NULL,
        GId                     UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_SlaPolicy_GId DEFAULT (NEWID()),
        CompanyId               INT                 NOT NULL,
        Name                    NVARCHAR(100)       NOT NULL,
        Description             NVARCHAR(500)       NULL,
        DepartmentId            INT                 NULL,
        CategoryId              INT                 NULL,
        SubCategoryId           INT                 NULL,
        Priority                NVARCHAR(20)        NULL, -- 'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'
        TicketType              NVARCHAR(30)        NULL, -- 'COMPLAINT', 'SERVICE_REQUEST'
        FirstResponseMinutes    INT                 NOT NULL,
        ResolutionMinutes       INT                 NOT NULL,
        EscalationBeforeMinutes INT                 NOT NULL CONSTRAINT DF_SlaPolicy_EscalationBeforeMinutes DEFAULT (0),
        IsDefault               BIT                 NOT NULL CONSTRAINT DF_SlaPolicy_IsDefault DEFAULT (0),
        IsActive                BIT                 NOT NULL CONSTRAINT DF_SlaPolicy_IsActive DEFAULT (1),
        IsDeleted               BIT                 NOT NULL CONSTRAINT DF_SlaPolicy_IsDeleted DEFAULT (0),
        Created                 DATETIME2(7)        NOT NULL CONSTRAINT DF_SlaPolicy_Created DEFAULT (SYSUTCDATETIME()),
        CreatedBy               INT                 NOT NULL,
        LastUpd                 DATETIME2(7)        NULL,
        LastUpdBy               INT                 NULL,

        CONSTRAINT PK_SlaPolicy_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys for dbo.SlaPolicy
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SlaPolicy_CreatedBy' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT FK_SlaPolicy_CreatedBy
    FOREIGN KEY (CreatedBy) REFERENCES dbo.UserMaster (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SlaPolicy_LastUpdBy' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT FK_SlaPolicy_LastUpdBy
    FOREIGN KEY (LastUpdBy) REFERENCES dbo.UserMaster (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SlaPolicy_Department' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT FK_SlaPolicy_Department
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Department (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SlaPolicy_Category' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT FK_SlaPolicy_Category
    FOREIGN KEY (CategoryId) REFERENCES dbo.Category (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SlaPolicy_SubCategory' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT FK_SlaPolicy_SubCategory
    FOREIGN KEY (SubCategoryId) REFERENCES dbo.SubCategory (Id);
END;

-- Check Constraints for dbo.SlaPolicy
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SlaPolicy_FirstResponseMinutes' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT CK_SlaPolicy_FirstResponseMinutes
    CHECK (FirstResponseMinutes > 0);
END;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SlaPolicy_ResolutionMinutes' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT CK_SlaPolicy_ResolutionMinutes
    CHECK (ResolutionMinutes > 0);
END;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SlaPolicy_EscalationBeforeMinutes' AND parent_object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    ALTER TABLE dbo.SlaPolicy
    ADD CONSTRAINT CK_SlaPolicy_EscalationBeforeMinutes
    CHECK (EscalationBeforeMinutes >= 0 AND EscalationBeforeMinutes < ResolutionMinutes);
END;

-- Unique and Non-Clustered Indexes on dbo.SlaPolicy
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_SlaPolicy_GId' AND object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_SlaPolicy_GId
    ON dbo.SlaPolicy (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_SlaPolicy_Company_Name' AND object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_SlaPolicy_Company_Name
    ON dbo.SlaPolicy (CompanyId, Name)
    WHERE IsDeleted = 0;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlaPolicy_Matching' AND object_id = OBJECT_ID(N'dbo.SlaPolicy'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SlaPolicy_Matching
    ON dbo.SlaPolicy (CompanyId, IsActive, IsDeleted, DepartmentId, CategoryId, SubCategoryId, Priority, TicketType)
    INCLUDE (FirstResponseMinutes, ResolutionMinutes, EscalationBeforeMinutes, IsDefault, Created, LastUpd);
END;

-- 2. dbo.TicketSla Table
IF OBJECT_ID(N'dbo.TicketSla', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketSla
    (
        Id                      INT IDENTITY(1,1)   NOT NULL,
        GId                     UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_TicketSla_GId DEFAULT (NEWID()),
        TicketId                INT                 NOT NULL,
        SlaPolicyId             INT                 NOT NULL,
        FirstResponseMinutes    INT                 NOT NULL,
        ResolutionMinutes       INT                 NOT NULL,
        FirstResponseDueAt      DATETIME2(7)        NOT NULL,
        FirstRespondedAt        DATETIME2(7)        NULL,
        FirstResponseBreached   BIT                 NOT NULL CONSTRAINT DF_TicketSla_FirstResponseBreached DEFAULT (0),
        ResolutionDueAt         DATETIME2(7)        NOT NULL,
        ResolvedAt              DATETIME2(7)        NULL,
        ResolutionBreached      BIT                 NOT NULL CONSTRAINT DF_TicketSla_ResolutionBreached DEFAULT (0),
        EscalationDueAt         DATETIME2(7)        NULL,
        EscalationTriggeredAt   DATETIME2(7)        NULL,
        Created                 DATETIME2(7)        NOT NULL CONSTRAINT DF_TicketSla_Created DEFAULT (SYSUTCDATETIME()),
        LastUpd                 DATETIME2(7)        NULL,

        CONSTRAINT PK_TicketSla_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign Keys for dbo.TicketSla
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TicketSla_Ticket' AND parent_object_id = OBJECT_ID(N'dbo.TicketSla'))
BEGIN
    ALTER TABLE dbo.TicketSla
    ADD CONSTRAINT FK_TicketSla_Ticket
    FOREIGN KEY (TicketId) REFERENCES dbo.Ticket (Id);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TicketSla_SlaPolicy' AND parent_object_id = OBJECT_ID(N'dbo.TicketSla'))
BEGIN
    ALTER TABLE dbo.TicketSla
    ADD CONSTRAINT FK_TicketSla_SlaPolicy
    FOREIGN KEY (SlaPolicyId) REFERENCES dbo.SlaPolicy (Id);
END;

-- Unique and Non-Clustered Indexes on dbo.TicketSla
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_TicketSla_GId' AND object_id = OBJECT_ID(N'dbo.TicketSla'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TicketSla_GId
    ON dbo.TicketSla (GId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_TicketSla_TicketId' AND object_id = OBJECT_ID(N'dbo.TicketSla'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TicketSla_TicketId
    ON dbo.TicketSla (TicketId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TicketSla_BreachMonitor' AND object_id = OBJECT_ID(N'dbo.TicketSla'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TicketSla_BreachMonitor
    ON dbo.TicketSla (ResolutionBreached, FirstResponseBreached)
    INCLUDE (TicketId, FirstResponseDueAt, ResolutionDueAt, FirstRespondedAt, ResolvedAt);
END;

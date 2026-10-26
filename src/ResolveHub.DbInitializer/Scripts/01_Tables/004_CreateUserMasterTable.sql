IF OBJECT_ID(N'dbo.UserMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserMaster
    (
        Id              INT IDENTITY(1,1)   NOT NULL,
        GId             UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_UserMaster_GId DEFAULT (NEWID()),
        CompanyId       INT                 NOT NULL,
        DepartmentId    INT                 NULL,
        UserName        NVARCHAR(100)       NOT NULL,
        FullName        NVARCHAR(150)       NOT NULL,
        Email           NVARCHAR(150)       NOT NULL,
        Phone           NVARCHAR(30)        NULL,
        PasswordHash    NVARCHAR(500)       NOT NULL,
        RoleCode        NVARCHAR(50)        NOT NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_UserMaster_IsActive DEFAULT (1),
        IsDeleted       BIT                 NOT NULL CONSTRAINT DF_UserMaster_IsDeleted DEFAULT (0),
        Created         DATETIME2(7)        NOT NULL CONSTRAINT DF_UserMaster_Created DEFAULT (SYSUTCDATETIME()),
        CreatedBy       INT                 NULL,
        LastUpd         DATETIME2(7)        NULL,
        LastUpdBy       INT                 NULL,

        CONSTRAINT PK_UserMaster_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign key constraint: UserMaster.DepartmentId -> Department.Id
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserMaster_Department' AND parent_object_id = OBJECT_ID(N'dbo.UserMaster'))
BEGIN
    ALTER TABLE dbo.UserMaster
    ADD CONSTRAINT FK_UserMaster_Department
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Department (Id);
END;

-- Unique index on GId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_UserMaster_GId' AND object_id = OBJECT_ID(N'dbo.UserMaster'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_UserMaster_GId
    ON dbo.UserMaster (GId);
END;

-- Filtered unique index on (CompanyId, UserName) where IsDeleted = 0
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_UserMaster_CompanyId_UserName' AND object_id = OBJECT_ID(N'dbo.UserMaster'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_UserMaster_CompanyId_UserName
    ON dbo.UserMaster (CompanyId, UserName)
    WHERE IsDeleted = 0;
END;

-- Filtered unique index on (CompanyId, Email) where IsDeleted = 0
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_UserMaster_CompanyId_Email' AND object_id = OBJECT_ID(N'dbo.UserMaster'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_UserMaster_CompanyId_Email
    ON dbo.UserMaster (CompanyId, Email)
    WHERE IsDeleted = 0;
END;

-- Non-clustered index for listing / searches / active filter
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserMaster_CompanyId_DepartmentId_IsDeleted_IsActive' AND object_id = OBJECT_ID(N'dbo.UserMaster'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserMaster_CompanyId_DepartmentId_IsDeleted_IsActive
    ON dbo.UserMaster (CompanyId, DepartmentId, IsDeleted, IsActive)
    INCLUDE (UserName, FullName, Email, Phone, RoleCode, GId, Created);
END;

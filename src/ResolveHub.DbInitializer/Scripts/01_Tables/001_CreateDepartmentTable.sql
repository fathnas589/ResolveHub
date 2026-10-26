IF OBJECT_ID(N'dbo.Department', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Department
    (
        Id              INT IDENTITY(1,1)   NOT NULL,
        GId             UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_Department_GId DEFAULT (NEWID()),
        DepartmentName  NVARCHAR(100)       NOT NULL,
        Description     NVARCHAR(500)       NULL,
        CompanyId       INT                 NOT NULL,
        Created         DATETIME2(7)        NOT NULL CONSTRAINT DF_Department_Created DEFAULT (SYSUTCDATETIME()),
        CreatedBy       INT                 NOT NULL,
        LastUpd         DATETIME2(7)        NULL,
        LastUpdBy       INT                 NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Department_IsActive DEFAULT (1),
        IsDeleted       BIT                 NOT NULL CONSTRAINT DF_Department_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Department_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Unique index on GId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Department_GId' AND object_id = OBJECT_ID(N'dbo.Department'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Department_GId
    ON dbo.Department (GId);
END;

-- Filtered unique index on (CompanyId, DepartmentName) where IsDeleted = 0
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Department_CompanyId_DepartmentName' AND object_id = OBJECT_ID(N'dbo.Department'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Department_CompanyId_DepartmentName
    ON dbo.Department (CompanyId, DepartmentName)
    WHERE IsDeleted = 0;
END;

-- Non-clustered index for listing / searches
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Department_CompanyId_IsDeleted_IsActive' AND object_id = OBJECT_ID(N'dbo.Department'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Department_CompanyId_IsDeleted_IsActive
    ON dbo.Department (CompanyId, IsDeleted, IsActive)
    INCLUDE (DepartmentName, Description, GId, Created);
END;

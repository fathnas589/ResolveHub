IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Category
    (
        Id              INT IDENTITY(1,1)   NOT NULL,
        GId             UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_Category_GId DEFAULT (NEWID()),
        CategoryName    NVARCHAR(100)       NOT NULL,
        Description     NVARCHAR(500)       NULL,
        DepartmentId    INT                 NOT NULL,
        CompanyId       INT                 NOT NULL,
        Created         DATETIME2(7)        NOT NULL CONSTRAINT DF_Category_Created DEFAULT (SYSUTCDATETIME()),
        CreatedBy       INT                 NOT NULL,
        LastUpd         DATETIME2(7)        NULL,
        LastUpdBy       INT                 NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Category_IsActive DEFAULT (1),
        IsDeleted       BIT                 NOT NULL CONSTRAINT DF_Category_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Category_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign key constraint: Category.DepartmentId -> Department.Id
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Category_Department' AND parent_object_id = OBJECT_ID(N'dbo.Category'))
BEGIN
    ALTER TABLE dbo.Category
    ADD CONSTRAINT FK_Category_Department
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Department (Id);
END;

-- Unique index on GId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Category_GId' AND object_id = OBJECT_ID(N'dbo.Category'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Category_GId
    ON dbo.Category (GId);
END;

-- Filtered unique index on (CompanyId, DepartmentId, CategoryName) where IsDeleted = 0
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Category_CompanyId_DepartmentId_CategoryName' AND object_id = OBJECT_ID(N'dbo.Category'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Category_CompanyId_DepartmentId_CategoryName
    ON dbo.Category (CompanyId, DepartmentId, CategoryName)
    WHERE IsDeleted = 0;
END;

-- Non-clustered index for listing / searches
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Category_CompanyId_DepartmentId_IsDeleted_IsActive' AND object_id = OBJECT_ID(N'dbo.Category'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Category_CompanyId_DepartmentId_IsDeleted_IsActive
    ON dbo.Category (CompanyId, DepartmentId, IsDeleted, IsActive)
    INCLUDE (CategoryName, Description, GId, Created);
END;

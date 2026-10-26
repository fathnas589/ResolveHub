IF OBJECT_ID(N'dbo.SubCategory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SubCategory
    (
        Id                  INT IDENTITY(1,1)   NOT NULL,
        GId                 UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_SubCategory_GId DEFAULT (NEWID()),
        CategoryId          INT                 NOT NULL,
        SubCategoryName     NVARCHAR(100)       NOT NULL,
        Description         NVARCHAR(250)       NULL,
        CompanyId           INT                 NOT NULL,
        Created             DATETIME2(7)        NOT NULL CONSTRAINT DF_SubCategory_Created DEFAULT (SYSUTCDATETIME()),
        CreatedBy           INT                 NOT NULL,
        LastUpd             DATETIME2(7)        NULL,
        LastUpdBy           INT                 NULL,
        IsActive            BIT                 NOT NULL CONSTRAINT DF_SubCategory_IsActive DEFAULT (1),
        IsDeleted           BIT                 NOT NULL CONSTRAINT DF_SubCategory_IsDeleted DEFAULT (0),

        CONSTRAINT PK_SubCategory_Id PRIMARY KEY CLUSTERED (Id)
    );
END;

-- Foreign key constraint: SubCategory.CategoryId -> Category.Id
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubCategory_Category' AND parent_object_id = OBJECT_ID(N'dbo.SubCategory'))
BEGIN
    ALTER TABLE dbo.SubCategory
    ADD CONSTRAINT FK_SubCategory_Category
    FOREIGN KEY (CategoryId) REFERENCES dbo.Category (Id);
END;

-- Unique index on GId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_SubCategory_GId' AND object_id = OBJECT_ID(N'dbo.SubCategory'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_SubCategory_GId
    ON dbo.SubCategory (GId);
END;

-- Filtered unique index on (CompanyId, CategoryId, SubCategoryName) where IsDeleted = 0
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_SubCategory_CompanyId_CategoryId_SubCategoryName' AND object_id = OBJECT_ID(N'dbo.SubCategory'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_SubCategory_CompanyId_CategoryId_SubCategoryName
    ON dbo.SubCategory (CompanyId, CategoryId, SubCategoryName)
    WHERE IsDeleted = 0;
END;

-- Non-clustered index for listing / searches
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubCategory_CompanyId_CategoryId_IsDeleted_IsActive' AND object_id = OBJECT_ID(N'dbo.SubCategory'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubCategory_CompanyId_CategoryId_IsDeleted_IsActive
    ON dbo.SubCategory (CompanyId, CategoryId, IsDeleted, IsActive)
    INCLUDE (SubCategoryName, Description, GId, Created);
END;

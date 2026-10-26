CREATE OR ALTER PROCEDURE dbo.SubCategory_Insert
    @CompanyId          INT,
    @CategoryGId        UNIQUEIDENTIFIER,
    @SubCategoryName    NVARCHAR(100),
    @Description        NVARCHAR(250) = NULL,
    @IsActive           BIT = 1,
    @CreatedBy          INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate parent Category existence for this company
    DECLARE @CategoryId INT;
    SELECT @CategoryId = c.Id
    FROM dbo.Category c
    WHERE c.GId = @CategoryGId
      AND c.CompanyId = @CompanyId
      AND c.IsDeleted = 0;

    IF @CategoryId IS NULL
    BEGIN
        RAISERROR(N'The specified category does not exist for this company.', 16, 1);
        RETURN;
    END

    -- Duplicate check validation for non-deleted subcategories in the same category
    IF EXISTS (
        SELECT 1
        FROM dbo.SubCategory s
        WHERE s.CompanyId = @CompanyId
          AND s.CategoryId = @CategoryId
          AND s.SubCategoryName = @SubCategoryName
          AND s.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'A subcategory with the name ''%s'' already exists for this category.', 16, 1, @SubCategoryName);
        RETURN;
    END

    DECLARE @NewGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedDate DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.SubCategory
    (
        GId,
        CategoryId,
        SubCategoryName,
        Description,
        CompanyId,
        Created,
        CreatedBy,
        LastUpd,
        LastUpdBy,
        IsActive,
        IsDeleted
    )
    VALUES
    (
        @NewGId,
        @CategoryId,
        @SubCategoryName,
        @Description,
        @CompanyId,
        @CreatedDate,
        @CreatedBy,
        NULL,
        NULL,
        @IsActive,
        0
    );

    DECLARE @NewId INT = SCOPE_IDENTITY();

    -- Return the created subcategory with explicit column list and parent metadata
    SELECT
        s.Id,
        s.GId,
        s.SubCategoryName,
        s.Description,
        s.CategoryId,
        c.GId AS CategoryGId,
        c.CategoryName,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        s.CompanyId,
        s.Created,
        s.CreatedBy,
        s.LastUpd,
        s.LastUpdBy,
        s.IsActive,
        s.IsDeleted
    FROM dbo.SubCategory s
    INNER JOIN dbo.Category c ON s.CategoryId = c.Id
    INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
    WHERE s.Id = @NewId;
END;

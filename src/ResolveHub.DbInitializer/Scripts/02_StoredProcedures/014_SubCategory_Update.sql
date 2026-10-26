CREATE OR ALTER PROCEDURE dbo.SubCategory_Update
    @CompanyId          INT,
    @GId                UNIQUEIDENTIFIER,
    @CategoryGId        UNIQUEIDENTIFIER,
    @SubCategoryName    NVARCHAR(100),
    @Description        NVARCHAR(250) = NULL,
    @IsActive           BIT,
    @LastUpdBy          INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if the subcategory exists
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.SubCategory
        WHERE GId = @GId
          AND CompanyId = @CompanyId
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'SubCategory not found.', 16, 1);
        RETURN;
    END

    -- Validate target Category existence for this company
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

    -- Check for duplicate subcategory name under the same category (excluding self)
    IF EXISTS (
        SELECT 1
        FROM dbo.SubCategory s
        WHERE s.CompanyId = @CompanyId
          AND s.CategoryId = @CategoryId
          AND s.SubCategoryName = @SubCategoryName
          AND s.GId <> @GId
          AND s.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Another subcategory with the name ''%s'' already exists for this category.', 16, 1, @SubCategoryName);
        RETURN;
    END

    DECLARE @UpdateDate DATETIME2(7) = SYSUTCDATETIME();

    UPDATE dbo.SubCategory
    SET
        SubCategoryName = @SubCategoryName,
        Description     = @Description,
        CategoryId      = @CategoryId,
        IsActive        = @IsActive,
        LastUpd         = @UpdateDate,
        LastUpdBy       = @LastUpdBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    -- Return the updated record with explicit columns and parent metadata
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
    WHERE s.GId = @GId
      AND s.CompanyId = @CompanyId
      AND s.IsDeleted = 0;
END;

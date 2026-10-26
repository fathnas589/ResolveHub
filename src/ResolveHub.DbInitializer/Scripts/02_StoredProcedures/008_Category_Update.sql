CREATE OR ALTER PROCEDURE dbo.Category_Update
    @CompanyId      INT,
    @GId            UNIQUEIDENTIFIER,
    @DepartmentGId  UNIQUEIDENTIFIER,
    @CategoryName   NVARCHAR(100),
    @Description    NVARCHAR(500) = NULL,
    @IsActive       BIT,
    @LastUpdBy      INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if the category exists
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Category
        WHERE GId = @GId
          AND CompanyId = @CompanyId
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Category not found.', 16, 1);
        RETURN;
    END

    -- Validate target Department existence for this company
    DECLARE @DepartmentId INT;
    SELECT @DepartmentId = d.Id
    FROM dbo.Department d
    WHERE d.GId = @DepartmentGId
      AND d.CompanyId = @CompanyId
      AND d.IsDeleted = 0;

    IF @DepartmentId IS NULL
    BEGIN
        RAISERROR(N'The specified department does not exist for this company.', 16, 1);
        RETURN;
    END

    -- Check for duplicate category name under the same department (excluding self)
    IF EXISTS (
        SELECT 1
        FROM dbo.Category c
        WHERE c.CompanyId = @CompanyId
          AND c.DepartmentId = @DepartmentId
          AND c.CategoryName = @CategoryName
          AND c.GId <> @GId
          AND c.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Another category with the name ''%s'' already exists for this department.', 16, 1, @CategoryName);
        RETURN;
    END

    DECLARE @UpdateDate DATETIME2(7) = SYSUTCDATETIME();

    UPDATE dbo.Category
    SET
        CategoryName = @CategoryName,
        Description  = @Description,
        DepartmentId = @DepartmentId,
        IsActive     = @IsActive,
        LastUpd      = @UpdateDate,
        LastUpdBy    = @LastUpdBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    -- Return the updated record with explicit columns and department metadata
    SELECT
        c.Id,
        c.GId,
        c.CategoryName,
        c.Description,
        c.DepartmentId,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        c.CompanyId,
        c.Created,
        c.CreatedBy,
        c.LastUpd,
        c.LastUpdBy,
        c.IsActive,
        c.IsDeleted
    FROM dbo.Category c
    INNER JOIN dbo.Department d ON c.DepartmentId = d.Id
    WHERE c.GId = @GId
      AND c.CompanyId = @CompanyId
      AND c.IsDeleted = 0;
END;

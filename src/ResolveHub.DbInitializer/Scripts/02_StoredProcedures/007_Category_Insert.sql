CREATE OR ALTER PROCEDURE dbo.Category_Insert
    @CompanyId      INT,
    @DepartmentGId  UNIQUEIDENTIFIER,
    @CategoryName   NVARCHAR(100),
    @Description    NVARCHAR(500) = NULL,
    @IsActive       BIT = 1,
    @CreatedBy      INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate parent Department existence for this company
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

    -- Duplicate check validation for non-deleted categories in the same department
    IF EXISTS (
        SELECT 1
        FROM dbo.Category c
        WHERE c.CompanyId = @CompanyId
          AND c.DepartmentId = @DepartmentId
          AND c.CategoryName = @CategoryName
          AND c.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'A category with the name ''%s'' already exists for this department.', 16, 1, @CategoryName);
        RETURN;
    END

    DECLARE @NewGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedDate DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.Category
    (
        GId,
        CategoryName,
        Description,
        DepartmentId,
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
        @CategoryName,
        @Description,
        @DepartmentId,
        @CompanyId,
        @CreatedDate,
        @CreatedBy,
        NULL,
        NULL,
        @IsActive,
        0
    );

    DECLARE @NewId INT = SCOPE_IDENTITY();

    -- Return the created category with explicit column list and department metadata
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
    WHERE c.Id = @NewId;
END;

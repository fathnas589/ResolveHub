CREATE OR ALTER PROCEDURE dbo.Department_Insert
    @CompanyId      INT,
    @DepartmentName NVARCHAR(100),
    @Description    NVARCHAR(500) = NULL,
    @IsActive       BIT = 1,
    @CreatedBy      INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Duplicate check validation for active/non-deleted departments in same company
    IF EXISTS (
        SELECT 1 
        FROM dbo.Department 
        WHERE CompanyId = @CompanyId 
          AND DepartmentName = @DepartmentName 
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'A department with the name ''%s'' already exists for this company.', 16, 1, @DepartmentName);
        RETURN;
    END

    DECLARE @NewGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedDate DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.Department
    (
        GId,
        DepartmentName,
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
        @DepartmentName,
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

    -- Return the created department with explicit column list
    SELECT
        d.Id,
        d.GId,
        d.DepartmentName,
        d.Description,
        d.CompanyId,
        d.Created,
        d.CreatedBy,
        d.LastUpd,
        d.LastUpdBy,
        d.IsActive,
        d.IsDeleted
    FROM dbo.Department d
    WHERE d.Id = @NewId;
END;

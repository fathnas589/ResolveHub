CREATE OR ALTER PROCEDURE dbo.Department_Update
    @CompanyId      INT,
    @GId            UNIQUEIDENTIFIER,
    @DepartmentName NVARCHAR(100),
    @Description    NVARCHAR(500) = NULL,
    @IsActive       BIT,
    @LastUpdBy      INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if the department exists
    IF NOT EXISTS (
        SELECT 1 
        FROM dbo.Department 
        WHERE GId = @GId 
          AND CompanyId = @CompanyId 
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Department not found.', 16, 1);
        RETURN;
    END

    -- Check for duplicate department name under the same company (excluding self)
    IF EXISTS (
        SELECT 1 
        FROM dbo.Department 
        WHERE CompanyId = @CompanyId 
          AND DepartmentName = @DepartmentName 
          AND GId <> @GId 
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Another department with the name ''%s'' already exists for this company.', 16, 1, @DepartmentName);
        RETURN;
    END

    DECLARE @UpdateDate DATETIME2(7) = SYSUTCDATETIME();

    UPDATE dbo.Department
    SET
        DepartmentName = @DepartmentName,
        Description    = @Description,
        IsActive       = @IsActive,
        LastUpd        = @UpdateDate,
        LastUpdBy      = @LastUpdBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    -- Return the updated record with explicit columns
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
    WHERE d.GId = @GId
      AND d.CompanyId = @CompanyId
      AND d.IsDeleted = 0;
END;

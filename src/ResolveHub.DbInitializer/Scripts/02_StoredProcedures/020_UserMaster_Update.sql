CREATE OR ALTER PROCEDURE dbo.UserMaster_Update
    @CompanyId          INT,
    @GId                UNIQUEIDENTIFIER,
    @DepartmentGId      UNIQUEIDENTIFIER = NULL,
    @UserName           NVARCHAR(100),
    @FullName           NVARCHAR(150),
    @Email              NVARCHAR(150),
    @Phone              NVARCHAR(30) = NULL,
    @RoleCode           NVARCHAR(50),
    @IsActive           BIT,
    @LastUpdBy          INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate user exists for this company
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.UserMaster
        WHERE GId = @GId
          AND CompanyId = @CompanyId
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'User not found.', 16, 1);
        RETURN;
    END

    -- If DepartmentGId is supplied, validate existence for this company
    DECLARE @DepartmentId INT = NULL;
    IF @DepartmentGId IS NOT NULL
    BEGIN
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
    END

    -- Duplicate UserName check excluding current user
    IF EXISTS (
        SELECT 1
        FROM dbo.UserMaster u
        WHERE u.CompanyId = @CompanyId
          AND u.UserName = @UserName
          AND u.GId <> @GId
          AND u.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Another user with username ''%s'' already exists for this company.', 16, 1, @UserName);
        RETURN;
    END

    -- Duplicate Email check excluding current user
    IF EXISTS (
        SELECT 1
        FROM dbo.UserMaster u
        WHERE u.CompanyId = @CompanyId
          AND u.Email = @Email
          AND u.GId <> @GId
          AND u.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'Another user with email ''%s'' already exists for this company.', 16, 1, @Email);
        RETURN;
    END

    DECLARE @UpdateDate DATETIME2(7) = SYSUTCDATETIME();

    UPDATE dbo.UserMaster
    SET
        DepartmentId = @DepartmentId,
        UserName     = @UserName,
        FullName     = @FullName,
        Email        = @Email,
        Phone        = @Phone,
        RoleCode     = @RoleCode,
        IsActive     = @IsActive,
        LastUpd      = @UpdateDate,
        LastUpdBy    = @LastUpdBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    -- Return updated safe user data
    SELECT
        u.Id,
        u.GId,
        u.UserName,
        u.FullName,
        u.Email,
        u.Phone,
        u.RoleCode,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        u.CompanyId,
        u.IsActive,
        u.Created,
        u.CreatedBy,
        u.LastUpd,
        u.LastUpdBy
    FROM dbo.UserMaster u
    LEFT JOIN dbo.Department d ON u.DepartmentId = d.Id
    WHERE u.GId = @GId
      AND u.CompanyId = @CompanyId
      AND u.IsDeleted = 0;
END;

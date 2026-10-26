CREATE OR ALTER PROCEDURE dbo.UserMaster_Insert
    @CompanyId          INT,
    @DepartmentGId      UNIQUEIDENTIFIER = NULL,
    @UserName           NVARCHAR(100),
    @FullName           NVARCHAR(150),
    @Email              NVARCHAR(150),
    @Phone              NVARCHAR(30) = NULL,
    @PasswordHash       NVARCHAR(500),
    @RoleCode           NVARCHAR(50),
    @IsActive           BIT = 1,
    @CreatedBy          INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate CompanyId
    IF @CompanyId <= 0
    BEGIN
        RAISERROR(N'Invalid company ID.', 16, 1);
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

    -- Duplicate UserName check for this company
    IF EXISTS (
        SELECT 1
        FROM dbo.UserMaster u
        WHERE u.CompanyId = @CompanyId
          AND u.UserName = @UserName
          AND u.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'A user with username ''%s'' already exists for this company.', 16, 1, @UserName);
        RETURN;
    END

    -- Duplicate Email check for this company
    IF EXISTS (
        SELECT 1
        FROM dbo.UserMaster u
        WHERE u.CompanyId = @CompanyId
          AND u.Email = @Email
          AND u.IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'A user with email ''%s'' already exists for this company.', 16, 1, @Email);
        RETURN;
    END

    DECLARE @NewGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedDate DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.UserMaster
    (
        GId,
        CompanyId,
        DepartmentId,
        UserName,
        FullName,
        Email,
        Phone,
        PasswordHash,
        RoleCode,
        IsActive,
        IsDeleted,
        Created,
        CreatedBy,
        LastUpd,
        LastUpdBy
    )
    VALUES
    (
        @NewGId,
        @CompanyId,
        @DepartmentId,
        @UserName,
        @FullName,
        @Email,
        @Phone,
        @PasswordHash,
        @RoleCode,
        @IsActive,
        0,
        @CreatedDate,
        @CreatedBy,
        NULL,
        NULL
    );

    DECLARE @NewId INT = SCOPE_IDENTITY();

    -- Return safe user data with department metadata
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
    WHERE u.Id = @NewId;
END;

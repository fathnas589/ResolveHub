CREATE OR ALTER PROCEDURE dbo.SlaPolicy_Insert
    @CompanyId              INT,
    @Name                   NVARCHAR(100),
    @Description            NVARCHAR(500) = NULL,
    @DepartmentGId          UNIQUEIDENTIFIER = NULL,
    @CategoryGId            UNIQUEIDENTIFIER = NULL,
    @SubCategoryGId         UNIQUEIDENTIFIER = NULL,
    @Priority               NVARCHAR(20) = NULL,
    @TicketType             NVARCHAR(30) = NULL,
    @FirstResponseMinutes   INT,
    @ResolutionMinutes      INT,
    @EscalationBeforeMinutes INT = 0,
    @IsDefault              BIT = 0,
    @IsActive               BIT = 1,
    @CreatedBy              INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate minutes
    IF @FirstResponseMinutes <= 0
    BEGIN
        RAISERROR(N'FirstResponseMinutes must be greater than zero.', 16, 1);
        RETURN;
    END;

    IF @ResolutionMinutes <= 0
    BEGIN
        RAISERROR(N'ResolutionMinutes must be greater than zero.', 16, 1);
        RETURN;
    END;

    IF @EscalationBeforeMinutes < 0 OR @EscalationBeforeMinutes >= @ResolutionMinutes
    BEGIN
        RAISERROR(N'EscalationBeforeMinutes must be non-negative and less than ResolutionMinutes.', 16, 1);
        RETURN;
    END;

    -- Look up optional Department
    DECLARE @DepartmentId INT = NULL;
    IF @DepartmentGId IS NOT NULL
    BEGIN
        SELECT @DepartmentId = Id
        FROM dbo.Department
        WHERE GId = @DepartmentGId
          AND CompanyId = @CompanyId
          AND IsDeleted = 0;

        IF @DepartmentId IS NULL
        BEGIN
            RAISERROR(N'The specified department does not exist for this company.', 16, 1);
            RETURN;
        END;
    END;

    -- Look up optional Category
    DECLARE @CategoryId INT = NULL;
    IF @CategoryGId IS NOT NULL
    BEGIN
        SELECT @CategoryId = Id, @DepartmentId = ISNULL(@DepartmentId, DepartmentId)
        FROM dbo.Category
        WHERE GId = @CategoryGId
          AND CompanyId = @CompanyId
          AND IsDeleted = 0;

        IF @CategoryId IS NULL
        BEGIN
            RAISERROR(N'The specified category does not exist for this company.', 16, 1);
            RETURN;
        END;

        -- If Department was also passed, ensure Category belongs to it
        IF @DepartmentGId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Category WHERE Id = @CategoryId AND DepartmentId = @DepartmentId
        )
        BEGIN
            RAISERROR(N'The specified category does not belong to the selected department.', 16, 1);
            RETURN;
        END;
    END;

    -- Look up optional SubCategory
    DECLARE @SubCategoryId INT = NULL;
    IF @SubCategoryGId IS NOT NULL
    BEGIN
        SELECT @SubCategoryId = Id, @CategoryId = ISNULL(@CategoryId, CategoryId)
        FROM dbo.SubCategory
        WHERE GId = @SubCategoryGId
          AND CompanyId = @CompanyId
          AND IsDeleted = 0;

        IF @SubCategoryId IS NULL
        BEGIN
            RAISERROR(N'The specified subcategory does not exist for this company.', 16, 1);
            RETURN;
        END;

        -- If Category was passed, ensure SubCategory belongs to it
        IF @CategoryGId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.SubCategory WHERE Id = @SubCategoryId AND CategoryId = @CategoryId
        )
        BEGIN
            RAISERROR(N'The specified subcategory does not belong to the selected category.', 16, 1);
            RETURN;
        END;
    END;

    -- Duplicate check on Name
    IF EXISTS (
        SELECT 1
        FROM dbo.SlaPolicy
        WHERE CompanyId = @CompanyId
          AND Name = @Name
          AND IsDeleted = 0
    )
    BEGIN
        RAISERROR(N'An SLA policy with the name ''%s'' already exists for this company.', 16, 1, @Name);
        RETURN;
    END;

    -- If marking as default, unset existing defaults for company
    IF @IsDefault = 1
    BEGIN
        UPDATE dbo.SlaPolicy
        SET IsDefault = 0,
            LastUpd = SYSUTCDATETIME(),
            LastUpdBy = @CreatedBy
        WHERE CompanyId = @CompanyId
          AND IsDefault = 1
          AND IsDeleted = 0;
    END;

    DECLARE @NewGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedDate DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.SlaPolicy
    (
        GId,
        CompanyId,
        Name,
        Description,
        DepartmentId,
        CategoryId,
        SubCategoryId,
        Priority,
        TicketType,
        FirstResponseMinutes,
        ResolutionMinutes,
        EscalationBeforeMinutes,
        IsDefault,
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
        @Name,
        @Description,
        @DepartmentId,
        @CategoryId,
        @SubCategoryId,
        @Priority,
        @TicketType,
        @FirstResponseMinutes,
        @ResolutionMinutes,
        @EscalationBeforeMinutes,
        @IsDefault,
        @IsActive,
        0,
        @CreatedDate,
        @CreatedBy,
        NULL,
        NULL
    );

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT
        p.Id,
        p.GId,
        p.CompanyId,
        p.Name,
        p.Description,
        p.DepartmentId,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        p.CategoryId,
        c.GId AS CategoryGId,
        c.CategoryName,
        p.SubCategoryId,
        sc.GId AS SubCategoryGId,
        sc.SubCategoryName,
        p.Priority,
        p.TicketType,
        p.FirstResponseMinutes,
        p.ResolutionMinutes,
        p.EscalationBeforeMinutes,
        p.IsDefault,
        p.IsActive,
        p.IsDeleted,
        p.Created,
        p.CreatedBy,
        u1.FullName AS CreatedByName,
        p.LastUpd,
        p.LastUpdBy,
        u2.FullName AS LastUpdByName
    FROM dbo.SlaPolicy p
    LEFT JOIN dbo.Department d ON p.DepartmentId = d.Id
    LEFT JOIN dbo.Category c ON p.CategoryId = c.Id
    LEFT JOIN dbo.SubCategory sc ON p.SubCategoryId = sc.Id
    LEFT JOIN dbo.UserMaster u1 ON p.CreatedBy = u1.Id
    LEFT JOIN dbo.UserMaster u2 ON p.LastUpdBy = u2.Id
    WHERE p.Id = @NewId;
END;

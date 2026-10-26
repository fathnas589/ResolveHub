CREATE OR ALTER PROCEDURE dbo.Ticket_Insert
    @CompanyId          INT,
    @CreatedByUserId    INT,
    @DepartmentGId      UNIQUEIDENTIFIER,
    @CategoryGId        UNIQUEIDENTIFIER,
    @SubCategoryGId     UNIQUEIDENTIFIER = NULL,
    @TicketType         NVARCHAR(30),
    @Subject            NVARCHAR(200),
    @Description        NVARCHAR(MAX),
    @Priority           NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate creator user
    IF NOT EXISTS (
        SELECT 1 FROM dbo.UserMaster 
        WHERE Id = @CreatedByUserId AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0
    )
    BEGIN
        RAISERROR('Invalid creator user.', 16, 1);
        RETURN;
    END;

    -- Validate Department
    DECLARE @DepartmentId INT;
    SELECT @DepartmentId = Id 
    FROM dbo.Department 
    WHERE GId = @DepartmentGId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @DepartmentId IS NULL
    BEGIN
        RAISERROR('The specified department does not exist for this company.', 16, 1);
        RETURN;
    END;

    -- Validate Category and its relationship to Department
    DECLARE @CategoryId INT;
    SELECT @CategoryId = Id 
    FROM dbo.Category 
    WHERE GId = @CategoryGId AND DepartmentId = @DepartmentId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @CategoryId IS NULL
    BEGIN
        RAISERROR('The specified category does not exist or does not belong to the specified department.', 16, 1);
        RETURN;
    END;

    -- Validate SubCategory if provided
    DECLARE @SubCategoryId INT = NULL;
    IF @SubCategoryGId IS NOT NULL
    BEGIN
        SELECT @SubCategoryId = Id 
        FROM dbo.SubCategory 
        WHERE GId = @SubCategoryGId AND CategoryId = @CategoryId AND CompanyId = @CompanyId AND IsDeleted = 0;

        IF @SubCategoryId IS NULL
        BEGIN
            RAISERROR('The specified sub-category does not exist or does not belong to the specified category.', 16, 1);
            RETURN;
        END;
    END;

    -- Generate Ticket Number
    DECLARE @SeqVal BIGINT = NEXT VALUE FOR dbo.Seq_TicketNumber;
    DECLARE @YearStr NVARCHAR(4) = CAST(YEAR(SYSUTCDATETIME()) AS NVARCHAR(4));
    DECLARE @TicketNumber NVARCHAR(30) = CONCAT('RH-', @YearStr, '-', RIGHT('000000' + CAST(@SeqVal AS NVARCHAR(10)), 6));

    DECLARE @NewTicketGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @InsertedId INT;

    INSERT INTO dbo.Ticket
    (
        GId,
        TicketNumber,
        CompanyId,
        CreatedByUserId,
        DepartmentId,
        CategoryId,
        SubCategoryId,
        TicketType,
        Subject,
        Description,
        Priority,
        Status,
        AssignedToUserId,
        Created,
        IsDeleted
    )
    VALUES
    (
        @NewTicketGId,
        @TicketNumber,
        @CompanyId,
        @CreatedByUserId,
        @DepartmentId,
        @CategoryId,
        @SubCategoryId,
        @TicketType,
        @Subject,
        @Description,
        @Priority,
        'OPEN',
        NULL,
        SYSUTCDATETIME(),
        0
    );

    SET @InsertedId = SCOPE_IDENTITY();

    -- Insert audit history record
    INSERT INTO dbo.TicketHistory
    (
        TicketId,
        ActionType,
        OldValue,
        NewValue,
        Comments,
        Created,
        CreatedByUserId
    )
    VALUES
    (
        @InsertedId,
        'CREATED',
        NULL,
        'OPEN',
        'Ticket created',
        SYSUTCDATETIME(),
        @CreatedByUserId
    );

    -- Return created ticket representation
    SELECT
        t.Id,
        t.GId,
        t.TicketNumber,
        t.TicketType,
        t.Subject,
        t.Description,
        t.Priority,
        t.Status,
        d.GId AS DepartmentGId,
        d.DepartmentName,
        c.GId AS CategoryGId,
        c.CategoryName,
        sc.GId AS SubCategoryGId,
        sc.SubCategoryName,
        uCreator.GId AS CreatedByUserGId,
        uCreator.UserName AS CreatedByUserName,
        uCreator.FullName AS CreatedByFullName,
        uAssignee.GId AS AssignedToUserGId,
        uAssignee.UserName AS AssignedToUserName,
        uAssignee.FullName AS AssignedToFullName,
        t.CompanyId,
        t.Created,
        t.LastUpd,
        t.ClosedAt
    FROM dbo.Ticket t
    INNER JOIN dbo.Department d ON t.DepartmentId = d.Id
    INNER JOIN dbo.Category c ON t.CategoryId = c.Id
    LEFT JOIN dbo.SubCategory sc ON t.SubCategoryId = sc.Id
    INNER JOIN dbo.UserMaster uCreator ON t.CreatedByUserId = uCreator.Id
    LEFT JOIN dbo.UserMaster uAssignee ON t.AssignedToUserId = uAssignee.Id
    WHERE t.Id = @InsertedId;
END;

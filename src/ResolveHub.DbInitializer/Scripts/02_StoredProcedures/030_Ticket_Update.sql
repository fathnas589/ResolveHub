CREATE OR ALTER PROCEDURE dbo.Ticket_Update
    @TicketGId          UNIQUEIDENTIFIER,
    @CompanyId          INT,
    @DepartmentGId      UNIQUEIDENTIFIER,
    @CategoryGId        UNIQUEIDENTIFIER,
    @SubCategoryGId     UNIQUEIDENTIFIER = NULL,
    @Subject            NVARCHAR(200),
    @Description        NVARCHAR(MAX),
    @Priority           NVARCHAR(20),
    @ActorUserId        INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TicketId INT;
    DECLARE @OldPriority NVARCHAR(20);

    SELECT @TicketId = Id, @OldPriority = Priority
    FROM dbo.Ticket
    WHERE GId = @TicketGId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @TicketId IS NULL
    BEGIN
        RAISERROR('Ticket not found.', 16, 1);
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

    -- Validate Category
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

    UPDATE dbo.Ticket
    SET
        DepartmentId = @DepartmentId,
        CategoryId = @CategoryId,
        SubCategoryId = @SubCategoryId,
        Subject = @Subject,
        Description = @Description,
        Priority = @Priority,
        LastUpd = SYSUTCDATETIME(),
        LastUpdBy = @ActorUserId
    WHERE Id = @TicketId;

    -- Audit history
    IF @OldPriority <> @Priority
    BEGIN
        INSERT INTO dbo.TicketHistory
        (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
        VALUES
        (@TicketId, 'PRIORITY_CHANGED', @OldPriority, @Priority, 'Priority updated', SYSUTCDATETIME(), @ActorUserId);
    END;

    INSERT INTO dbo.TicketHistory
    (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
    VALUES
    (@TicketId, 'UPDATED', NULL, NULL, 'Ticket details updated', SYSUTCDATETIME(), @ActorUserId);

    -- Return updated ticket
    SELECT
        t.Id,
        t.GId,
        t.TicketNumber,
        t.CompanyId,
        t.CreatedByUserId,
        t.DepartmentId,
        t.CategoryId,
        t.SubCategoryId,
        t.AssignedToUserId,
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
    WHERE t.Id = @TicketId;
END;

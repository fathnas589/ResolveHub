CREATE OR ALTER PROCEDURE dbo.TicketComment_Insert
    @TicketGId          UNIQUEIDENTIFIER,
    @CompanyId          INT,
    @CommentText        NVARCHAR(2000),
    @IsInternal         BIT = 0,
    @CreatedByUserId    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TicketId INT;

    SELECT @TicketId = Id 
    FROM dbo.Ticket 
    WHERE GId = @TicketGId AND CompanyId = @CompanyId AND IsDeleted = 0;

    IF @TicketId IS NULL
    BEGIN
        RAISERROR('Ticket not found.', 16, 1);
        RETURN;
    END;

    DECLARE @NewCommentGId UNIQUEIDENTIFIER = NEWID();
    DECLARE @InsertedId BIGINT;

    INSERT INTO dbo.TicketComment
    (
        GId,
        TicketId,
        CommentText,
        Created,
        CreatedByUserId,
        IsInternal,
        IsDeleted
    )
    VALUES
    (
        @NewCommentGId,
        @TicketId,
        @CommentText,
        SYSUTCDATETIME(),
        @CreatedByUserId,
        @IsInternal,
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
        @TicketId,
        'COMMENT_ADDED',
        NULL,
        NULL,
        CASE WHEN @IsInternal = 1 THEN 'Internal comment added' ELSE 'Public comment added' END,
        SYSUTCDATETIME(),
        @CreatedByUserId
    );

    -- Return created comment record
    SELECT
        tc.Id,
        tc.GId,
        tc.CommentText,
        tc.IsInternal,
        tc.Created,
        u.GId AS AuthorUserGId,
        u.UserName AS AuthorUserName,
        u.FullName AS AuthorFullName
    FROM dbo.TicketComment tc
    INNER JOIN dbo.UserMaster u ON tc.CreatedByUserId = u.Id
    WHERE tc.Id = @InsertedId;
END;

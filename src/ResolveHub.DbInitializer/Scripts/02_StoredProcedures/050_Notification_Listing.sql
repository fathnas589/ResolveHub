CREATE OR ALTER PROCEDURE dbo.Notification_Listing
    @CompanyId          INT,
    @RecipientUserId    INT,
    @UnreadOnly         BIT = 0,
    @EventType          NVARCHAR(50) = NULL,
    @PageNumber         INT = 1,
    @PageSize           INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Total Count
    SELECT COUNT(1) AS TotalCount
    FROM dbo.NotificationRecipient nr
    INNER JOIN dbo.Notification n ON nr.NotificationId = n.Id
    WHERE n.CompanyId = @CompanyId
      AND nr.RecipientUserId = @RecipientUserId
      AND n.IsDeleted = 0
      AND (@UnreadOnly = 0 OR nr.IsRead = 0)
      AND (@EventType IS NULL OR RTRIM(LTRIM(@EventType)) = N'' OR n.EventType = @EventType);

    -- Paginated Rows
    SELECT
        n.GId,
        n.EventType,
        n.Title,
        n.Message,
        n.ActionUrl,
        nr.IsRead,
        nr.ReadAt,
        nr.Created,
        t.GId AS TicketGId,
        t.TicketNumber
    FROM dbo.NotificationRecipient nr
    INNER JOIN dbo.Notification n ON nr.NotificationId = n.Id
    LEFT JOIN dbo.Ticket t ON n.TicketId = t.Id
    WHERE n.CompanyId = @CompanyId
      AND nr.RecipientUserId = @RecipientUserId
      AND n.IsDeleted = 0
      AND (@UnreadOnly = 0 OR nr.IsRead = 0)
      AND (@EventType IS NULL OR RTRIM(LTRIM(@EventType)) = N'' OR n.EventType = @EventType)
    ORDER BY nr.Created DESC, nr.Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

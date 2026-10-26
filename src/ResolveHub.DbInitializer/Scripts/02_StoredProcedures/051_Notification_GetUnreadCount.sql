CREATE OR ALTER PROCEDURE dbo.Notification_GetUnreadCount
    @CompanyId          INT,
    @RecipientUserId    INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1) AS UnreadCount
    FROM dbo.NotificationRecipient nr
    INNER JOIN dbo.Notification n ON nr.NotificationId = n.Id
    WHERE n.CompanyId = @CompanyId
      AND nr.RecipientUserId = @RecipientUserId
      AND nr.IsRead = 0
      AND n.IsDeleted = 0;
END;

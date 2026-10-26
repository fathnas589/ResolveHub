CREATE OR ALTER PROCEDURE dbo.Notification_MarkAllRead
    @CompanyId          INT,
    @RecipientUserId    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NowUtc DATETIME2(7) = SYSUTCDATETIME();

    UPDATE nr
    SET nr.IsRead = 1,
        nr.ReadAt = @NowUtc,
        nr.LastUpd = @NowUtc
    FROM dbo.NotificationRecipient nr
    INNER JOIN dbo.Notification n ON nr.NotificationId = n.Id
    WHERE n.CompanyId = @CompanyId
      AND nr.RecipientUserId = @RecipientUserId
      AND nr.IsRead = 0
      AND n.IsDeleted = 0;

    SELECT @@ROWCOUNT AS RowsAffected;
END;

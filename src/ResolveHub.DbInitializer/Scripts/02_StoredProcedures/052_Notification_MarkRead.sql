CREATE OR ALTER PROCEDURE dbo.Notification_MarkRead
    @CompanyId          INT,
    @RecipientUserId    INT,
    @NotificationGId    UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NowUtc DATETIME2(7) = SYSUTCDATETIME();

    UPDATE nr
    SET nr.IsRead = 1,
        nr.ReadAt = ISNULL(nr.ReadAt, @NowUtc),
        nr.LastUpd = @NowUtc
    FROM dbo.NotificationRecipient nr
    INNER JOIN dbo.Notification n ON nr.NotificationId = n.Id
    WHERE n.GId = @NotificationGId
      AND n.CompanyId = @CompanyId
      AND nr.RecipientUserId = @RecipientUserId
      AND n.IsDeleted = 0;

    SELECT @@ROWCOUNT AS RowsAffected;
END;

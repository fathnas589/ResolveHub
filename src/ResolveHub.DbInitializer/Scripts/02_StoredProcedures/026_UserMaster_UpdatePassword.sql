CREATE OR ALTER PROCEDURE dbo.UserMaster_UpdatePassword
    @CompanyId      INT,
    @GId            UNIQUEIDENTIFIER,
    @PasswordHash   NVARCHAR(500),
    @LastUpdBy      INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.UserMaster
    SET
        PasswordHash = @PasswordHash,
        LastUpd      = SYSUTCDATETIME(),
        LastUpdBy    = @LastUpdBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    SELECT CAST(CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END AS BIT) AS IsSuccess;
END;

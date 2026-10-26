CREATE OR ALTER PROCEDURE dbo.UserMaster_Delete
    @CompanyId  INT,
    @GId        UNIQUEIDENTIFIER,
    @LastUpdBy  INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.UserMaster
    SET
        IsDeleted = 1,
        IsActive  = 0,
        LastUpd   = SYSUTCDATETIME(),
        LastUpdBy = @LastUpdBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    SELECT CAST(CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END AS BIT) AS IsSuccess;
END;

CREATE OR ALTER PROCEDURE dbo.SlaPolicy_Delete
    @CompanyId      INT,
    @GId            UNIQUEIDENTIFIER,
    @DeletedBy      INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.SlaPolicy
    SET IsDeleted = 1,
        IsActive = 0,
        LastUpd = SYSUTCDATETIME(),
        LastUpdBy = @DeletedBy
    WHERE GId = @GId
      AND CompanyId = @CompanyId
      AND IsDeleted = 0;

    SELECT @@ROWCOUNT AS RowsAffected;
END;

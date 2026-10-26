CREATE OR ALTER PROCEDURE dbo.Department_Get
    @CompanyId  INT,
    @GId        UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.Id,
        d.GId,
        d.DepartmentName,
        d.Description,
        d.CompanyId,
        d.Created,
        d.CreatedBy,
        d.LastUpd,
        d.LastUpdBy,
        d.IsActive,
        d.IsDeleted
    FROM dbo.Department d
    WHERE d.GId = @GId
      AND d.CompanyId = @CompanyId
      AND d.IsDeleted = 0;
END;

CREATE OR ALTER PROCEDURE dbo.SlaPolicy_DuplicateCheck
    @CompanyId      INT,
    @Name           NVARCHAR(100),
    @ExcludeGId     UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM dbo.SlaPolicy
        WHERE CompanyId = @CompanyId
          AND Name = @Name
          AND IsDeleted = 0
          AND (@ExcludeGId IS NULL OR GId <> @ExcludeGId)
    )
    BEGIN
        SELECT CAST(1 AS BIT) AS IsDuplicate;
    END
    ELSE
    BEGIN
        SELECT CAST(0 AS BIT) AS IsDuplicate;
    END
END;

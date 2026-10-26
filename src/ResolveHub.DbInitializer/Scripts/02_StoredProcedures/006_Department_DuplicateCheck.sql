CREATE OR ALTER PROCEDURE dbo.Department_DuplicateCheck
    @CompanyId      INT,
    @DepartmentName NVARCHAR(100),
    @ExcludeGId     UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Department d
            WHERE d.CompanyId = @CompanyId
              AND d.DepartmentName = @DepartmentName
              AND d.IsDeleted = 0
              AND (@ExcludeGId IS NULL OR d.GId <> @ExcludeGId)
        ) THEN 1 ELSE 0 END AS BIT
    ) AS IsDuplicate;
END;

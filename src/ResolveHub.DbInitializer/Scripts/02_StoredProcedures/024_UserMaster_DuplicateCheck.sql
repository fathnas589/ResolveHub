CREATE OR ALTER PROCEDURE dbo.UserMaster_DuplicateCheck
    @CompanyId      INT,
    @UserName       NVARCHAR(100) = NULL,
    @Email          NVARCHAR(150) = NULL,
    @ExcludeGId     UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CAST(CASE WHEN @UserName IS NOT NULL AND EXISTS (
            SELECT 1 FROM dbo.UserMaster u
            WHERE u.CompanyId = @CompanyId
              AND u.UserName = @UserName
              AND u.IsDeleted = 0
              AND (@ExcludeGId IS NULL OR u.GId <> @ExcludeGId)
        ) THEN 1 ELSE 0 END AS BIT) AS IsDuplicateUserName,
        CAST(CASE WHEN @Email IS NOT NULL AND EXISTS (
            SELECT 1 FROM dbo.UserMaster u
            WHERE u.CompanyId = @CompanyId
              AND u.Email = @Email
              AND u.IsDeleted = 0
              AND (@ExcludeGId IS NULL OR u.GId <> @ExcludeGId)
        ) THEN 1 ELSE 0 END AS BIT) AS IsDuplicateEmail;
END;

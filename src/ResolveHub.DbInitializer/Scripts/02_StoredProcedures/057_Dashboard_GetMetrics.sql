CREATE OR ALTER PROCEDURE dbo.Dashboard_GetMetrics
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TodayStartUtc DATETIME2(7) = DATEADD(day, DATEDIFF(day, 0, SYSUTCDATETIME()), 0);

    -- 1. High-Level KPI Summary
    SELECT
        COUNT(1) AS TotalTickets,
        ISNULL(SUM(CASE WHEN t.Status IN ('OPEN', 'ASSIGNED', 'PENDING_USER') THEN 1 ELSE 0 END), 0) AS OpenTickets,
        ISNULL(SUM(CASE WHEN t.Status = 'IN_PROGRESS' THEN 1 ELSE 0 END), 0) AS InProgressTickets,
        ISNULL(SUM(CASE WHEN t.Status IN ('RESOLVED', 'CLOSED') AND (ts.ResolvedAt >= @TodayStartUtc OR (ts.ResolvedAt IS NULL AND t.LastUpd >= @TodayStartUtc)) THEN 1 ELSE 0 END), 0) AS ResolvedToday,
        ISNULL(SUM(CASE WHEN (ts.FirstResponseBreached = 1 OR ts.ResolutionBreached = 1) THEN 1 ELSE 0 END), 0) AS BreachedTickets,
        ISNULL(SUM(CASE WHEN t.Status NOT IN ('RESOLVED', 'CLOSED', 'REJECTED') AND ts.ResolutionBreached = 0 AND ts.ResolutionDueAt IS NOT NULL AND ts.ResolutionDueAt < DATEADD(hour, 4, SYSUTCDATETIME()) THEN 1 ELSE 0 END), 0) AS WarningTickets,
        ISNULL(AVG(CASE WHEN ts.FirstRespondedAt IS NOT NULL THEN CAST(DATEDIFF(minute, ts.Created, ts.FirstRespondedAt) AS FLOAT) / 60.0 ELSE NULL END), 0.0) AS AvgResponseHours
    FROM dbo.Ticket t
    LEFT JOIN dbo.TicketSla ts ON ts.TicketId = t.Id
    WHERE t.CompanyId = @CompanyId AND t.IsDeleted = 0;

    -- 2. Status Breakdown
    SELECT
        t.Status,
        COUNT(1) AS TicketCount
    FROM dbo.Ticket t
    WHERE t.CompanyId = @CompanyId AND t.IsDeleted = 0
    GROUP BY t.Status;

    -- 3. Priority Breakdown
    SELECT
        t.Priority,
        COUNT(1) AS TicketCount
    FROM dbo.Ticket t
    WHERE t.CompanyId = @CompanyId AND t.IsDeleted = 0
    GROUP BY t.Priority;

    -- 4. Department Distribution
    SELECT
        ISNULL(d.DepartmentName, 'General / Unassigned') AS DepartmentName,
        COUNT(1) AS TicketCount
    FROM dbo.Ticket t
    LEFT JOIN dbo.Department d ON d.Id = t.DepartmentId AND d.CompanyId = @CompanyId
    WHERE t.CompanyId = @CompanyId AND t.IsDeleted = 0
    GROUP BY d.DepartmentName;

    -- 5. Recent 10 Tickets
    SELECT TOP 10
        t.GId,
        t.TicketNumber,
        t.Subject,
        t.Status,
        t.Priority,
        ISNULL(d.DepartmentName, 'General / Unassigned') AS DepartmentName,
        ISNULL(u.FullName, 'Unassigned') AS AssignedToName,
        t.Created,
        ISNULL(ts.ResolutionBreached, 0) AS IsBreached
    FROM dbo.Ticket t
    LEFT JOIN dbo.Department d ON d.Id = t.DepartmentId AND d.CompanyId = @CompanyId
    LEFT JOIN dbo.UserMaster u ON u.Id = t.AssignedToUserId AND u.CompanyId = @CompanyId
    LEFT JOIN dbo.TicketSla ts ON ts.TicketId = t.Id
    WHERE t.CompanyId = @CompanyId AND t.IsDeleted = 0
    ORDER BY t.Id DESC;
END;

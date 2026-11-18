CREATE OR ALTER PROCEDURE dbo.Report_GetPerformanceMetrics
    @CompanyId INT,
    @FromDateUtc DATETIME2(7) = NULL,
    @ToDateUtc DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDateUtc IS NULL
        SET @FromDateUtc = DATEADD(day, -30, SYSUTCDATETIME());

    IF @ToDateUtc IS NULL
        SET @ToDateUtc = SYSUTCDATETIME();

    -- 1. Executive Summary Overview
    SELECT
        COUNT(1) AS TotalTickets,
        ISNULL(SUM(CASE WHEN t.Status IN ('RESOLVED', 'CLOSED') THEN 1 ELSE 0 END), 0) AS ResolvedTickets,
        ISNULL(SUM(CASE WHEN (ts.FirstResponseBreached = 1 OR ts.ResolutionBreached = 1) THEN 1 ELSE 0 END), 0) AS BreachedTickets,
        ISNULL(SUM(CASE WHEN (ts.FirstResponseBreached = 0 AND ts.ResolutionBreached = 0) THEN 1 ELSE 0 END), 0) AS CompliantTickets,
        CASE
            WHEN COUNT(ts.Id) = 0 THEN 100.0
            ELSE CAST(ROUND(100.0 * CAST(SUM(CASE WHEN (ts.FirstResponseBreached = 0 AND ts.ResolutionBreached = 0) THEN 1 ELSE 0 END) AS FLOAT) / CAST(COUNT(ts.Id) AS FLOAT), 1) AS FLOAT)
        END AS CompliancePercentage,
        ISNULL(AVG(CASE WHEN ts.FirstRespondedAt IS NOT NULL THEN CAST(DATEDIFF(minute, ts.Created, ts.FirstRespondedAt) AS FLOAT) / 60.0 ELSE NULL END), 0.0) AS AvgFirstResponseHours,
        ISNULL(AVG(CASE WHEN ts.ResolvedAt IS NOT NULL THEN CAST(DATEDIFF(minute, ts.Created, ts.ResolvedAt) AS FLOAT) / 60.0 ELSE NULL END), 0.0) AS AvgResolutionHours
    FROM dbo.Ticket t
    LEFT JOIN dbo.TicketSla ts ON ts.TicketId = t.Id
    WHERE t.CompanyId = @CompanyId
      AND t.IsDeleted = 0
      AND t.Created >= @FromDateUtc
      AND t.Created <= @ToDateUtc;

    -- 2. Department Breakdown
    SELECT
        ISNULL(d.DepartmentName, 'General / Unassigned') AS DepartmentName,
        COUNT(1) AS TotalTickets,
        ISNULL(SUM(CASE WHEN t.Status IN ('RESOLVED', 'CLOSED') THEN 1 ELSE 0 END), 0) AS ResolvedCount,
        ISNULL(SUM(CASE WHEN (ts.FirstResponseBreached = 1 OR ts.ResolutionBreached = 1) THEN 1 ELSE 0 END), 0) AS BreachedCount,
        CASE
            WHEN COUNT(ts.Id) = 0 THEN 100.0
            ELSE CAST(ROUND(100.0 * CAST(SUM(CASE WHEN (ts.FirstResponseBreached = 0 AND ts.ResolutionBreached = 0) THEN 1 ELSE 0 END) AS FLOAT) / CAST(COUNT(ts.Id) AS FLOAT), 1) AS FLOAT)
        END AS ComplianceRate,
        ISNULL(AVG(CASE WHEN ts.ResolvedAt IS NOT NULL THEN CAST(DATEDIFF(minute, ts.Created, ts.ResolvedAt) AS FLOAT) / 60.0 ELSE NULL END), 0.0) AS AvgResolutionHours
    FROM dbo.Ticket t
    LEFT JOIN dbo.Department d ON d.Id = t.DepartmentId AND d.CompanyId = @CompanyId
    LEFT JOIN dbo.TicketSla ts ON ts.TicketId = t.Id
    WHERE t.CompanyId = @CompanyId
      AND t.IsDeleted = 0
      AND t.Created >= @FromDateUtc
      AND t.Created <= @ToDateUtc
    GROUP BY d.DepartmentName
    ORDER BY TotalTickets DESC;

    -- 3. Agent Performance Leaderboard
    SELECT
        u.FullName AS AgentName,
        u.Email AS AgentEmail,
        COUNT(1) AS AssignedCount,
        ISNULL(SUM(CASE WHEN t.Status IN ('RESOLVED', 'CLOSED') THEN 1 ELSE 0 END), 0) AS ResolvedCount,
        ISNULL(SUM(CASE WHEN (ts.FirstResponseBreached = 1 OR ts.ResolutionBreached = 1) THEN 1 ELSE 0 END), 0) AS BreachedCount,
        CASE
            WHEN COUNT(ts.Id) = 0 THEN 100.0
            ELSE CAST(ROUND(100.0 * CAST(SUM(CASE WHEN (ts.FirstResponseBreached = 0 AND ts.ResolutionBreached = 0) THEN 1 ELSE 0 END) AS FLOAT) / CAST(COUNT(ts.Id) AS FLOAT), 1) AS FLOAT)
        END AS ComplianceRate
    FROM dbo.Ticket t
    INNER JOIN dbo.UserMaster u ON u.Id = t.AssignedToUserId AND u.CompanyId = @CompanyId
    LEFT JOIN dbo.TicketSla ts ON ts.TicketId = t.Id
    WHERE t.CompanyId = @CompanyId
      AND t.IsDeleted = 0
      AND t.Created >= @FromDateUtc
      AND t.Created <= @ToDateUtc
    GROUP BY u.FullName, u.Email
    ORDER BY ResolvedCount DESC;
END;

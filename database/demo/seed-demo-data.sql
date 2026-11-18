/* ============================================================================
   ResolveHub — Demo Data Reset (CompanyId = 1)
   ----------------------------------------------------------------------------
   Replaces ALL CompanyId = 1 data (except the 'admin' account) with a clean,
   realistic demo dataset for screenshots and walkthroughs:
     - 4 departments, 12 categories, 26 subcategories
     - 5 agents + 8 requesters (password = same as the admin account)
     - 5 SLA policies (default, priority-based, department-based)
     - 36 tickets over the last 30 days with consistent SLA timers,
       history, comments and a few notifications

   This file is intentionally NOT under DbInitializer/Scripts, so it never runs
   automatically. Run it manually (back up first!):

     sqlcmd -S "localhost\SQLEXPRESS" -E -d ResolveHubDb -b -i database/demo/seed-demo-data.sql

   The whole reset runs in one transaction: it either fully succeeds or changes nothing.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @CompanyId INT = 1;
DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();
DECLARE @AdminId INT = (SELECT Id FROM dbo.UserMaster WHERE CompanyId = @CompanyId AND UserName = N'admin' AND IsDeleted = 0);

IF @AdminId IS NULL
BEGIN
    RAISERROR(N'Admin user (CompanyId=1, UserName=admin) not found. Run DbInitializer first.', 16, 1);
    RETURN;
END;

DECLARE @PasswordHash NVARCHAR(500) = (SELECT PasswordHash FROM dbo.UserMaster WHERE Id = @AdminId);

BEGIN TRANSACTION;

/* ----------------------------------------------------------------------------
   1. Remove existing CompanyId = 1 data (children first)
   ---------------------------------------------------------------------------- */
DECLARE @DoomedUsers TABLE (Id INT PRIMARY KEY);
INSERT INTO @DoomedUsers (Id) SELECT Id FROM dbo.UserMaster WHERE CompanyId = @CompanyId AND Id <> @AdminId;

DELETE nd FROM dbo.NotificationDelivery nd
JOIN dbo.NotificationRecipient nr ON nr.Id = nd.NotificationRecipientId
JOIN dbo.Notification n ON n.Id = nr.NotificationId
WHERE n.CompanyId = @CompanyId OR nr.RecipientUserId IN (SELECT Id FROM @DoomedUsers);

DELETE nr FROM dbo.NotificationRecipient nr
JOIN dbo.Notification n ON n.Id = nr.NotificationId
WHERE n.CompanyId = @CompanyId OR nr.RecipientUserId IN (SELECT Id FROM @DoomedUsers);

DELETE FROM dbo.Notification WHERE CompanyId = @CompanyId;

DELETE tc FROM dbo.TicketComment tc JOIN dbo.Ticket t ON t.Id = tc.TicketId WHERE t.CompanyId = @CompanyId;
DELETE th FROM dbo.TicketHistory th JOIN dbo.Ticket t ON t.Id = th.TicketId WHERE t.CompanyId = @CompanyId;
DELETE ts FROM dbo.TicketSla ts JOIN dbo.Ticket t ON t.Id = ts.TicketId WHERE t.CompanyId = @CompanyId;
DELETE FROM dbo.Ticket WHERE CompanyId = @CompanyId;

DELETE FROM dbo.SlaPolicy WHERE CompanyId = @CompanyId;
DELETE FROM dbo.SubCategory WHERE CompanyId = @CompanyId;
DELETE FROM dbo.Category WHERE CompanyId = @CompanyId;

UPDATE dbo.UserMaster SET DepartmentId = NULL WHERE Id = @AdminId;
DELETE FROM dbo.UserMaster WHERE Id IN (SELECT Id FROM @DoomedUsers);
DELETE FROM dbo.Department WHERE CompanyId = @CompanyId;

-- Fresh, readable ticket numbers (RH-2026-001001 …) when no other company has tickets.
IF NOT EXISTS (SELECT 1 FROM dbo.Ticket)
    ALTER SEQUENCE dbo.Seq_TicketNumber RESTART WITH 1001;

/* ----------------------------------------------------------------------------
   2. Departments
   ---------------------------------------------------------------------------- */
DECLARE @Dept TABLE (DKey NVARCHAR(10) PRIMARY KEY, Id INT);

INSERT INTO dbo.Department (DepartmentName, Description, CompanyId, Created, CreatedBy)
VALUES
 (N'IT Service Desk',  N'Hardware, software, network access and collaboration tools', @CompanyId, DATEADD(DAY, -120, @Now), @AdminId),
 (N'Human Resources',  N'Payroll, leave, onboarding and employee services',            @CompanyId, DATEADD(DAY, -120, @Now), @AdminId),
 (N'Finance & Accounts', N'Expense claims, vendor payments and purchase orders',          @CompanyId, DATEADD(DAY, -120, @Now), @AdminId),
 (N'Facilities',       N'Workplace, maintenance and physical security',                 @CompanyId, DATEADD(DAY, -120, @Now), @AdminId);

INSERT INTO @Dept (DKey, Id)
SELECT CASE DepartmentName WHEN N'IT Service Desk' THEN N'IT' WHEN N'Human Resources' THEN N'HR'
                           WHEN N'Finance & Accounts' THEN N'FIN' ELSE N'FAC' END, Id
FROM dbo.Department WHERE CompanyId = @CompanyId;

/* ----------------------------------------------------------------------------
   3. Categories & subcategories
   ---------------------------------------------------------------------------- */
DECLARE @CatSrc TABLE (CKey NVARCHAR(10) PRIMARY KEY, DKey NVARCHAR(10), Name NVARCHAR(100), Descr NVARCHAR(500));
INSERT INTO @CatSrc VALUES
 (N'HW',   N'IT',  N'Hardware',                N'Laptops, desktops, monitors and peripherals'),
 (N'SW',   N'IT',  N'Software & Licenses',     N'Installations, upgrades and license renewals'),
 (N'NET',  N'IT',  N'Network & Access',        N'VPN, Wi-Fi, accounts and password resets'),
 (N'COL',  N'IT',  N'Email & Collaboration',   N'Outlook, Teams and shared mailboxes'),
 (N'PAY',  N'HR',  N'Payroll',                 N'Salary, payslips and tax deductions'),
 (N'LEAVE',N'HR',  N'Leave & Attendance',      N'Leave balances and attendance corrections'),
 (N'ONB',  N'HR',  N'Onboarding',              N'New joiner setup, ID cards and induction'),
 (N'EXP',  N'FIN', N'Expense Claims',          N'Travel, meals and reimbursement queries'),
 (N'VEN',  N'FIN', N'Vendor Payments',         N'Invoice status and vendor registration'),
 (N'PO',   N'FIN', N'Purchase Orders',         N'PO creation and approvals'),
 (N'WORK', N'FAC', N'Workplace',               N'Desks, seating and climate control'),
 (N'MAINT',N'FAC', N'Maintenance & Security',  N'Electrical, plumbing and access cards');

DECLARE @Cat TABLE (CKey NVARCHAR(10) PRIMARY KEY, Id INT);
INSERT INTO dbo.Category (CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy)
SELECT c.Name, c.Descr, d.Id, @CompanyId, DATEADD(DAY, -118, @Now), @AdminId
FROM @CatSrc c JOIN @Dept d ON d.DKey = c.DKey;
INSERT INTO @Cat (CKey, Id)
SELECT c.CKey, cat.Id FROM @CatSrc c
JOIN @Dept d ON d.DKey = c.DKey
JOIN dbo.Category cat ON cat.CompanyId = @CompanyId AND cat.DepartmentId = d.Id AND cat.CategoryName = c.Name;

DECLARE @SubSrc TABLE (SKey NVARCHAR(10) PRIMARY KEY, CKey NVARCHAR(10), Name NVARCHAR(100), Descr NVARCHAR(250));
INSERT INTO @SubSrc VALUES
 (N'LAP',  N'HW',   N'Laptop & Desktop',       N'Faults, replacements and new devices'),
 (N'MON',  N'HW',   N'Monitors & Peripherals', N'Displays, docks, keyboards and headsets'),
 (N'INST', N'SW',   N'Installation Request',   N'New software installs and upgrades'),
 (N'LIC',  N'SW',   N'License Renewal',        N'Expiring or additional licenses'),
 (N'VPN',  N'NET',  N'VPN & Remote Access',    N'Connectivity from home or travel'),
 (N'WIFI', N'NET',  N'Office Wi-Fi',           N'Wireless coverage and performance'),
 (N'ACC',  N'NET',  N'Account & Password',     N'Lockouts, resets and MFA'),
 (N'OUT',  N'COL',  N'Outlook & Mailboxes',    N'Mail flow, shared mailboxes and calendars'),
 (N'TEAMS',N'COL',  N'Microsoft Teams',        N'Meetings, channels and calling'),
 (N'SAL',  N'PAY',  N'Salary Discrepancy',     N'Incorrect or missing salary components'),
 (N'SLIP', N'PAY',  N'Payslip & Tax Forms',    N'Payslip copies and tax certificates'),
 (N'BAL',  N'LEAVE',N'Leave Balance',          N'Balance queries and carry-forward'),
 (N'ATT',  N'LEAVE',N'Attendance Correction',  N'Missed punches and timesheet fixes'),
 (N'NJ',   N'ONB',  N'New Joiner Setup',       N'Accounts and equipment for new hires'),
 (N'IDC',  N'ONB',  N'ID Card',                N'New, lost or damaged ID cards'),
 (N'TRV',  N'EXP',  N'Travel Reimbursement',   N'Flights, hotels and mileage'),
 (N'MEAL', N'EXP',  N'Meals & Entertainment',  N'Client meals and team events'),
 (N'INV',  N'VEN',  N'Invoice Status',         N'Payment status of vendor invoices'),
 (N'NVEN', N'VEN',  N'New Vendor Registration',N'Onboarding a new supplier'),
 (N'POAPP',N'PO',   N'PO Approval',            N'Pending or rejected approvals'),
 (N'POCR', N'PO',   N'PO Creation',            N'Raising a new purchase order'),
 (N'DESK', N'WORK', N'Desk & Seating',         N'Desk moves and ergonomic requests'),
 (N'AC',   N'WORK', N'Air Conditioning',       N'Temperature and ventilation'),
 (N'ELEC', N'MAINT',N'Electrical',             N'Power sockets and lighting'),
 (N'PLMB', N'MAINT',N'Plumbing',               N'Leaks and washroom issues'),
 (N'CARD', N'MAINT',N'Access Card',            N'Building and floor access');

DECLARE @Sub TABLE (SKey NVARCHAR(10) PRIMARY KEY, Id INT);
INSERT INTO dbo.SubCategory (CategoryId, SubCategoryName, Description, CompanyId, Created, CreatedBy)
SELECT c.Id, s.Name, s.Descr, @CompanyId, DATEADD(DAY, -118, @Now), @AdminId
FROM @SubSrc s JOIN @Cat c ON c.CKey = s.CKey;
INSERT INTO @Sub (SKey, Id)
SELECT s.SKey, sc.Id FROM @SubSrc s
JOIN @Cat c ON c.CKey = s.CKey
JOIN dbo.SubCategory sc ON sc.CompanyId = @CompanyId AND sc.CategoryId = c.Id AND sc.SubCategoryName = s.Name;

/* ----------------------------------------------------------------------------
   4. Users (fictional people; .demo domain)
   ---------------------------------------------------------------------------- */
DECLARE @UserSrc TABLE (UKey NVARCHAR(20) PRIMARY KEY, FullName NVARCHAR(150), Role NVARCHAR(10), DKey NVARCHAR(10), Phone NVARCHAR(30), DaysAgo INT);
INSERT INTO @UserSrc VALUES
 (N'priya.kapoor',  N'Priya Kapoor',   N'AGENT', N'IT',  N'+1 555 0101', 110),
 (N'daniel.reyes',  N'Daniel Reyes',   N'AGENT', N'IT',  N'+1 555 0102', 104),
 (N'sara.ahmed',    N'Sara Ahmed',     N'AGENT', N'HR',  N'+1 555 0103', 100),
 (N'michael.chen',  N'Michael Chen',   N'AGENT', N'FIN', N'+1 555 0104', 96),
 (N'lena.fischer',  N'Lena Fischer',   N'AGENT', N'FAC', N'+1 555 0105', 90),
 (N'arjun.mehta',   N'Arjun Mehta',    N'USER',  N'FIN', N'+1 555 0201', 80),
 (N'emily.carter',  N'Emily Carter',   N'USER',  N'HR',  N'+1 555 0202', 75),
 (N'ravi.nair',     N'Ravi Nair',      N'USER',  N'IT',  N'+1 555 0203', 70),
 (N'sophie.martin', N'Sophie Martin',  N'USER',  N'FAC', N'+1 555 0204', 66),
 (N'james.okafor',  N'James Okafor',   N'USER',  N'FIN', N'+1 555 0205', 60),
 (N'aisha.khan',    N'Aisha Khan',     N'USER',  N'HR',  N'+1 555 0206', 55),
 (N'tom.becker',    N'Tom Becker',     N'USER',  N'IT',  N'+1 555 0207', 50),
 (N'nina.rossi',    N'Nina Rossi',     N'USER',  N'FAC', N'+1 555 0208', 45);

INSERT INTO dbo.UserMaster (CompanyId, DepartmentId, UserName, FullName, Email, Phone, PasswordHash, RoleCode, IsActive, IsDeleted, Created, CreatedBy)
SELECT @CompanyId, d.Id, u.UKey, u.FullName, u.UKey + N'@resolvehub.demo', u.Phone, @PasswordHash, u.Role, 1, 0, DATEADD(DAY, -u.DaysAgo, @Now), @AdminId
FROM @UserSrc u LEFT JOIN @Dept d ON d.DKey = u.DKey;

DECLARE @User TABLE (UKey NVARCHAR(20) PRIMARY KEY, Id INT, FullName NVARCHAR(150), UserName NVARCHAR(100));
INSERT INTO @User SELECT UserName, Id, FullName, UserName FROM dbo.UserMaster WHERE CompanyId = @CompanyId;

/* ----------------------------------------------------------------------------
   5. SLA policies
   ---------------------------------------------------------------------------- */
INSERT INTO dbo.SlaPolicy (CompanyId, Name, Description, DepartmentId, Priority, TicketType, FirstResponseMinutes, ResolutionMinutes, EscalationBeforeMinutes, IsDefault, Created, CreatedBy)
VALUES
 (@CompanyId, N'Standard Service SLA',  N'Company-wide default for all requests',                 NULL, NULL,        NULL,              240, 2880, 360, 1, DATEADD(DAY, -115, @Now), @AdminId),
 (@CompanyId, N'Critical Incident SLA', N'Business-stopping incidents across all departments',   NULL, N'CRITICAL', NULL,               30,  240,  60, 0, DATEADD(DAY, -115, @Now), @AdminId),
 (@CompanyId, N'High Priority SLA',     N'Major impact with limited workaround',                 NULL, N'HIGH',     NULL,               60,  480, 120, 0, DATEADD(DAY, -115, @Now), @AdminId),
 (@CompanyId, N'HR Service Requests',   N'Payroll, leave and onboarding requests',
     (SELECT Id FROM @Dept WHERE DKey = N'HR'),  NULL, N'SERVICE_REQUEST', 480, 4320, 720, 0, DATEADD(DAY, -115, @Now), @AdminId),
 (@CompanyId, N'Facilities Response',   N'Workplace and maintenance requests',
     (SELECT Id FROM @Dept WHERE DKey = N'FAC'), NULL, NULL,              240, 1440, 240, 0, DATEADD(DAY, -115, @Now), @AdminId);

/* ----------------------------------------------------------------------------
   6. Tickets
   Columns: #, sub, type, priority, subject, description, requester, agent,
            status, created (minutes ago), first response after (min), resolved after (min), resolution note
   ---------------------------------------------------------------------------- */
DECLARE @T TABLE (
    N INT PRIMARY KEY, SKey NVARCHAR(10), TicketType NVARCHAR(30), Priority NVARCHAR(20),
    Subject NVARCHAR(200), Descr NVARCHAR(1000), Requester NVARCHAR(20), Agent NVARCHAR(20) NULL,
    Status NVARCHAR(30), CreatedAgo INT, RespondAfter INT NULL, ResolveAfter INT NULL, Note NVARCHAR(500) NULL,
    -- computed below
    TicketId INT NULL, CreatedAt DATETIME2(7) NULL, PolicyId INT NULL);

INSERT INTO @T (N, SKey, TicketType, Priority, Subject, Descr, Requester, Agent, Status, CreatedAgo, RespondAfter, ResolveAfter, Note) VALUES
-- ===== Closed / resolved on time =====
 (1,  N'VPN',  N'COMPLAINT',       N'HIGH',     N'VPN disconnects every 10 minutes when working from home', N'Since yesterday my VPN session drops roughly every 10 minutes, which interrupts calls and file uploads.', N'ravi.nair', N'priya.kapoor', N'CLOSED', 41000, 25, 300, N'Updated the VPN client to 5.2 and switched the profile to the new gateway. Stable for 24h.'),
 (2,  N'LAP',  N'SERVICE_REQUEST', N'MEDIUM',   N'New laptop for incoming data analyst', N'Please prepare a laptop with the standard analytics image for a new hire starting next Monday.', N'tom.becker', N'daniel.reyes', N'CLOSED', 39500, 90, 1500, N'Laptop imaged with analytics profile and handed over at the IT desk.'),
 (3,  N'SAL',  N'COMPLAINT',       N'HIGH',     N'Overtime missing from October salary', N'My October payslip does not include the 12 hours of approved overtime.', N'arjun.mehta', N'sara.ahmed', N'CLOSED', 37800, 45, 420, N'Overtime was approved after the payroll cut-off. Adjustment processed as an off-cycle payment.'),
 (4,  N'TRV',  N'SERVICE_REQUEST', N'LOW',      N'Reimbursement for client visit to Chicago', N'Submitting hotel and flight receipts for the client workshop on the 3rd.', N'james.okafor', N'michael.chen', N'CLOSED', 36000, 200, 2200, N'Claim approved and scheduled in the next reimbursement run.'),
 (5,  N'AC',   N'COMPLAINT',       N'MEDIUM',   N'Meeting room 4B is too warm', N'Room 4B stays above 27°C in the afternoon, making meetings uncomfortable.', N'sophie.martin', N'lena.fischer', N'CLOSED', 34500, 60, 900, N'Thermostat recalibrated and a blocked vent cleared.'),
 (6,  N'ACC',  N'COMPLAINT',       N'CRITICAL', N'Finance team locked out after MFA change', N'All five members of the AP team are getting MFA errors and cannot log in to the ERP.', N'arjun.mehta', N'priya.kapoor', N'CLOSED', 33000, 10, 150, N'Conditional access rule rolled back; MFA re-registration link sent to the team.'),
 (7,  N'SLIP', N'SERVICE_REQUEST', N'LOW',      N'Annual tax certificate for 2025', N'Need my annual tax certificate for a home loan application.', N'emily.carter', N'sara.ahmed', N'CLOSED', 31000, 300, 2600, N'Certificate generated and shared via the employee portal.'),
 (8,  N'INV',  N'SERVICE_REQUEST', N'MEDIUM',   N'Status of invoice INV-88213 from Northwind Supplies', N'Vendor says invoice INV-88213 is 15 days overdue. Can you confirm payment status?', N'james.okafor', N'michael.chen', N'CLOSED', 29000, 120, 1900, N'Invoice was on hold for a PO mismatch; corrected and released for payment.'),
 (9,  N'TEAMS',N'COMPLAINT',       N'MEDIUM',   N'Teams meetings have no audio on conference room PC', N'The Teams Room PC in 2A joins meetings but there is no audio either way.', N'tom.becker', N'daniel.reyes', N'CLOSED', 27000, 70, 600, N'Audio driver reinstalled and default device fixed in Teams Rooms settings.'),
 (10, N'CARD', N'SERVICE_REQUEST', N'MEDIUM',   N'Access card not opening 5th floor', N'My card works at reception but not on the 5th floor where my team moved.', N'nina.rossi', N'lena.fischer', N'CLOSED', 25000, 40, 500, N'Access group updated to include floor 5.'),
 (11, N'BAL',  N'SERVICE_REQUEST', N'LOW',      N'Leave balance not reflecting carry-forward days', N'My 4 carry-forward days from last year are not visible in the leave portal.', N'aisha.khan', N'sara.ahmed', N'RESOLVED', 20000, 260, 2400, N'Carry-forward balance applied manually; portal now shows 18 days.'),
 (12, N'OUT',  N'COMPLAINT',       N'HIGH',     N'Shared mailbox finance@ not receiving external email', N'External senders get a bounce when emailing the finance shared mailbox.', N'arjun.mehta', N'priya.kapoor', N'RESOLVED', 17000, 20, 280, N'Mail flow rule was rejecting external senders; rule scoped correctly.'),
 (13, N'POAPP',N'SERVICE_REQUEST', N'MEDIUM',   N'PO-4471 stuck in approval for a week', N'Purchase order PO-4471 for ergonomic chairs has been pending approval for 7 days.', N'sophie.martin', N'michael.chen', N'RESOLVED', 14500, 100, 1300, N'Approver was on leave; routed to the delegate and approved.'),
 (14, N'INST', N'SERVICE_REQUEST', N'LOW',      N'Install Power BI Desktop', N'Need Power BI Desktop for the monthly KPI report.', N'james.okafor', N'daniel.reyes', N'RESOLVED', 12000, 180, 1100, N'Installed via Software Center; license assigned.'),
 (15, N'ELEC', N'COMPLAINT',       N'MEDIUM',   N'Flickering lights near desk cluster C', N'The overhead lights around desks C10–C14 flicker constantly.', N'nina.rossi', N'lena.fischer', N'RESOLVED', 9000, 50, 700, N'Faulty ballast replaced on two fittings.'),
 (16, N'NJ',   N'SERVICE_REQUEST', N'MEDIUM',   N'Onboarding setup for two sales associates', N'Two new sales associates join on the 25th; please set up accounts and induction.', N'emily.carter', N'sara.ahmed', N'RESOLVED', 7000, 240, 2100, N'Accounts, ID cards and induction slots arranged.'),
 (17, N'WIFI', N'COMPLAINT',       N'HIGH',     N'Weak Wi-Fi in the 3rd floor east wing', N'Video calls keep freezing in the east wing; signal drops to one bar.', N'tom.becker', N'priya.kapoor', N'RESOLVED', 600, 35, 390, N'Access point replaced and channel plan adjusted.'),
-- ===== Resolved late (SLA breached) =====
 (18, N'LIC',  N'COMPLAINT',       N'HIGH',     N'Adobe Acrobat license expired for design team', N'The design team cannot export PDFs — Acrobat says the license expired.', N'ravi.nair', N'daniel.reyes', N'CLOSED', 30000, 50, 720, N'Renewal PO was delayed; temporary licenses issued, then renewed.'),
 (19, N'PLMB', N'COMPLAINT',       N'MEDIUM',   N'Leak under the pantry sink on floor 2', N'Water pooling under the pantry sink; placed a bucket for now.', N'sophie.martin', N'lena.fischer', N'RESOLVED', 11000, 300, 1800, N'Contractor replaced the trap seal. Delay due to parts availability.'),
-- ===== In progress / assigned / pending (live work) =====
 (20, N'LAP',  N'COMPLAINT',       N'CRITICAL', N'CFO laptop not booting before board meeting', N'Laptop shows a blue screen on startup. Board meeting is in 3 hours.', N'arjun.mehta', N'priya.kapoor', N'IN_PROGRESS', 95, 8, NULL, NULL),
 (21, N'MEAL', N'SERVICE_REQUEST', N'LOW',      N'Team dinner reimbursement — Q4 offsite', N'Submitting receipts for the Q4 offsite team dinner (14 attendees).', N'james.okafor', N'michael.chen', N'IN_PROGRESS', 1500, 200, NULL, NULL),
 (22, N'ATT',  N'SERVICE_REQUEST', N'MEDIUM',   N'Attendance shows absent on days I worked remotely', N'Three remote-work days last week are marked as absent.', N'aisha.khan', N'sara.ahmed', N'IN_PROGRESS', 2600, 300, NULL, NULL),
 (23, N'MON',  N'SERVICE_REQUEST', N'LOW',      N'Second monitor and docking station', N'Requesting a second 27" monitor and USB-C dock for my desk.', N'ravi.nair', N'daniel.reyes', N'IN_PROGRESS', 2200, 180, NULL, NULL),
 (24, N'DESK', N'SERVICE_REQUEST', N'MEDIUM',   N'Standing desk request (ergonomic assessment attached)', N'Occupational health recommended a sit-stand desk. Assessment attached.', N'nina.rossi', N'lena.fischer', N'IN_PROGRESS', 1250, 90, NULL, NULL),
 (25, N'VPN',  N'COMPLAINT',       N'HIGH',     N'Cannot reach internal Git server over VPN', N'VPN connects but git.internal times out. Colleagues on the same VPN can reach it.', N'tom.becker', N'priya.kapoor', N'IN_PROGRESS', 380, 30, NULL, NULL),
 (26, N'NVEN', N'SERVICE_REQUEST', N'MEDIUM',   N'Register new vendor: Contoso Catering', N'Please onboard Contoso Catering as an approved vendor for office events.', N'sophie.martin', N'michael.chen', N'PENDING_USER', 2000, 150, NULL, NULL),
 (27, N'SAL',  N'COMPLAINT',       N'MEDIUM',   N'Shift allowance calculated at old rate', N'November shift allowance used last year''s rate.', N'emily.carter', N'sara.ahmed', N'PENDING_USER', 1700, 240, NULL, NULL),
 (28, N'OUT',  N'SERVICE_REQUEST', N'LOW',      N'Calendar delegate access for my assistant', N'Please give my assistant edit access to my Outlook calendar.', N'arjun.mehta', N'daniel.reyes', N'ASSIGNED', 160, NULL, NULL, NULL),
 (29, N'IDC',  N'SERVICE_REQUEST', N'LOW',      N'Replacement ID card — damaged chip', N'My ID card chip is cracked and no longer scans at the turnstile.', N'aisha.khan', N'sara.ahmed', N'ASSIGNED', 200, NULL, NULL, NULL),
 (30, N'AC',   N'COMPLAINT',       N'MEDIUM',   N'Server room temperature alarm', N'Temperature alarm in the comms room went off twice this morning.', N'ravi.nair', N'lena.fischer', N'ASSIGNED', 120, NULL, NULL, NULL),
-- ===== Overdue (breached, still open) =====
 (31, N'INV',  N'COMPLAINT',       N'HIGH',     N'Duplicate payment sent to vendor', N'Fabrikam Logistics received the same payment twice for INV-77120.', N'james.okafor', N'michael.chen', N'IN_PROGRESS', 720, 40, NULL, NULL),
-- ===== Open / unassigned (new queue) =====
 (32, N'TEAMS',N'SERVICE_REQUEST', N'LOW',      N'Create Teams channel for Project Atlas', N'Please create a private channel for Project Atlas with 8 members.', N'tom.becker', NULL, N'OPEN', 45, NULL, NULL, NULL),
 (33, N'PLMB', N'COMPLAINT',       N'HIGH',     N'Washroom on floor 3 out of order', N'Both washrooms on the 3rd floor have no water supply.', N'nina.rossi', NULL, N'OPEN', 25, NULL, NULL, NULL),
 (34, N'POCR', N'SERVICE_REQUEST', N'MEDIUM',   N'Raise PO for annual software subscription', N'Need a PO raised for the analytics platform renewal (USD 18,400).', N'arjun.mehta', NULL, N'OPEN', 150, NULL, NULL, NULL),
 (35, N'ACC',  N'COMPLAINT',       N'MEDIUM',   N'Password reset link not arriving', N'The self-service reset email never arrives, even in junk.', N'emily.carter', NULL, N'OPEN', 300, NULL, NULL, NULL),
-- ===== Rejected =====
 (36, N'INST', N'SERVICE_REQUEST', N'LOW',      N'Install personal music streaming app', N'Could you install a music streaming desktop app on my work laptop?', N'ravi.nair', N'daniel.reyes', N'REJECTED', 8000, 120, NULL, N'Not on the approved software list per IT acceptable-use policy.');

-- Resolve SLA policy (same precedence idea as the app: priority > department/type > default)
DECLARE @PDefault INT = (SELECT Id FROM dbo.SlaPolicy WHERE CompanyId = @CompanyId AND IsDefault = 1);
DECLARE @PCrit INT    = (SELECT Id FROM dbo.SlaPolicy WHERE CompanyId = @CompanyId AND Name = N'Critical Incident SLA');
DECLARE @PHigh INT    = (SELECT Id FROM dbo.SlaPolicy WHERE CompanyId = @CompanyId AND Name = N'High Priority SLA');
DECLARE @PHr INT      = (SELECT Id FROM dbo.SlaPolicy WHERE CompanyId = @CompanyId AND Name = N'HR Service Requests');
DECLARE @PFac INT     = (SELECT Id FROM dbo.SlaPolicy WHERE CompanyId = @CompanyId AND Name = N'Facilities Response');

UPDATE t SET
    CreatedAt = DATEADD(MINUTE, -t.CreatedAgo, @Now),
    PolicyId = CASE
        WHEN t.Priority = N'CRITICAL' THEN @PCrit
        WHEN t.Priority = N'HIGH' THEN @PHigh
        WHEN cs.DKey = N'HR' AND t.TicketType = N'SERVICE_REQUEST' THEN @PHr
        WHEN cs.DKey = N'FAC' THEN @PFac
        ELSE @PDefault END
FROM @T t
JOIN @SubSrc ss ON ss.SKey = t.SKey
JOIN @CatSrc cs ON cs.CKey = ss.CKey;

-- Insert tickets in creation order so ticket numbers increase with time
DECLARE @n INT, @TicketId INT, @Seq BIGINT;
DECLARE tcur CURSOR LOCAL FAST_FORWARD FOR SELECT N FROM @T ORDER BY CreatedAgo DESC;
OPEN tcur;
FETCH NEXT FROM tcur INTO @n;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Seq = NEXT VALUE FOR dbo.Seq_TicketNumber;

    INSERT INTO dbo.Ticket (TicketNumber, CompanyId, CreatedByUserId, DepartmentId, CategoryId, SubCategoryId,
                            TicketType, Subject, Description, Priority, Status, AssignedToUserId, Created)
    SELECT CONCAT(N'RH-', YEAR(t.CreatedAt), N'-', RIGHT(N'000000' + CAST(@Seq AS NVARCHAR(10)), 6)),
           @CompanyId, req.Id, d.Id, c.Id, s.Id,
           t.TicketType, t.Subject, t.Descr, t.Priority, t.Status, ag.Id, t.CreatedAt
    FROM @T t
    JOIN @SubSrc ss ON ss.SKey = t.SKey
    JOIN @CatSrc cs ON cs.CKey = ss.CKey
    JOIN @Dept d ON d.DKey = cs.DKey
    JOIN @Cat c ON c.CKey = cs.CKey
    JOIN @Sub s ON s.SKey = t.SKey
    JOIN @User req ON req.UKey = t.Requester
    LEFT JOIN @User ag ON ag.UKey = t.Agent
    WHERE t.N = @n;

    SET @TicketId = SCOPE_IDENTITY();
    UPDATE @T SET TicketId = @TicketId WHERE N = @n;

    FETCH NEXT FROM tcur INTO @n;
END;
CLOSE tcur;
DEALLOCATE tcur;

/* ----------------------------------------------------------------------------
   7. SLA timers — flags derived from real timestamps so the SLA monitor agrees
   ---------------------------------------------------------------------------- */
INSERT INTO dbo.TicketSla (TicketId, SlaPolicyId, FirstResponseMinutes, ResolutionMinutes,
                           FirstResponseDueAt, FirstRespondedAt, FirstResponseBreached,
                           ResolutionDueAt, ResolvedAt, ResolutionBreached,
                           EscalationDueAt, EscalationTriggeredAt, Created)
SELECT x.TicketId, p.Id, p.FirstResponseMinutes, p.ResolutionMinutes,
       x.FrDue, x.RespondedAt,
       CASE WHEN x.RespondedAt > x.FrDue OR (x.RespondedAt IS NULL AND x.Status NOT IN (N'REJECTED') AND @Now > x.FrDue) THEN 1 ELSE 0 END,
       x.ResDue, x.ResolvedAt,
       CASE WHEN x.ResolvedAt > x.ResDue OR (x.ResolvedAt IS NULL AND x.Status NOT IN (N'REJECTED') AND @Now > x.ResDue) THEN 1 ELSE 0 END,
       x.EscDue,
       CASE WHEN x.EscDue IS NOT NULL AND x.ResolvedAt IS NULL AND x.Status <> N'REJECTED' AND @Now > x.EscDue THEN x.EscDue END,
       x.CreatedAt
FROM (
    SELECT t.TicketId, t.Status, t.CreatedAt, t.PolicyId,
           DATEADD(MINUTE, p.FirstResponseMinutes, t.CreatedAt) AS FrDue,
           DATEADD(MINUTE, p.ResolutionMinutes, t.CreatedAt) AS ResDue,
           CASE WHEN p.EscalationBeforeMinutes > 0 THEN DATEADD(MINUTE, p.ResolutionMinutes - p.EscalationBeforeMinutes, t.CreatedAt) END AS EscDue,
           CASE WHEN t.RespondAfter IS NOT NULL THEN DATEADD(MINUTE, t.RespondAfter, t.CreatedAt) END AS RespondedAt,
           CASE WHEN t.ResolveAfter IS NOT NULL THEN DATEADD(MINUTE, t.ResolveAfter, t.CreatedAt) END AS ResolvedAt
    FROM @T t JOIN dbo.SlaPolicy p ON p.Id = t.PolicyId
) x
JOIN dbo.SlaPolicy p ON p.Id = x.PolicyId;

/* ----------------------------------------------------------------------------
   8. History & comments (mirrors what the stored procedures write)
   ---------------------------------------------------------------------------- */
-- Created
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'CREATED', NULL, N'OPEN', N'Ticket created', t.CreatedAt, req.Id
FROM @T t JOIN @User req ON req.UKey = t.Requester;

-- Assigned (by admin, shortly after creation)
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'ASSIGNED', NULL, ag.FullName + N' (' + ag.UserName + N')', N'Assigned to ' + ag.UserName,
       DATEADD(MINUTE, CASE WHEN t.RespondAfter IS NULL THEN 5 ELSE t.RespondAfter / 2 END, t.CreatedAt), @AdminId
FROM @T t JOIN @User ag ON ag.UKey = t.Agent
WHERE t.Status <> N'REJECTED';

INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'STATUS_CHANGED', N'OPEN', N'ASSIGNED', N'Status changed upon assignment',
       DATEADD(MINUTE, CASE WHEN t.RespondAfter IS NULL THEN 5 ELSE t.RespondAfter / 2 END, t.CreatedAt), @AdminId
FROM @T t WHERE t.Agent IS NOT NULL AND t.Status <> N'REJECTED';

-- Work started + first public reply
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'STATUS_CHANGED', N'ASSIGNED', N'IN_PROGRESS', N'Investigation started', DATEADD(MINUTE, t.RespondAfter, t.CreatedAt), ag.Id
FROM @T t JOIN @User ag ON ag.UKey = t.Agent
WHERE t.RespondAfter IS NOT NULL AND t.Status IN (N'IN_PROGRESS', N'PENDING_USER', N'RESOLVED', N'CLOSED');

INSERT INTO dbo.TicketComment (TicketId, CommentText, Created, CreatedByUserId, IsInternal)
SELECT t.TicketId,
       N'Hi ' + LEFT(req.FullName, CHARINDEX(N' ', req.FullName + N' ') - 1) + N', thanks for raising this. I''ve picked it up and will keep you posted here.',
       DATEADD(MINUTE, t.RespondAfter, t.CreatedAt), ag.Id, 0
FROM @T t JOIN @User ag ON ag.UKey = t.Agent JOIN @User req ON req.UKey = t.Requester
WHERE t.RespondAfter IS NOT NULL AND t.Status <> N'REJECTED';

INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'COMMENT_ADDED', NULL, NULL, N'Public comment added', DATEADD(MINUTE, t.RespondAfter, t.CreatedAt), ag.Id
FROM @T t JOIN @User ag ON ag.UKey = t.Agent
WHERE t.RespondAfter IS NOT NULL AND t.Status <> N'REJECTED';

-- A few internal notes (visible to staff only)
INSERT INTO dbo.TicketComment (TicketId, CommentText, Created, CreatedByUserId, IsInternal)
SELECT t.TicketId, v.Note, DATEADD(MINUTE, t.RespondAfter + 10, t.CreatedAt), ag.Id, 1
FROM @T t JOIN @User ag ON ag.UKey = t.Agent
JOIN (VALUES
    (20, N'Escalated to the hardware vendor; loaner laptop being imaged in parallel.'),
    (25, N'Looks like a split-tunnel route missing for 10.40.0.0/16. Checking with the network team.'),
    (31, N'Bank recall request raised (ref BR-22917). Waiting on vendor confirmation.'),
    (6,  N'Root cause: conditional access policy CA-07 scoped to all users by mistake.')
) v(N, Note) ON v.N = t.N;

INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'COMMENT_ADDED', NULL, NULL, N'Internal comment added', DATEADD(MINUTE, t.RespondAfter + 10, t.CreatedAt), ag.Id
FROM @T t JOIN @User ag ON ag.UKey = t.Agent WHERE t.N IN (20, 25, 31, 6);

-- Waiting on requester
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'STATUS_CHANGED', N'IN_PROGRESS', N'PENDING_USER', N'Waiting for supporting documents from requester',
       DATEADD(MINUTE, t.RespondAfter + 60, t.CreatedAt), ag.Id
FROM @T t JOIN @User ag ON ag.UKey = t.Agent WHERE t.Status = N'PENDING_USER';

-- Resolved (+ resolution note as a public comment)
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'STATUS_CHANGED', N'IN_PROGRESS', N'RESOLVED', t.Note, DATEADD(MINUTE, t.ResolveAfter, t.CreatedAt), ag.Id
FROM @T t JOIN @User ag ON ag.UKey = t.Agent WHERE t.Status IN (N'RESOLVED', N'CLOSED');

INSERT INTO dbo.TicketComment (TicketId, CommentText, Created, CreatedByUserId, IsInternal)
SELECT t.TicketId, t.Note, DATEADD(MINUTE, t.ResolveAfter, t.CreatedAt), ag.Id, 0
FROM @T t JOIN @User ag ON ag.UKey = t.Agent WHERE t.Status IN (N'RESOLVED', N'CLOSED');

-- Closed by requester one day after resolution
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'STATUS_CHANGED', N'RESOLVED', N'CLOSED', N'Confirmed fixed — thank you!', DATEADD(MINUTE, t.ResolveAfter + 1440, t.CreatedAt), req.Id
FROM @T t JOIN @User req ON req.UKey = t.Requester WHERE t.Status = N'CLOSED';

-- Rejected
INSERT INTO dbo.TicketHistory (TicketId, ActionType, OldValue, NewValue, Comments, Created, CreatedByUserId)
SELECT t.TicketId, N'STATUS_CHANGED', N'OPEN', N'REJECTED', t.Note, DATEADD(MINUTE, t.RespondAfter, t.CreatedAt), ag.Id
FROM @T t JOIN @User ag ON ag.UKey = t.Agent WHERE t.Status = N'REJECTED';

-- Ticket LastUpd / ClosedAt / LastUpdBy from the latest history entry
UPDATE tk SET
    LastUpd = h.LastAt,
    LastUpdBy = h.LastBy,
    ClosedAt = CASE WHEN tk.Status IN (N'CLOSED', N'REJECTED') THEN h.LastAt END
FROM dbo.Ticket tk
CROSS APPLY (
    SELECT TOP 1 th.Created AS LastAt, th.CreatedByUserId AS LastBy
    FROM dbo.TicketHistory th WHERE th.TicketId = tk.Id
    ORDER BY th.Created DESC, th.Id DESC
) h
WHERE tk.CompanyId = @CompanyId AND h.LastAt > tk.Created;

/* ----------------------------------------------------------------------------
   9. A handful of recent notifications for the admin bell
   ---------------------------------------------------------------------------- */
DECLARE @Notif TABLE (N INT, EventType NVARCHAR(50), Title NVARCHAR(150), Msg NVARCHAR(500), MinsAgo INT, IsRead BIT);
INSERT INTO @Notif VALUES
 (33, N'TICKET_CREATED',              N'New ticket raised',                 N'Washroom on floor 3 out of order — High priority, Facilities.', 25, 0),
 (20, N'SLA_WARNING',                 N'SLA at risk',                       N'CFO laptop not booting before board meeting is approaching its resolution deadline.', 30, 0),
 (31, N'RESOLUTION_SLA_BREACHED',     N'Resolution SLA breached',           N'Duplicate payment sent to vendor has passed its resolution deadline.', 240, 0),
 (32, N'TICKET_CREATED',              N'New ticket raised',                 N'Create Teams channel for Project Atlas — Low priority, IT Service Desk.', 45, 1),
 (17, N'TICKET_RESOLVED',             N'Ticket resolved',                   N'Weak Wi-Fi in the 3rd floor east wing was resolved by Priya Kapoor.', 210, 1),
 (16, N'TICKET_RESOLVED',             N'Ticket resolved',                   N'Onboarding setup for two sales associates was resolved by Sara Ahmed.', 4900, 1);

INSERT INTO dbo.Notification (CompanyId, EventType, Title, Message, TicketId, ActorUserId, ActionUrl, CorrelationKey, Created, CreatedBy)
SELECT @CompanyId, n.EventType, n.Title, n.Msg, t.TicketId, NULL,
       N'/tickets/' + LOWER(CONVERT(NVARCHAR(36), tk.GId)),
       N'DEMO:' + n.EventType + N':' + CAST(t.TicketId AS NVARCHAR(20)),
       DATEADD(MINUTE, -n.MinsAgo, @Now), @AdminId
FROM @Notif n JOIN @T t ON t.N = n.N JOIN dbo.Ticket tk ON tk.Id = t.TicketId;

INSERT INTO dbo.NotificationRecipient (NotificationId, RecipientUserId, IsRead, ReadAt, Created)
SELECT no.Id, @AdminId, n.IsRead, CASE WHEN n.IsRead = 1 THEN DATEADD(MINUTE, 5, no.Created) END, no.Created
FROM dbo.Notification no
JOIN @T t ON t.TicketId = no.TicketId
JOIN @Notif n ON n.N = t.N AND n.EventType = no.EventType
WHERE no.CompanyId = @CompanyId;

COMMIT TRANSACTION;

/* ----------------------------------------------------------------------------
   Summary
   ---------------------------------------------------------------------------- */
SELECT
    (SELECT COUNT(*) FROM dbo.Department  WHERE CompanyId = @CompanyId) AS Departments,
    (SELECT COUNT(*) FROM dbo.Category    WHERE CompanyId = @CompanyId) AS Categories,
    (SELECT COUNT(*) FROM dbo.SubCategory WHERE CompanyId = @CompanyId) AS SubCategories,
    (SELECT COUNT(*) FROM dbo.UserMaster  WHERE CompanyId = @CompanyId) AS Users,
    (SELECT COUNT(*) FROM dbo.SlaPolicy   WHERE CompanyId = @CompanyId) AS SlaPolicies,
    (SELECT COUNT(*) FROM dbo.Ticket      WHERE CompanyId = @CompanyId) AS Tickets,
    (SELECT COUNT(*) FROM dbo.TicketSla ts JOIN dbo.Ticket t ON t.Id = ts.TicketId
        WHERE t.CompanyId = @CompanyId AND (ts.FirstResponseBreached = 1 OR ts.ResolutionBreached = 1)) AS BreachedTickets;

-- ============================================================================
-- ResolveHub - Seed Master Categories and SubCategories
-- Idempotent seed data for Finance, HR, IT, and Billing
-- ============================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @CompanyId INT = 1;
DECLARE @UserId INT = 1;
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

-- Helper procedure or logic to insert Category safely
-- ----------------------------------------------------------------------------
-- 1. FINANCE DEPARTMENT (DepartmentId = 3)
-- ----------------------------------------------------------------------------
DECLARE @FinanceDeptId INT = (SELECT TOP 1 Id FROM dbo.Department WHERE DepartmentName = 'Finance' AND CompanyId = @CompanyId);
IF @FinanceDeptId IS NOT NULL
BEGIN
    -- Category 1.1: Payroll & Compensation
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @FinanceDeptId AND CategoryName = 'Payroll & Compensation' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Payroll & Compensation', 'Salary, bonuses, tax deductions, and payroll queries', @FinanceDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END

    -- Category 1.2: Expense Reimbursements
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @FinanceDeptId AND CategoryName = 'Expense Reimbursements' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Expense Reimbursements', 'Travel claims, client dinners, and employee expense reports', @FinanceDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END

    -- Category 1.3: Invoices & Vendor Payments
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @FinanceDeptId AND CategoryName = 'Invoices & Vendor Payments' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Invoices & Vendor Payments', 'Vendor invoices, client billing, and payments processing', @FinanceDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END
END;

-- ----------------------------------------------------------------------------
-- 2. HR DEPARTMENT (DepartmentId = 2)
-- ----------------------------------------------------------------------------
DECLARE @HrDeptId INT = (SELECT TOP 1 Id FROM dbo.Department WHERE DepartmentName = 'HR' AND CompanyId = @CompanyId);
IF @HrDeptId IS NOT NULL
BEGIN
    -- Category 2.1: Leave & Attendance
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @HrDeptId AND CategoryName = 'Leave & Attendance' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Leave & Attendance', 'Leave balances, swipe regularizations, and holidays', @HrDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END

    -- Category 2.2: Benefits & Medical Insurance
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @HrDeptId AND CategoryName = 'Benefits & Medical Insurance' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Benefits & Medical Insurance', 'Corporate health insurance, claims, and provident fund', @HrDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END

    -- Category 2.3: Employee Onboarding & Offboarding
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @HrDeptId AND CategoryName = 'Onboarding & Offboarding' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Onboarding & Offboarding', 'New joiner documentation, access badges, and exit formalities', @HrDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END
END;

-- ----------------------------------------------------------------------------
-- 3. INFORMATION TECHNOLOGY DEPARTMENT (DepartmentId = 1)
-- ----------------------------------------------------------------------------
DECLARE @ItDeptId INT = (SELECT TOP 1 Id FROM dbo.Department WHERE DepartmentName = 'Information Technology' AND CompanyId = @CompanyId);
IF @ItDeptId IS NOT NULL
BEGIN
    -- Category 3.1: Software & Applications
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @ItDeptId AND CategoryName = 'Software & Applications' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Software & Applications', 'Operating systems, productivity apps, IDEs, and VPN', @ItDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END

    -- Category 3.2: Network & Access
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @ItDeptId AND CategoryName = 'Network & Access' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Network & Access', 'Wi-Fi connectivity, domain access, and firewall permissions', @ItDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END
END;

-- ----------------------------------------------------------------------------
-- 4. BILLING DEPARTMENT (DepartmentId = 8)
-- ----------------------------------------------------------------------------
DECLARE @BillingDeptId INT = (SELECT TOP 1 Id FROM dbo.Department WHERE DepartmentName = 'Billing' AND CompanyId = @CompanyId);
IF @BillingDeptId IS NOT NULL
BEGIN
    -- Category 4.1: Billing Inquiries & Disputes
    IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE DepartmentId = @BillingDeptId AND CategoryName = 'Billing Inquiries & Disputes' AND CompanyId = @CompanyId)
    BEGIN
        INSERT INTO dbo.Category (GId, CategoryName, Description, DepartmentId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Billing Inquiries & Disputes', 'Customer billing queries, charge disputes, and receipts', @BillingDeptId, @CompanyId, @Now, @UserId, 1, 0);
    END
END;

-- ============================================================================
-- SEED SUBCATEGORIES
-- ============================================================================

-- SubCategories for Payroll & Compensation
DECLARE @CatPayroll INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Payroll & Compensation' AND CompanyId = @CompanyId);
IF @CatPayroll IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatPayroll AND SubCategoryName = 'Salary Discrepancy' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Salary Discrepancy', 'Monthly pay calculation or bonus mismatch', @CatPayroll, @CompanyId, @Now, @UserId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatPayroll AND SubCategoryName = 'Tax Deductions (TDS)' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Tax Deductions (TDS)', 'Tax withholdings and Form 16 queries', @CatPayroll, @CompanyId, @Now, @UserId, 1, 0);
END;

-- SubCategories for Expense Reimbursements
DECLARE @CatExpense INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Expense Reimbursements' AND CompanyId = @CompanyId);
IF @CatExpense IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatExpense AND SubCategoryName = 'Travel & Conveyance Claim' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Travel & Conveyance Claim', 'Flight, train, taxi or fuel expense reimbursement', @CatExpense, @CompanyId, @Now, @UserId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatExpense AND SubCategoryName = 'Meal & Incidentals' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Meal & Incidentals', 'Business lunch or client entertainment claims', @CatExpense, @CompanyId, @Now, @UserId, 1, 0);
END;

-- SubCategories for Invoices & Vendor Payments
DECLARE @CatInvoices INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Invoices & Vendor Payments' AND CompanyId = @CompanyId);
IF @CatInvoices IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatInvoices AND SubCategoryName = 'Vendor Invoice Processing' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Vendor Invoice Processing', 'Submission and payment status of vendor invoices', @CatInvoices, @CompanyId, @Now, @UserId, 1, 0);
END;

-- SubCategories for Leave & Attendance
DECLARE @CatLeave INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Leave & Attendance' AND CompanyId = @CompanyId);
IF @CatLeave IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatLeave AND SubCategoryName = 'Leave Balance Correction' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Leave Balance Correction', 'Earned, sick, or casual leave adjustment', @CatLeave, @CompanyId, @Now, @UserId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatLeave AND SubCategoryName = 'Swipe / Biometric Regularization' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Swipe / Biometric Regularization', 'Missing punch in or punch out regularizations', @CatLeave, @CompanyId, @Now, @UserId, 1, 0);
END;

-- SubCategories for Benefits & Medical Insurance
DECLARE @CatBenefits INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Benefits & Medical Insurance' AND CompanyId = @CompanyId);
IF @CatBenefits IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatBenefits AND SubCategoryName = 'Mediclaim E-Card Issue' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Mediclaim E-Card Issue', 'Health insurance e-card or dependent addition', @CatBenefits, @CompanyId, @Now, @UserId, 1, 0);
END;

-- SubCategories for Software & Applications
DECLARE @CatSoftware INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Software & Applications' AND CompanyId = @CompanyId);
IF @CatSoftware IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatSoftware AND SubCategoryName = 'VPN & Remote Access' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'VPN & Remote Access', 'Cisco AnyConnect or GlobalProtect VPN issues', @CatSoftware, @CompanyId, @Now, @UserId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatSoftware AND SubCategoryName = 'Developer Tools & IDEs' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Developer Tools & IDEs', 'Visual Studio, VS Code, Git, or Docker licenses', @CatSoftware, @CompanyId, @Now, @UserId, 1, 0);
END;

-- SubCategories for Network & Access
DECLARE @CatNetwork INT = (SELECT TOP 1 Id FROM dbo.Category WHERE CategoryName = 'Network & Access' AND CompanyId = @CompanyId);
IF @CatNetwork IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SubCategory WHERE CategoryId = @CatNetwork AND SubCategoryName = 'Office Wi-Fi Connection' AND CompanyId = @CompanyId)
        INSERT INTO dbo.SubCategory (GId, SubCategoryName, Description, CategoryId, CompanyId, Created, CreatedBy, IsActive, IsDeleted)
        VALUES (NEWID(), 'Office Wi-Fi Connection', 'Slow speed or authentication failure on office Wi-Fi', @CatNetwork, @CompanyId, @Now, @UserId, 1, 0);
END;

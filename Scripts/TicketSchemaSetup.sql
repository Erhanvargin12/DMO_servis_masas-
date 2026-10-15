-- =============================================
-- BackendCore - ticket şeması ve ek tablolar kurulum scripti
-- =============================================

USE CoreDev;
GO

-- 1. Create Schema ticket if not exists
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ticket')
BEGIN
    EXEC('CREATE SCHEMA [ticket]');
END
GO

-- 2. Users table columns check
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EbysUserId')
BEGIN
    ALTER TABLE Users ADD EbysUserId NVARCHAR(100) NULL;
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'BranchId')
BEGIN
    ALTER TABLE Users ADD BranchId NVARCHAR(100) NULL;
END
GO

-- 3. Branches table (dbo.Branches)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Branches' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Branches (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DepartmentId NVARCHAR(100) NOT NULL,
        ParentDepartmentId NVARCHAR(100) NULL,
        Name NVARCHAR(250) NOT NULL,
        Description NVARCHAR(500) NULL
    );
END
GO

-- 4. Companies table (dbo.Companies)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Companies' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Companies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyName NVARCHAR(250) NOT NULL,
        TaxNumber NVARCHAR(50) NOT NULL,
        Mail NVARCHAR(150) NOT NULL,
        PasswordHash VARBINARY(MAX) NULL,
        PasswordSalt VARBINARY(MAX) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        AuthorizedPerson NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(50) NOT NULL
    );
END
GO

-- 5. AuditLogs table (dbo.AuditLogs)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditLogs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Application NVARCHAR(150) NULL,
        TableName NVARCHAR(150) NULL,
        CreatedUserId NVARCHAR(100) NULL,
        CreatedSurrogateId NVARCHAR(100) NULL,
        Operation NVARCHAR(100) NULL,
        KeyValues NVARCHAR(MAX) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        TraceIdentifier NVARCHAR(250) NULL,
        RemoteIpAddress NVARCHAR(100) NULL,
        RecordDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 6. ticket.Categories
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(250) NOT NULL,
        Description NVARCHAR(500) NULL
    );
END
GO

-- 7. ticket.CategoryBranches
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CategoryBranches' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.CategoryBranches (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId INT NOT NULL,
        BranchId INT NOT NULL,
        DisplayOrder INT NOT NULL DEFAULT 0
    );
END
GO

-- 8. ticket.Tickets
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Tickets' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.Tickets (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TicketNo NVARCHAR(100) NOT NULL,
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        CreatedById INT NOT NULL,
        CurrentBranchId INT NOT NULL,
        CategoryId INT NULL,
        AssignedUserId INT NULL,
        StatusId INT NULL,
        PriorityId INT NOT NULL DEFAULT 2,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        ResolvedAt DATETIME NULL,
        ClosedAt DATETIME NULL
    );
END
GO

-- 9. ticket.Assignments
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Assignments' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.Assignments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TicketId INT NOT NULL,
        AssignedById INT NOT NULL,
        AssignedToId INT NOT NULL,
        AssignedAt DATETIME NOT NULL DEFAULT GETDATE(),
        Description NVARCHAR(MAX) NULL
    );
END
GO

-- 10. ticket.Attachments
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Attachments' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.Attachments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TicketId INT NOT NULL,
        ResponseId INT NOT NULL,
        FileName NVARCHAR(500) NOT NULL,
        FilePath NVARCHAR(1000) NOT NULL,
        UploadedById INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 11. ticket.Responses
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Responses' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.Responses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SenderId INT NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        IsInternal BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 12. ticket.Transfers
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Transfers' AND schema_id = SCHEMA_ID('ticket'))
BEGIN
    CREATE TABLE ticket.Transfers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TicketId INT NOT NULL,
        FromBranchId INT NOT NULL,
        ToBranchId INT NOT NULL,
        TransferredById INT NOT NULL,
        TransferredAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

PRINT 'ticket şeması ve tüm tablolar başarıyla oluşturuldu.';
GO

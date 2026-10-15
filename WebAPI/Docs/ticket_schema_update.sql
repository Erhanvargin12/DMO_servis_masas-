-- =====================================================================
-- Ticket Modülü — Yeni Tablolar ve Kolon Eklemeleri
-- =====================================================================

-- 1. Ticket tablosuna yeni kolonlar
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ticket.Tickets') AND name = 'CategoryId')
BEGIN
    ALTER TABLE ticket.Tickets ADD CategoryId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ticket.Tickets') AND name = 'ResolvedAt')
BEGIN
    ALTER TABLE ticket.Tickets ADD ResolvedAt DATETIME2 NULL;
END
GO

-- 2. Kategori tablosu
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('ticket') AND name = 'Categories')
BEGIN
    CREATE TABLE ticket.Categories
    (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(200)  NOT NULL,
        Description NVARCHAR(500)  NULL
    );
END
GO

-- 3. Kategori ↔ Şube ilişki tablosu (çok-a-çok)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('ticket') AND name = 'CategoryBranches')
BEGIN
    CREATE TABLE ticket.CategoryBranches
    (
        Id         INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId INT NOT NULL,
        BranchId   INT NOT NULL,

        CONSTRAINT FK_CategoryBranches_Category FOREIGN KEY (CategoryId)
            REFERENCES ticket.Categories(Id) ON DELETE CASCADE,

        CONSTRAINT FK_CategoryBranches_Branch FOREIGN KEY (BranchId)
            REFERENCES dbo.Branches(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ticket.CategoryBranches') AND name = 'DisplayOrder')
BEGIN
    ALTER TABLE ticket.CategoryBranches ADD DisplayOrder INT NOT NULL CONSTRAINT DF_CategoryBranches_DisplayOrder DEFAULT (1);
END
GO

-- 4. Ticket → Category FK (opsiyonel)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Tickets_Category')
BEGIN
    ALTER TABLE ticket.Tickets
        ADD CONSTRAINT FK_Tickets_Category
        FOREIGN KEY (CategoryId) REFERENCES ticket.Categories(Id);
END
GO

PRINT 'Ticket schema güncellemesi tamamlandı.';

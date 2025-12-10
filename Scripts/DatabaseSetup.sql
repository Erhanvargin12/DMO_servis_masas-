-- =============================================
-- BackendCore - Veritabanı Kurulum Scripti
-- Versiyon: 1.0
-- Tarih: 2025
-- =============================================
-- Bu script veritabanını sıfırdan oluşturur ve
-- test verileri ile doldurur.
-- 
-- Connection String (appsettings.Development.json):
-- Server=(localdb)\mssqllocaldb; Database=CoreDev; 
-- Trusted_Connection=True; TrustServerCertificate=true;
-- =============================================

USE master;
GO

-- =============================================
-- 1. VERİTABANINI SİL VE YENİDEN OLUŞTUR
-- =============================================
PRINT '=== Veritabanı oluşturuluyor... ===';

-- Mevcut bağlantıları kapat
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'CoreDev')
BEGIN
    ALTER DATABASE CoreDev SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CoreDev;
    PRINT 'Mevcut veritabanı silindi.';
END
GO

-- Yeni veritabanı oluştur
CREATE DATABASE CoreDev;
GO

USE CoreDev;
GO

PRINT 'Veritabanı oluşturuldu.';

-- =============================================
-- 2. TABLOLARI OLUŞTUR
-- =============================================
PRINT '=== Tablolar oluşturuluyor... ===';

-- Users Tablosu
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Mail NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARBINARY(500) NOT NULL,
    PasswordSalt VARBINARY(500) NOT NULL,
    Status BIT NOT NULL DEFAULT 1,
    CreateAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdateAt DATETIME NULL,
    RegistirationId INT NOT NULL,
    OrganizationUnitId INT NOT NULL,
    PositionId INT NOT NULL
);
PRINT 'Users tablosu oluşturuldu.';

-- OperationClaims (Roller) Tablosu
CREATE TABLE OperationClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);
PRINT 'OperationClaims tablosu oluşturuldu.';

-- UserOperationClaims (Kullanıcı-Rol İlişkisi) Tablosu
CREATE TABLE UserOperationClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OperationClaimId INT NOT NULL,
    CONSTRAINT FK_UserOperationClaims_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserOperationClaims_OperationClaim FOREIGN KEY (OperationClaimId) REFERENCES OperationClaims(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserOperationClaims UNIQUE (UserId, OperationClaimId)
);
PRINT 'UserOperationClaims tablosu oluşturuldu.';

-- Menus Tablosu
CREATE TABLE Menus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    Icon NVARCHAR(50) NULL,
    Url NVARCHAR(255) NULL,
    ParentId INT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreateAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdateAt DATETIME NULL,
    CONSTRAINT FK_Menus_Parent FOREIGN KEY (ParentId) REFERENCES Menus(Id)
);
PRINT 'Menus tablosu oluşturuldu.';

-- MenuOperationClaims (Menü-Rol İlişkisi) Tablosu
CREATE TABLE MenuOperationClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MenuId INT NOT NULL,
    OperationClaimId INT NOT NULL,
    CONSTRAINT FK_MenuOperationClaims_Menu FOREIGN KEY (MenuId) REFERENCES Menus(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MenuOperationClaims_OperationClaim FOREIGN KEY (OperationClaimId) REFERENCES OperationClaims(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuOperationClaims UNIQUE (MenuId, OperationClaimId)
);
PRINT 'MenuOperationClaims tablosu oluşturuldu.';

-- RefreshTokens Tablosu
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    Expires DATETIME NOT NULL,
    Created DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CreatedByIp NVARCHAR(50) NULL,
    Revoked DATETIME NULL,
    RevokedByIp NVARCHAR(50) NULL,
    ReplacedByToken NVARCHAR(500) NULL,
    ReasonRevoked NVARCHAR(255) NULL,
    CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
PRINT 'RefreshTokens tablosu oluşturuldu.';

-- =============================================
-- 3. İNDEKSLER
-- =============================================
PRINT '=== İndeksler oluşturuluyor... ===';

CREATE INDEX IX_Users_Mail ON Users(Mail);
CREATE INDEX IX_Users_Status ON Users(Status);
CREATE INDEX IX_UserOperationClaims_UserId ON UserOperationClaims(UserId);
CREATE INDEX IX_UserOperationClaims_OperationClaimId ON UserOperationClaims(OperationClaimId);
CREATE INDEX IX_Menus_ParentId ON Menus(ParentId);
CREATE INDEX IX_Menus_IsActive ON Menus(IsActive);
CREATE INDEX IX_MenuOperationClaims_MenuId ON MenuOperationClaims(MenuId);
CREATE INDEX IX_MenuOperationClaims_OperationClaimId ON MenuOperationClaims(OperationClaimId);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);

PRINT 'İndeksler oluşturuldu.';

-- =============================================
-- 4. ROLLER
-- =============================================
PRINT '=== Roller ekleniyor... ===';

INSERT INTO OperationClaims (Name, Description) VALUES 
('Admin', 'Sistem Yöneticisi - Tüm yetkilere sahip'),
('Moderator', 'Moderatör - Kullanıcı ve içerik yönetimi'),
('Editor', 'Editör - İçerik düzenleme yetkisi'),
('Viewer', 'Görüntüleyici - Sadece okuma yetkisi'),
('User', 'Standart Kullanıcı - Temel erişim');

PRINT '5 rol eklendi.';

-- =============================================
-- 5. KULLANICILAR
-- =============================================
PRINT '=== Kullanıcılar ekleniyor... ===';

-- Not: Şifre "123456" için HMACSHA512 hash değerleri
-- Her kullanıcı için farklı salt kullanılıyor

-- Admin Kullanıcı (Şifre: 123456)
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Admin', 'Sistem', 'admin@backendcore.com', 
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'admin123456salt1')),
    CONVERT(VARBINARY(500), 'salt1'),
    1, GETDATE(), 1001, 1, 1);

-- Ahmet Yılmaz - Moderator
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Ahmet', 'Yılmaz', 'ahmet.yilmaz@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'ahmet123456salt2')),
    CONVERT(VARBINARY(500), 'salt2'),
    1, GETDATE(), 1002, 1, 2);

-- Mehmet Kaya - Editor
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Mehmet', 'Kaya', 'mehmet.kaya@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'mehmet123456salt3')),
    CONVERT(VARBINARY(500), 'salt3'),
    1, GETDATE(), 1003, 2, 3);

-- Ayşe Demir - Editor
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Ayşe', 'Demir', 'ayse.demir@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'ayse123456salt4')),
    CONVERT(VARBINARY(500), 'salt4'),
    1, GETDATE(), 1004, 2, 2);

-- Fatma Şahin - Viewer
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Fatma', 'Şahin', 'fatma.sahin@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'fatma123456salt5')),
    CONVERT(VARBINARY(500), 'salt5'),
    1, GETDATE(), 1005, 3, 4);

-- Ali Öztürk - User (Pasif)
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Ali', 'Öztürk', 'ali.ozturk@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'ali123456salt6')),
    CONVERT(VARBINARY(500), 'salt6'),
    0, GETDATE(), 1006, 1, 5);

-- Zeynep Arslan - User
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Zeynep', 'Arslan', 'zeynep.arslan@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'zeynep123456salt7')),
    CONVERT(VARBINARY(500), 'salt7'),
    1, GETDATE(), 1007, 3, 2);

-- Mustafa Çelik - Moderator + Editor
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Mustafa', 'Çelik', 'mustafa.celik@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'mustafa123456salt8')),
    CONVERT(VARBINARY(500), 'salt8'),
    1, GETDATE(), 1008, 2, 3);

-- Elif Yıldız - Viewer
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Elif', 'Yıldız', 'elif.yildiz@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'elif123456salt9')),
    CONVERT(VARBINARY(500), 'salt9'),
    1, GETDATE(), 1009, 1, 4);

-- Emre Koç - Admin
INSERT INTO Users (FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Emre', 'Koç', 'emre.koc@backendcore.com',
    CONVERT(VARBINARY(500), HASHBYTES('SHA2_512', 'emre123456salt10')),
    CONVERT(VARBINARY(500), 'salt10'),
    1, GETDATE(), 1010, 3, 1);

PRINT '10 kullanıcı eklendi.';

-- =============================================
-- 6. KULLANICI-ROL İLİŞKİLERİ
-- =============================================
PRINT '=== Kullanıcı-Rol ilişkileri ekleniyor... ===';

INSERT INTO UserOperationClaims (UserId, OperationClaimId) VALUES 
(1, 1),   -- Admin -> Admin rolü
(2, 2),   -- Ahmet -> Moderator
(3, 3),   -- Mehmet -> Editor
(4, 3),   -- Ayşe -> Editor
(5, 4),   -- Fatma -> Viewer
(6, 5),   -- Ali -> User (pasif)
(7, 5),   -- Zeynep -> User
(8, 2),   -- Mustafa -> Moderator
(8, 3),   -- Mustafa -> Editor (çoklu rol)
(9, 4),   -- Elif -> Viewer
(10, 1);  -- Emre -> Admin

PRINT 'Kullanıcı-Rol ilişkileri eklendi.';

-- =============================================
-- 7. MENÜLER
-- =============================================
PRINT '=== Menüler ekleniyor... ===';

-- Ana Menü Grupları (ParentId = NULL)
INSERT INTO Menus (Name, Description, Icon, Url, ParentId, DisplayOrder, IsActive, CreateAt) VALUES 
('Genel', 'Genel', NULL, NULL, NULL, 1, 1, GETDATE()),
('Yönetim', 'Yönetim', NULL, NULL, NULL, 2, 1, GETDATE()),
('Raporlar', 'Raporlar', NULL, NULL, NULL, 3, 1, GETDATE()),
('Sistem', 'Sistem', NULL, NULL, NULL, 4, 1, GETDATE());

-- Genel Alt Menüler (ParentId = 1)
INSERT INTO Menus (Name, Description, Icon, Url, ParentId, DisplayOrder, IsActive, CreateAt) VALUES 
('Dashboard', NULL, 'dashboard', '/', 1, 1, 1, GETDATE()),
('Profil', NULL, 'users', '/Profile', 1, 2, 1, GETDATE());

-- Yönetim Alt Menüler (ParentId = 2)
INSERT INTO Menus (Name, Description, Icon, Url, ParentId, DisplayOrder, IsActive, CreateAt) VALUES 
('Kullanıcılar', NULL, 'users', '/Users', 2, 1, 1, GETDATE()),
('Roller', NULL, 'roles', '/Roles', 2, 2, 1, GETDATE()),
('Menüler', NULL, 'menu', '/Menus', 2, 3, 1, GETDATE()),
('Organizasyonlar', NULL, 'organization', '/Organizations', 2, 4, 1, GETDATE());

-- Raporlar Alt Menüler (ParentId = 3)
INSERT INTO Menus (Name, Description, Icon, Url, ParentId, DisplayOrder, IsActive, CreateAt) VALUES 
('Kullanıcı Raporu', NULL, 'logs', '/Reports/Users', 3, 1, 1, GETDATE()),
('Aktivite Raporu', NULL, 'logs', '/Reports/Activity', 3, 2, 1, GETDATE()),
('Sistem Raporu', NULL, 'logs', '/Reports/System', 3, 3, 1, GETDATE());

-- Sistem Alt Menüler (ParentId = 4)
INSERT INTO Menus (Name, Description, Icon, Url, ParentId, DisplayOrder, IsActive, CreateAt) VALUES 
('Ayarlar', NULL, 'settings', '/Settings', 4, 1, 1, GETDATE()),
('Loglar', NULL, 'logs', '/Logs', 4, 2, 1, GETDATE()),
('Yedekleme', NULL, 'folder', '/Backup', 4, 3, 1, GETDATE());

PRINT '16 menü eklendi.';

-- =============================================
-- 8. MENÜ-ROL İLİŞKİLERİ
-- =============================================
PRINT '=== Menü-Rol ilişkileri ekleniyor... ===';

-- Admin (RolId=1) - Tüm alt menülere erişim
INSERT INTO MenuOperationClaims (MenuId, OperationClaimId) VALUES 
(5, 1), (6, 1), (7, 1), (8, 1), (9, 1), (10, 1), 
(11, 1), (12, 1), (13, 1), (14, 1), (15, 1), (16, 1);

-- Moderator (RolId=2) - Yönetim + Raporlar (Sistem hariç)
INSERT INTO MenuOperationClaims (MenuId, OperationClaimId) VALUES 
(5, 2), (6, 2), (7, 2), (8, 2), (10, 2), (11, 2), (12, 2);

-- Editor (RolId=3) - Temel + Raporlar
INSERT INTO MenuOperationClaims (MenuId, OperationClaimId) VALUES 
(5, 3), (6, 3), (11, 3), (12, 3);

-- Viewer (RolId=4) - Temel + Kullanıcı Raporu
INSERT INTO MenuOperationClaims (MenuId, OperationClaimId) VALUES 
(5, 4), (6, 4), (11, 4);

-- User (RolId=5) - Sadece Dashboard ve Profil
INSERT INTO MenuOperationClaims (MenuId, OperationClaimId) VALUES 
(5, 5), (6, 5);

PRINT 'Menü-Rol ilişkileri eklendi.';

-- =============================================
-- 9. ÖZET RAPOR
-- =============================================
PRINT '';
PRINT '╔══════════════════════════════════════════════════════════════╗';
PRINT '║           BACKENDCORE VERİTABANI KURULUMU TAMAMLANDI         ║';
PRINT '╚══════════════════════════════════════════════════════════════╝';
PRINT '';

SELECT 'TABLO ÖZETİ' AS [Rapor];
SELECT '─────────────────────────────────' AS [─];

SELECT 
    'Users' AS Tablo, 
    COUNT(*) AS [Kayıt Sayısı],
    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS [Aktif],
    SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) AS [Pasif]
FROM Users
UNION ALL
SELECT 'OperationClaims', COUNT(*), COUNT(*), 0 FROM OperationClaims
UNION ALL
SELECT 'UserOperationClaims', COUNT(*), COUNT(*), 0 FROM UserOperationClaims
UNION ALL
SELECT 'Menus', COUNT(*), SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END), SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) FROM Menus
UNION ALL
SELECT 'MenuOperationClaims', COUNT(*), COUNT(*), 0 FROM MenuOperationClaims;

PRINT '';
SELECT 'KULLANICI LİSTESİ' AS [Rapor];
SELECT '─────────────────────────────────' AS [─];

SELECT 
    u.Id,
    u.FirstName + ' ' + u.LastName AS [Ad Soyad],
    u.Mail AS [E-posta],
    CASE WHEN u.Status = 1 THEN 'Aktif' ELSE 'Pasif' END AS [Durum],
    STRING_AGG(oc.Name, ', ') AS [Roller]
FROM Users u
LEFT JOIN UserOperationClaims uoc ON u.Id = uoc.UserId
LEFT JOIN OperationClaims oc ON uoc.OperationClaimId = oc.Id
GROUP BY u.Id, u.FirstName, u.LastName, u.Mail, u.Status
ORDER BY u.Id;

PRINT '';
SELECT 'MENÜ YAPISI' AS [Rapor];
SELECT '─────────────────────────────────' AS [─];

SELECT 
    CASE WHEN m.ParentId IS NULL THEN m.Name ELSE '  └─ ' + m.Name END AS [Menü],
    ISNULL(m.Url, '-') AS [URL],
    ISNULL(m.Icon, '-') AS [İkon],
    CASE WHEN m.IsActive = 1 THEN '✓' ELSE '✗' END AS [Aktif]
FROM Menus m
ORDER BY ISNULL(m.ParentId, m.Id), m.ParentId, m.DisplayOrder;

PRINT '';
PRINT '╔══════════════════════════════════════════════════════════════╗';
PRINT '║                        ÖNEMLİ NOT                            ║';
PRINT '╠══════════════════════════════════════════════════════════════╣';
PRINT '║  SQL ile oluşturulan şifreler C# HMACSHA512 ile uyumlu       ║';
PRINT '║  DEĞİLDİR! Gerçek kullanıcılar için:                         ║';
PRINT '║                                                              ║';
PRINT '║  1. Swagger: https://localhost:5001/swagger                  ║';
PRINT '║  2. POST /api/auth/register endpoint kullanın                ║';
PRINT '║  3. Sonra UserOperationClaim ile rol atayın                  ║';
PRINT '║                                                              ║';
PRINT '║  Örnek Register JSON:                                        ║';
PRINT '║  {                                                           ║';
PRINT '║    "firstName": "Test",                                      ║';
PRINT '║    "lastName": "User",                                       ║';
PRINT '║    "mail": "test@test.com",                                  ║';
PRINT '║    "password": "123456",                                     ║';
PRINT '║    "registirationId": 9999,                                  ║';
PRINT '║    "branchId": 1,                                            ║';
PRINT '║    "positionId": 1                                           ║';
PRINT '║  }                                                           ║';
PRINT '╚══════════════════════════════════════════════════════════════╝';
GO


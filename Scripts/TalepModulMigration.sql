-- =============================================
-- BackendCore — Talep Yönetim Modülü Migration
-- Versiyon: 1.0
-- Tarih: 2026-02-25
-- =============================================
-- Mevcut bir veritabanına (CoreDev) yeni Talep
-- modülü tablolarını ekler.
-- =============================================

USE CoreDev;
GO

PRINT '=== Talep Modülü tabloları oluşturuluyor... ===';

-- ── 1. TalepKategoriler ──────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TalepKategoriler')
BEGIN
    CREATE TABLE TalepKategoriler (
        Id              INT           IDENTITY(1,1) PRIMARY KEY,
        Ad              NVARCHAR(100) NOT NULL,
        Aciklama        NVARCHAR(500) NULL,
        AktifMi         BIT           NOT NULL DEFAULT 1,
        OlusturmaTarihi DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'TalepKategoriler tablosu oluşturuldu.';
END
ELSE
    PRINT 'TalepKategoriler zaten mevcut, atlandı.';
GO

-- ── 2. Talepler ──────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Talepler')
BEGIN
    CREATE TABLE Talepler (
        Id                    INT            IDENTITY(1,1) PRIMARY KEY,
        Baslik                NVARCHAR(250)  NOT NULL,
        Aciklama              NVARCHAR(MAX)  NOT NULL DEFAULT '',
        KategoriId            INT            NOT NULL,
        Oncelik               INT            NOT NULL DEFAULT 2,   -- TalepOncelik: Normal=2
        Durum                 INT            NOT NULL DEFAULT 1,   -- TalepDurum: Acik=1
        OlusturanKullaniciId  INT            NOT NULL,
        AtananKullaniciId     INT            NULL,
        OlusturmaTarihi       DATETIME       NOT NULL DEFAULT GETDATE(),
        GuncellenmeTarihi     DATETIME       NULL,
        KapanmaTarihi         DATETIME       NULL,

        CONSTRAINT FK_Talepler_Kategori
            FOREIGN KEY (KategoriId) REFERENCES TalepKategoriler(Id),
        CONSTRAINT FK_Talepler_OlusturanKullanici
            FOREIGN KEY (OlusturanKullaniciId) REFERENCES Users(Id),
        CONSTRAINT FK_Talepler_AtananKullanici
            FOREIGN KEY (AtananKullaniciId) REFERENCES Users(Id)
    );
    PRINT 'Talepler tablosu oluşturuldu.';
END
ELSE
    PRINT 'Talepler zaten mevcut, atlandı.';
GO

-- ── 3. TalepYanitlar ─────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TalepYanitlar')
BEGIN
    CREATE TABLE TalepYanitlar (
        Id              INT           IDENTITY(1,1) PRIMARY KEY,
        TalepId         INT           NOT NULL,
        KullaniciId     INT           NOT NULL,
        Icerik          NVARCHAR(MAX) NOT NULL,
        OlusturmaTarihi DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TalepYanitlar_Talep
            FOREIGN KEY (TalepId) REFERENCES Talepler(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TalepYanitlar_Kullanici
            FOREIGN KEY (KullaniciId) REFERENCES Users(Id)
    );
    PRINT 'TalepYanitlar tablosu oluşturuldu.';
END
ELSE
    PRINT 'TalepYanitlar zaten mevcut, atlandı.';
GO

-- ── 4. TalepChatler ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TalepChatler')
BEGIN
    CREATE TABLE TalepChatler (
        Id                   INT           IDENTITY(1,1) PRIMARY KEY,
        TalepId              INT           NOT NULL,
        GonderenKullaniciId  INT           NOT NULL,
        Mesaj                NVARCHAR(MAX) NOT NULL,
        OlusturmaTarihi      DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TalepChatler_Talep
            FOREIGN KEY (TalepId) REFERENCES Talepler(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TalepChatler_GonderenKullanici
            FOREIGN KEY (GonderenKullaniciId) REFERENCES Users(Id)
    );
    PRINT 'TalepChatler tablosu oluşturuldu.';
END
ELSE
    PRINT 'TalepChatler zaten mevcut, atlandı.';
GO

-- ── 5. TalepAtamalar ─────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TalepAtamalar')
BEGIN
    CREATE TABLE TalepAtamalar (
        Id                  INT            IDENTITY(1,1) PRIMARY KEY,
        TalepId             INT            NOT NULL,
        AtayanKullaniciId   INT            NOT NULL,
        AtananKullaniciId   INT            NOT NULL,
        Aciklama            NVARCHAR(500)  NULL,
        AtamaTarihi         DATETIME       NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TalepAtamalar_Talep
            FOREIGN KEY (TalepId) REFERENCES Talepler(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TalepAtamalar_AtayanKullanici
            FOREIGN KEY (AtayanKullaniciId) REFERENCES Users(Id),
        CONSTRAINT FK_TalepAtamalar_AtananKullanici
            FOREIGN KEY (AtananKullaniciId) REFERENCES Users(Id)
    );
    PRINT 'TalepAtamalar tablosu oluşturuldu.';
END
ELSE
    PRINT 'TalepAtamalar zaten mevcut, atlandı.';
GO

-- ── 6. TalepSevkler ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TalepSevkler')
BEGIN
    CREATE TABLE TalepSevkler (
        Id                   INT           IDENTITY(1,1) PRIMARY KEY,
        TalepId              INT           NOT NULL,
        SevkEdenKullaniciId  INT           NOT NULL,
        HedefKullaniciId     INT           NULL,
        HedefRolId           INT           NULL,
        Aciklama             NVARCHAR(500) NULL,
        SevkTarihi           DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TalepSevkler_Talep
            FOREIGN KEY (TalepId) REFERENCES Talepler(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TalepSevkler_SevkEdenKullanici
            FOREIGN KEY (SevkEdenKullaniciId) REFERENCES Users(Id),
        CONSTRAINT FK_TalepSevkler_HedefKullanici
            FOREIGN KEY (HedefKullaniciId) REFERENCES Users(Id),
        CONSTRAINT FK_TalepSevkler_HedefRol
            FOREIGN KEY (HedefRolId) REFERENCES OperationClaims(Id)
    );
    PRINT 'TalepSevkler tablosu oluşturuldu.';
END
ELSE
    PRINT 'TalepSevkler zaten mevcut, atlandı.';
GO

-- ── 7. TalepEkler ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TalepEkler')
BEGIN
    CREATE TABLE TalepEkler (
        Id                   INT            IDENTITY(1,1) PRIMARY KEY,
        TalepId              INT            NOT NULL,
        YanitId              INT            NULL,
        DosyaAdi             NVARCHAR(500)  NOT NULL,
        DosyaYolu            NVARCHAR(1000) NOT NULL,
        DosyaBoyutu          BIGINT         NOT NULL DEFAULT 0,
        DosyaTipi            NVARCHAR(100)  NOT NULL DEFAULT '',
        YukleyenKullaniciId  INT            NOT NULL,
        YuklemeTarihi        DATETIME       NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TalepEkler_Talep
            FOREIGN KEY (TalepId) REFERENCES Talepler(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TalepEkler_Yanit
            FOREIGN KEY (YanitId) REFERENCES TalepYanitlar(Id),
        CONSTRAINT FK_TalepEkler_YukleyenKullanici
            FOREIGN KEY (YukleyenKullaniciId) REFERENCES Users(Id)
    );
    PRINT 'TalepEkler tablosu oluşturuldu.';
END
ELSE
    PRINT 'TalepEkler zaten mevcut, atlandı.';
GO

-- ── İndeksler ────────────────────────────────────────────────────────────────
PRINT '=== İndeksler oluşturuluyor... ===';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Talepler_OlusturanKullaniciId')
    CREATE INDEX IX_Talepler_OlusturanKullaniciId ON Talepler(OlusturanKullaniciId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Talepler_AtananKullaniciId')
    CREATE INDEX IX_Talepler_AtananKullaniciId ON Talepler(AtananKullaniciId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Talepler_Durum')
    CREATE INDEX IX_Talepler_Durum ON Talepler(Durum);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Talepler_KategoriId')
    CREATE INDEX IX_Talepler_KategoriId ON Talepler(KategoriId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TalepYanitlar_TalepId')
    CREATE INDEX IX_TalepYanitlar_TalepId ON TalepYanitlar(TalepId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TalepChatler_TalepId')
    CREATE INDEX IX_TalepChatler_TalepId ON TalepChatler(TalepId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TalepAtamalar_TalepId')
    CREATE INDEX IX_TalepAtamalar_TalepId ON TalepAtamalar(TalepId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TalepSevkler_TalepId')
    CREATE INDEX IX_TalepSevkler_TalepId ON TalepSevkler(TalepId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TalepEkler_TalepId')
    CREATE INDEX IX_TalepEkler_TalepId ON TalepEkler(TalepId);

PRINT 'İndeksler oluşturuldu.';
GO

-- ── Örnek Kategori Verileri ──────────────────────────────────────────────────
PRINT '=== Örnek kategori verileri ekleniyor... ===';

IF NOT EXISTS (SELECT 1 FROM TalepKategoriler WHERE Ad = 'Teknik Destek')
BEGIN
    INSERT INTO TalepKategoriler (Ad, Aciklama, AktifMi) VALUES
    ('Teknik Destek',   'Yazılım ve donanım teknik sorunları', 1),
    ('İnsan Kaynakları','İK ile ilgili talepler', 1),
    ('Finans',          'Muhasebe ve finans talepleri', 1),
    ('Genel',           'Diğer talepler', 1);

    PRINT 'Örnek kategoriler eklendi.';
END
ELSE
    PRINT 'Kategoriler zaten mevcut, atlandı.';
GO

PRINT '=== Talep Modülü migration tamamlandı! ===';
GO


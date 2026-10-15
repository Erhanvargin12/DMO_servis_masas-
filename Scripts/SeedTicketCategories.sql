USE CoreDev;
GO

IF NOT EXISTS (SELECT 1 FROM ticket.Categories)
BEGIN
    INSERT INTO ticket.Categories (Name, Description) VALUES 
    (N'Yazılım & Uygulama Talepleri', N'Yazılım geliştirme, portal, modül ve sistem hata bildirimleri'),
    (N'Donanım & Çevre Birimleri', N'Bilgisayar, monitör, yazıcı, tarayıcı ve diğer donanım arızaları'),
    (N'Ağ, İnternet & VPN', N'Ağ erişimi, Wi-Fi, kablolu bağlantı ve VPN erişim sorunları'),
    (N'Sistem, Sunucu & Şifre İşlemleri', N'Kullanıcı hesabı, şifre sıfırlama, yetkilendirme ve sunucu erişimleri'),
    (N'İş Analizi & Fonksiyonel Destek', N'Süreç analizi, iş akışı ve fonksiyonel iyileştirme talepleri'),
    (N'Veri & İstatistik Raporlama', N'Veri analitiği, raporlama ve istatistiki bilgi talepleri'),
    (N'Genel Bilgi İşlem Destek', N'Diğer genel bilgi işlem soru ve yardım talepleri');
END
GO

IF NOT EXISTS (SELECT 1 FROM ticket.CategoryBranches)
BEGIN
    -- 1. Yazılım -> Yazılım Şube (2)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 2, 1 FROM ticket.Categories c WHERE c.Name = N'Yazılım & Uygulama Talepleri';

    -- 2. Donanım -> Donanım Şube (6)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 6, 1 FROM ticket.Categories c WHERE c.Name = N'Donanım & Çevre Birimleri';

    -- 3. Ağ & VPN -> Sistem ve Ağ Şube (5)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 5, 1 FROM ticket.Categories c WHERE c.Name = N'Ağ, İnternet & VPN';

    -- 4. Sistem & Sunucu -> Sistem ve Ağ Şube (5)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 5, 1 FROM ticket.Categories c WHERE c.Name = N'Sistem, Sunucu & Şifre İşlemleri';

    -- 5. İş Analizi -> Fonksiyonel Şube (4)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 4, 1 FROM ticket.Categories c WHERE c.Name = N'İş Analizi & Fonksiyonel Destek';

    -- 6. Veri & İstatistik -> İstatistik Şube (3)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 3, 1 FROM ticket.Categories c WHERE c.Name = N'Veri & İstatistik Raporlama';

    -- 7. Genel Destek -> Donanım Şube (6)
    INSERT INTO ticket.CategoryBranches (CategoryId, BranchId, DisplayOrder)
    SELECT c.Id, 6, 1 FROM ticket.Categories c WHERE c.Name = N'Genel Bilgi İşlem Destek';
END
GO

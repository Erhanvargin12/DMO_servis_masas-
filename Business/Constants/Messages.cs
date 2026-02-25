namespace Business.Constants;

public static class Messages
{
    #region General Messages
    public const string ActionSuccessful = "İşlem başarılı.";
    public const string ActionFailed = "İşlem başarısız.";
    public const string AuthorizationsDenied = "Yetkiniz yok!";
    public const string UserNotFound = "Kullanıcı bulunamadı!";
    public const string UserNotActive = "Kullanıcı hesabı aktif değil!";
    public const string LdapError = "Personel bulunamadı!";
    public const string PasswordError = "Hatalı Parola";
    public const string SuccessfulLogin = "Giriş Başarılı";
    public const string AccessTokenCreated = "Erişim jetonu oluşturuldu.";

    #endregion

    #region Token Messages
    public const string InvalidToken = "Geçersiz token!";
    public const string InvalidRefreshToken = "Geçersiz refresh token!";
    public const string RefreshTokenExpired = "Refresh token süresi dolmuş. Lütfen tekrar giriş yapın.";
    public const string TokenRefreshed = "Token başarıyla yenilendi.";
    public const string TokenRevoked = "Token başarıyla iptal edildi.";
    public const string TokenAlreadyRevoked = "Token zaten iptal edilmiş.";
    public const string TokenReuseDetected = "Güvenlik ihlali tespit edildi. Tüm oturumlar sonlandırıldı.";
    public const string AllTokensRevoked = "Tüm oturumlar sonlandırıldı.";
    #endregion

    #region User Messages
    public const string UserAdded = "Kullanıcı başarıyla eklendi!";
    public const string UserDeleted = "Kullanıcı başarıyla silindi!";
    public const string UserUpdated = "Kullanıcı başarıyla güncellendi!";
    public const string UserNameInvalid = "Kullanıcı adı en az 2 karakter olmalıdır.";
    public const string UsersList = "Kullanıcılar listelendi.";
    public const string UserDetailsList = "Kullanıcı detayları listelendi.";
    public const string UserMailAlreadyExists = "Bu kullanıcı maili sistemde zaten kayıtlı!";
    public const string UserRegistrationIdAlreadyExists = "Bu kullanıcı sicil numarası sistemde zaten kayıtlı!";
    public const string UserAlreadyExists = "Bu kullanıcı zaten var!";

    #endregion

    #region OperationClaim Messages
    public const string OperationClaimsListed = "Roller listelendi.";
    public const string OperationClaimAdded = "Rol başarıyla eklendi!";
    public const string OperationClaimUpdated = "Rol başarıyla güncellendi!";
    public const string OperationClaimDeleted = "Rol başarıyla silindi!";
    public const string OperationClaimNotFound = "Rol bulunamadı!";
    public const string OperationClaimAlreadyExists = "Bu rol zaten mevcut!";
    #endregion

    #region UserOperationClaim Messages
    public const string UserOperationClaimsListed = "Kullanıcı rolleri listelendi.";
    public const string UserOperationClaimAdded = "Kullanıcıya rol atandı!";
    public const string UserOperationClaimDeleted = "Kullanıcıdan rol kaldırıldı!";
    public const string UserOperationClaimsDeleted = "Kullanıcının tüm rolleri kaldırıldı!";
    public const string UserOperationClaimAlreadyExists = "Bu rol kullanıcıya zaten atanmış!";
    public const string UserOperationClaimNotFound = "Kullanıcı rol ataması bulunamadı!";
    #endregion

    #region Menu Messages
    public const string MenusListed = "Menüler listelendi.";
    public const string MenuAdded = "Menü başarıyla eklendi!";
    public const string MenuUpdated = "Menü başarıyla güncellendi!";
    public const string MenuDeleted = "Menü başarıyla silindi!";
    public const string MenuNotFound = "Menü bulunamadı!";
    public const string MenuAlreadyExists = "Bu menü zaten mevcut!";
    public const string MenuHasChildren = "Bu menünün alt menüleri var, önce onları silin!";
    #endregion

    #region Talep Messages
    public const string TaleplerListelendi = "Talepler listelendi.";
    public const string TalepOlusturuldu = "Talep başarıyla oluşturuldu!";
    public const string TalepGuncellendi = "Talep başarıyla güncellendi!";
    public const string TalepSilindi = "Talep başarıyla silindi!";
    public const string TalepBulunamadi = "Talep bulunamadı!";
    public const string TalepBaslikGecersiz = "Talep başlığı en az 5 karakter olmalıdır!";
    public const string TalepDurumuGuncellendi = "Talep durumu güncellendi!";
    public const string TalepAtandi = "Talep kullanıcıya atandı!";
    public const string TalepSevkedildi = "Talep başarıyla sevk edildi!";
    public const string SevkHedefGerekli = "Sevk için hedef kullanıcı veya rol seçilmelidir!";

    public const string YanitEklendi = "Yanıt başarıyla eklendi!";
    public const string YanitSilindi = "Yanıt silindi!";
    public const string YanitBulunamadi = "Yanıt bulunamadı!";
    public const string YanitIcerikBos = "Yanıt içeriği boş olamaz!";

    public const string ChatMesajEklendi = "Mesaj gönderildi!";
    public const string ChatMesajBos = "Mesaj boş olamaz!";

    public const string EkEklendi = "Dosya başarıyla eklendi!";
    public const string EkSilindi = "Dosya silindi!";
    public const string EkBulunamadi = "Dosya bulunamadı!";

    public const string KategoriEklendi = "Kategori başarıyla eklendi!";
    public const string KategoriGuncellendi = "Kategori güncellendi!";
    public const string KategoriSilindi = "Kategori silindi!";
    public const string KategoriBulunamadi = "Kategori bulunamadı!";
    #endregion

    #region MenuOperationClaim Messages
    public const string MenuOperationClaimsListed = "Menü yetkileri listelendi.";
    public const string MenuOperationClaimAdded = "Menüye yetki atandı!";
    public const string MenuOperationClaimDeleted = "Menüden yetki kaldırıldı!";
    public const string MenuOperationClaimsDeleted = "Menünün tüm yetkileri kaldırıldı!";
    public const string MenuOperationClaimAlreadyExists = "Bu yetki menüye zaten atanmış!";
    public const string MenuOperationClaimNotFound = "Menü yetki ataması bulunamadı!";
    #endregion
}

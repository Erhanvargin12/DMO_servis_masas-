namespace Business.Constants;

public static class Messages
{
    #region General Messages
    public const string ActionSuccessful = "İşlem başarılı.";
    public const string ActionFailed = "İşlem başarısız.";
    internal static string? AuthorizationsDenied = "Yetkiniz yok!";
    public const string UserNotFound = "Kullanıcı bulunamadı!";
    public const string LdapError = "Personel bulunamadı!";
    public const string PasswordError = "Hatalı Parola";
    public const string SuccessfulLogin = "Giriş Başarılı";
    internal static string AccessTokenCreated = "Erişim jetonu oluşturuldu.";

    #endregion

    #region User Messages
    public const  string UserAdded = "Kullanıcı başarıyla eklendi!";
    public const string UserDeleted = "Kullanıcı başarıyla silindi!";
    public const string UserUpdated = "Kullanıcı başarıyla güncellendi!";
    public const string UserNameInvalid = "Kullanıcı adı en az 2 karakter olmalıdır.";
    public const string UsersList = "Kullanıcılar listelendi.";
    public const string UserDetailsList = "Kullanıcı detayları listelendi.";
    internal static string UserMailAlreadyExists = "Bu kullanıcı maili sistemde zaten kayıtlı!";
    internal static string UserRegistrationIdAlreadyExists = "Bu kullanıcı sicil numarası sistemde zaten kayıtlı!";
    internal static string UserAlreadyExists = "Bu kullaıcı zaten var!";



    #endregion
}

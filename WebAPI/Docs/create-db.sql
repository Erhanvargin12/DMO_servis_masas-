-- Her ortam için (development, staging, production) bir db oluştur
-- Oluşan dblerin connection stringlerini appsettings'e kaydet

CREATE DATABASE CoreDev;
GO
USE CoreDev;
GO


CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Mail NVARCHAR(200) NOT NULL,
    PasswordHash VARBINARY(MAX) NOT NULL,
    PasswordSalt VARBINARY(MAX) NOT NULL,
    Status BIT NOT NULL DEFAULT(1),
    CreateAt DATETIME NOT NULL,
    UpdateAt DATETIME NULL,
    RegistirationId INT NOT NULL,
    OrganizationUnitId INT NOT NULL,
    PositionId INT NOT NULL
);

CREATE TABLE OperationClaims
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(250)
);

CREATE TABLE UserOperationClaims
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    OperationClaimId INT NOT NULL FOREIGN KEY REFERENCES OperationClaims(Id)
);

CREATE TABLE UserClaims
(
    UserId INT NOT NULL,
    ClaimId INT NOT NULL,
    PRIMARY KEY (UserId, ClaimId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ClaimId) REFERENCES OperationClaims(Id)
);
GO


INSERT INTO Users(FirstName, LastName, Mail, PasswordHash, PasswordSalt, Status, CreateAt, RegistirationId, OrganizationUnitId, PositionId)
VALUES ('Test','User','user@dev.com',0x010203,0x040506,1,GETDATE(),1,10,5);

INSERT INTO OperationClaims (Name, Description) 
VALUES ('Admin','Yönetim Yetkisi'), ('User','Temel Kullanıcı Yetkisi'), ('Inspector','Denetim Yetkisi');

INSERT INTO UserOperationClaims (UserId, OperationClaimId)
VALUES (1,1),(1,2);
GO

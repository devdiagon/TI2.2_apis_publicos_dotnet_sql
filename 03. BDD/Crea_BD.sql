IF DB_ID(N'ApiPaisesDB') IS NULL
BEGIN
    CREATE DATABASE ApiPaisesDB;
    PRINT 'Base de datos ApiPaisesDB creada.';
END
ELSE
    PRINT 'La base de datos ApiPaisesDB ya existe.';
GO

/*

USE ApiPaisesDB;
GO

CREATE TABLE Paises(
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Nombre    NVARCHAR(100) NOT NULL,
    Capital   NVARCHAR(100) NULL,
    Region    NVARCHAR(100) NOT NULL,
    SubRegion NVARCHAR(100) NULL,
    Poblacion BIGINT        NOT NULL,
    Area      FLOAT         NULL,
    Moneda    NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Paises_Nombre ON Paises(Nombre);
GO


CREATE TABLE Universidades(
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Nombre    NVARCHAR(200) NOT NULL,
    Dominio   NVARCHAR(100) NULL,
    SitioWeb  NVARCHAR(300) NULL,
    Pais      NVARCHAR(100) NOT NULL
);
GO

CREATE UNIQUE INDEX UX_Universidades_Nombre_Pais ON Universidades(Nombre, Pais);
GO

*/

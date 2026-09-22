-- Schema pentru exemplul de publicare a notelor unui examen (Lucrarea 8).
-- Script idempotent: poate fi rulat de mai multe ori fără eroare.

IF DB_ID(N'Student') IS NULL
BEGIN
    CREATE DATABASE [Student];
END
GO

USE [Student];
GO

IF OBJECT_ID(N'dbo.Student', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Student]
    (
        [StudentId]          INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Student] PRIMARY KEY CLUSTERED,
        [RegistrationNumber] VARCHAR(7)  NOT NULL CONSTRAINT [UQ_Student_RegistrationNumber] UNIQUE,
        [Name]               NVARCHAR(50) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Grade', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Grade]
    (
        [GradeId]   INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Grade] PRIMARY KEY CLUSTERED,
        [StudentId] INT NOT NULL CONSTRAINT [FK_Grade_Student] REFERENCES [dbo].[Student]([StudentId]),
        [Exam]      DECIMAL(4, 2) NULL CONSTRAINT [CK_Grade_Exam] CHECK ([Exam] > 0 AND [Exam] <= 10),
        [Activity]  DECIMAL(4, 2) NULL CONSTRAINT [CK_Grade_Activity] CHECK ([Activity] > 0 AND [Activity] <= 10),
        [Final]     DECIMAL(4, 2) NULL CONSTRAINT [CK_Grade_Final] CHECK ([Final] > 0 AND [Final] <= 10),
        -- O singură notă per student: ținta pentru upsert-ul din Examples.Data.Repositories.GradesRepository.
        CONSTRAINT [UQ_Grade_StudentId] UNIQUE ([StudentId])
    );
END
GO

-- Date de start: aceiași patru studenți folosiți în exemplul consolă din Lucrarea 3.
MERGE [dbo].[Student] AS target
USING (VALUES
    (N'LM12345', N'Ana Popescu'),
    (N'LM54321', N'Mihai Ionescu'),
    (N'LM67890', N'Ioana Dumitrescu'),
    (N'LM98765', N'Andrei Georgescu')
) AS source ([RegistrationNumber], [Name])
ON target.[RegistrationNumber] = source.[RegistrationNumber]
WHEN NOT MATCHED THEN
    INSERT ([RegistrationNumber], [Name]) VALUES (source.[RegistrationNumber], source.[Name]);
GO

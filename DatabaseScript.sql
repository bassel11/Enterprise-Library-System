-- =========================================================================
-- Enterprise Library Management System - Master Database Script
-- =========================================================================

CREATE DATABASE EnterpriseLibraryDB;
GO

USE EnterpriseLibraryDB;
GO

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE, -- Unique Constraint
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit & Soft Delete Fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedBy INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL
);
GO

CREATE TABLE Books (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL UNIQUE, -- Unique Constraint
    IsAvailable BIT NOT NULL DEFAULT 1,
    
    -- Audit & Soft Delete Fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedBy INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL
);
GO

CREATE TABLE Borrowings (
    BorrowId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    BookId INT NOT NULL FOREIGN KEY REFERENCES Books(BookId),
    BorrowDate DATETIME NOT NULL DEFAULT GETDATE(),
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL
);
GO

-- =========================================================================
-- High-Performance Filtered Indexes
-- =========================================================================
CREATE NONCLUSTERED INDEX IX_Books_ISBN_Active ON Books(ISBN) WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Books_Availability ON Books(IsAvailable) WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Users_Active ON Users(IsActive) WHERE IsDeleted = 0;
GO
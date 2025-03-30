-- Create the database (run this separately with SA privileges)
-- CREATE DATABASE AppDb;
-- GO

-- Use the database
USE FriendRequests;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        ProfilePhotoPath NVARCHAR(255) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END;
GO

-- Create FriendRequests table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FriendRequests')
BEGIN
    CREATE TABLE FriendRequests (
        RequestId INT IDENTITY(1,1) PRIMARY KEY,
        SenderUsername NVARCHAR(50) NOT NULL,
        SenderEmail NVARCHAR(100) NOT NULL,
        SenderProfilePhotoPath NVARCHAR(255) NULL,
        ReceiverUsername NVARCHAR(50) NOT NULL,
        RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_SenderReceiver UNIQUE (SenderUsername, ReceiverUsername)
    );
END;
GO

-- Create Friends table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Friends')
BEGIN
    CREATE TABLE Friends (
        FriendshipId INT IDENTITY(1,1) PRIMARY KEY,
        User1Username NVARCHAR(50) NOT NULL,
        User2Username NVARCHAR(50) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_Friendship UNIQUE (User1Username, User2Username)
    );
END;
GO

-- Insert sample data
-- Sample users
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'MainUser')
BEGIN
    INSERT INTO Users (Username, Email, ProfilePhotoPath)
    VALUES ('MainUser', 'main.user@example.com', 'ms-appx:///Assets/default_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'User1')
BEGIN
    INSERT INTO Users (Username, Email, ProfilePhotoPath)
    VALUES ('User1', 'user1@example.com', 'ms-appx:///Assets/friend1_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'User2')
BEGIN
    INSERT INTO Users (Username, Email, ProfilePhotoPath)
    VALUES ('User2', 'user2@example.com', 'ms-appx:///Assets/friend2_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'User3')
BEGIN
    INSERT INTO Users (Username, Email, ProfilePhotoPath)
    VALUES ('User3', 'user3@example.com', 'ms-appx:///Assets/friend3_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'User4')
BEGIN
    INSERT INTO Users (Username, Email, ProfilePhotoPath)
    VALUES ('User4', 'user4@example.com', 'ms-appx:///Assets/request1_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'User5')
BEGIN
    INSERT INTO Users (Username, Email, ProfilePhotoPath)
    VALUES ('User5', 'user5@example.com', 'ms-appx:///Assets/request2_avatar.png');
END;

-- Sample friend requests
IF NOT EXISTS (SELECT * FROM FriendRequests WHERE SenderUsername = 'User4' AND ReceiverUsername = 'MainUser')
BEGIN
    INSERT INTO FriendRequests (SenderUsername, SenderEmail, SenderProfilePhotoPath, ReceiverUsername, RequestDate)
    VALUES ('User4', 'user4@example.com', 'ms-appx:///Assets/request1_avatar.png', 'MainUser', DATEADD(day, -1, GETDATE()));
END;

IF NOT EXISTS (SELECT * FROM FriendRequests WHERE SenderUsername = 'User5' AND ReceiverUsername = 'EmaB')
BEGIN
    INSERT INTO FriendRequests (SenderUsername, SenderEmail, SenderProfilePhotoPath, ReceiverUsername, RequestDate)
    VALUES ('User5', 'user5@example.com', 'ms-appx:///Assets/request2_avatar.png', 'MainUser', DATEADD(day, -2, GETDATE()));
END;

-- Sample friendships
IF NOT EXISTS (SELECT * FROM Friends WHERE User1Username = 'MainUser' AND User2Username = 'User1')
BEGIN
    INSERT INTO Friends (User1Username, User2Username)
    VALUES ('MainUser', 'User1');
END;

IF NOT EXISTS (SELECT * FROM Friends WHERE User1Username = 'MainUser' AND User2Username = 'User2')
BEGIN
    INSERT INTO Friends (User1Username, User2Username)
    VALUES ('MainUser', 'User2');
END;

IF NOT EXISTS (SELECT * FROM Friends WHERE User1Username = 'MainUser' AND User2Username = 'User3')
BEGIN
    INSERT INTO Friends (User1Username, User2Username)
    VALUES ('MainUser', 'User3');
END;
GO

-- Instructions for running this script:
-- 1. Open SQL Server Management Studio
-- 2. Connect to your SQL Server instance (CARINA-HP)
-- 3. Create a new database named "AppDb" if it doesn't exist
-- 4. Run this script to create the tables and insert sample data 
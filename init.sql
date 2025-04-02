use Community
go

drop table if exists UserLikedComment
drop table if exists UserLikedPost
drop table if exists UserDislikedComment
drop table if exists UserDislikedPost
drop table if exists ForumComments
drop table if exists ForumPosts

create table ForumPosts (
	post_id int primary key identity(1,1),
	title nvarchar(max),
	body nvarchar(max),
	creation_date datetime,
	author_id int, -- should be foreign key referencing users table
	score int,
	game_id int null, -- should be foreign key referencing games table
)

create table ForumComments (
	comment_id int primary key identity(1,1),
	body nvarchar(max),
	creation_date datetime,
	author_id int,
	score int,
	post_id int foreign key references ForumPosts(post_id) on delete cascade
)

create table UserLikedComment (
	userId int, -- should be foreign key referencing users table
	comment_id int foreign key references ForumComments(comment_id) on delete cascade,
	primary key (userId, comment_id)
)

create table UserLikedPost (
	userId int, -- should be foreign key referencing users table
	post_id int foreign key references ForumPosts(post_id) on delete cascade,
	primary key (userId, post_id)
)

create table UserDislikedComment (
	userId int, -- should be foreign key referencing users table
	comment_id int foreign key references ForumComments(comment_id) on delete cascade,
	primary key (userId, comment_id)
)

create table UserDislikedPost (
	userId int, -- should be foreign key referencing users table
	post_id int foreign key references ForumPosts(post_id) on delete cascade,
	primary key (userId, post_id)
)

DROP TABLE IF EXISTS Ratings
DROP TABLE IF EXISTS NewsComments
DROP TABLE IF EXISTS NewsPosts

CREATE TABLE NewsPosts (
	id INT PRIMARY KEY IDENTITY(1, 1),
	authorId INT,
	content NVARCHAR(MAX),
	uploadDate DATETIME,
	nrLikes INT,
	nrDislikes INT,
	nrComments INT
)

CREATE TABLE NewsComments (
	id INT PRIMARY KEY IDENTITY(1, 1),
	authorId INT,
	postId INT NULL FOREIGN KEY REFERENCES NewsPosts(id) ON DELETE CASCADE,
	content NVARCHAR(MAX),
	uploadDate DATETIME,
)

CREATE TABLE Ratings (
	postId INT FOREIGN KEY REFERENCES NewsPosts(id),
	authorId INT,
	ratingType BIT
	PRIMARY KEY(postId, authorId)
)

DROP TABLE IF EXISTS CHAT_INVITES
DROP TABLE IF EXISTS USERS

CREATE TABLE USERS(
	id INT PRIMARY KEY,
	username NVARCHAR(50),
	ipAddress VARCHAR(50),
)

CREATE TABLE CHAT_INVITES(
	sender INT FOREIGN KEY REFERENCES USERS(id),
	receiver INT FOREIGN KEY REFERENCES USERS(id),
)

INSERT INTO USERS (id, username, ipAddress) VALUES (1, 'JaneSmith', '192.168.50.247')
INSERT INTO USERS (id, username, ipAddress) VALUES (2, 'Justin', '192.168.50.163')
INSERT INTO USERS (id, username, ipAddress) VALUES (3, 'Andrei', '192.168.50.164')
INSERT INTO USERS (id, username, ipAddress) VALUES (4, 'Marius', '192.168.50.165')

-- SELECT * FROM USERS

drop table if exists FriendUsers
drop table if exists FriendRequests
go
-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FriendUsers')
BEGIN
    CREATE TABLE FriendUsers (
        UserId INT PRIMARY KEY,
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
IF NOT EXISTS (SELECT * FROM FriendUsers WHERE Username = 'JaneSmith')
BEGIN
    INSERT INTO FriendUsers (UserId, Username, Email, ProfilePhotoPath)
    VALUES (1, 'JaneSmith', 'jane.smith.69@fake.email.ai.com"', 'ms-appx:///Assets/default_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM FriendUsers WHERE Username = 'User1')
BEGIN
    INSERT INTO FriendUsers (UserId, Username, Email, ProfilePhotoPath)
    VALUES (2, 'User1', 'user1@example.com', 'ms-appx:///Assets/friend1_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM FriendUsers WHERE Username = 'User2')
BEGIN
    INSERT INTO FriendUsers (UserId, Username, Email, ProfilePhotoPath)
    VALUES (3, 'User2', 'user2@example.com', 'ms-appx:///Assets/friend2_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM FriendUsers WHERE Username = 'User3')
BEGIN
    INSERT INTO FriendUsers (UserId, Username, Email, ProfilePhotoPath)
    VALUES (4, 'User3', 'user3@example.com', 'ms-appx:///Assets/friend3_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'User4')
BEGIN
    INSERT INTO FriendUsers (UserId, Username, Email, ProfilePhotoPath)
    VALUES (5, 'User4', 'user4@example.com', 'ms-appx:///Assets/request1_avatar.png');
END;

IF NOT EXISTS (SELECT * FROM FriendUsers WHERE Username = 'User5')
BEGIN
    INSERT INTO FriendUsers (UserId, Username, Email, ProfilePhotoPath)
    VALUES (6, 'User5', 'user5@example.com', 'ms-appx:///Assets/request2_avatar.png');
END;

-- Sample friend requests
IF NOT EXISTS (SELECT * FROM FriendRequests WHERE SenderUsername = 'User4' AND ReceiverUsername = 'MainUser')
BEGIN
    INSERT INTO FriendRequests (SenderUsername, SenderEmail, SenderProfilePhotoPath, ReceiverUsername, RequestDate)
    VALUES ('User4', 'user4@example.com', 'ms-appx:///Assets/request1_avatar.png', 'JaneSmith', DATEADD(day, -1, GETDATE()));
END;

IF NOT EXISTS (SELECT * FROM FriendRequests WHERE SenderUsername = 'User5' AND ReceiverUsername = 'EmaB')
BEGIN
    INSERT INTO FriendRequests (SenderUsername, SenderEmail, SenderProfilePhotoPath, ReceiverUsername, RequestDate)
    VALUES ('User5', 'user5@example.com', 'ms-appx:///Assets/request2_avatar.png', 'JaneSmith', DATEADD(day, -2, GETDATE()));
END;

-- Sample friendships
IF NOT EXISTS (SELECT * FROM Friends WHERE User1Username = 'JaneSmith' AND User2Username = 'User1')
BEGIN
    INSERT INTO Friends (User1Username, User2Username)
    VALUES ('JaneSmith', 'User1');
END;

IF NOT EXISTS (SELECT * FROM Friends WHERE User1Username = 'JaneSmith' AND User2Username = 'User2')
BEGIN
    INSERT INTO Friends (User1Username, User2Username)
    VALUES ('JaneSmith', 'User2');
END;

IF NOT EXISTS (SELECT * FROM Friends WHERE User1Username = 'JaneSmith' AND User2Username = 'User3')
BEGIN
    INSERT INTO Friends (User1Username, User2Username)
    VALUES ('JaneSmith', 'User3');
END;
GO

-- select * from Friends
drop table if exists Reviews
drop table if exists ReviewsUsers
drop table if exists Games

CREATE TABLE Games (

    GameId INT PRIMARY KEY  IDENTITY (1,1) , 
    Title NVARCHAR (100)  NOT NULL, 
    Description NVARCHAR (MAX) , 
    ReleaseDate DATE , 
    CoverImage  VARBINARY (MAX)  -- For storing Images as BLOB  (Binary Large Object) 

) ; 


-- Create the Users Table 



CREATE TABLE ReviewsUsers ( 
   UserId  INT PRIMARY KEY, 
   Name  NVARCHAR (100) NOT NULL, 
   ProfilePicture VARBINARY (MAX) -- Also BLOB 

)


-- Create the Reviews Table 

CREATE TABLE Reviews (  
   ReviewId INT PRIMARY KEY IDENTITY(1,1), 
   Title NVARCHAR (100) NOT NULL , 
   Content NVARCHAR (2000) NOT NULL, 
   IsRecommended BIT , 
   Rating DECIMAL (3,1)  CHECK  (Rating BETWEEN 1.0 AND 5.0 ), 
   HelpfulVotes INT DEFAULT 0, 
   FunnyVotes INT DEFAULT 0, 
   HoursPlayed INT, 
   CreatedAt DATETIME DEFAULT GETDATE(), 


   -- FOREIGN KEYS 
   UserId INT NOT NULL, 
   GameId INT NOT NULL, 


   CONSTRAINT FK_Review_User  FOREIGN KEY (UserId) REFERENCES ReviewsUsers(UserId), 
   CONSTRAINT FK_Review_Game FOREIGN KEY (GameId) REFERENCES Games(GameId)

);

-- Insert Sample Users
INSERT INTO ReviewsUsers (UserId, Name, ProfilePicture) VALUES 
(1, 'JaneSmith', NULL),
(2, 'Sam Carter', NULL),
(3, 'Taylor Kim', NULL);

-- Insert Sample Games
INSERT INTO Games (Title, Description, ReleaseDate, CoverImage) VALUES
('Eternal Odyssey', 'An epic space exploration game.', '2022-11-05', NULL),
('Shadow Relic', 'A mystical action RPG set in ancient ruins.', '2021-06-14', NULL),
('Pixel Racers', 'Fast-paced arcade racing game.', '2023-02-20', NULL);

-- Insert Sample Reviews
INSERT INTO Reviews (Title, Content, IsRecommended, Rating, HelpfulVotes, FunnyVotes, HoursPlayed, CreatedAt, UserId, GameId) VALUES
('Loved it!', 'This game was amazing. I spent so many hours exploring!', 1, 4.8, 3, 1, 35, GETDATE(), 1, 1),
('Too buggy at launch', 'Crashes often but has potential.', 0, 2.5, 5, 0, 10, GETDATE(), 2, 2),
('Solid gameplay', 'It reminds me of old-school racers with a twist.', 1, 4.2, 2, 3, 20, GETDATE(), 3, 3),
('Good but needs polish', 'Fun mechanics but graphics are a bit outdated.', 1, 3.9, 1, 0, 18, GETDATE(), 1, 2),
('Underrated gem', 'A surprisingly deep and rewarding experience.', 1, 4.5, 4, 2, 40, GETDATE(), 2, 1);



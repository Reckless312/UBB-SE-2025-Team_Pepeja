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

-- Sample data for ForumPosts table

INSERT INTO ForumPosts (title, body, creation_date, author_id, score, game_id)
VALUES
('Looking for Players - New RPG Campaign!', 'Hey everyone, Im starting a new tabletop RPG campaign using the [System Name] system. Looking for 3-4 dedicated players. Experience is a plus but not required. Well be playing on [Day of the week] evenings. Let me know if youre interested!', '2025-04-01 10:30:00', 5, 15, NULL),
('Question about the latest patch', 'Has anyone else noticed the increased difficulty after the latest patch for [Game Title]? Im struggling with the [Specific Enemy/Area] now. Any tips?', '2025-04-01 14:45:00', 12, 8, 101),
('My Review of [Game Title] - Spoiler Free!', 'Just finished playing through [Game Title] and wanted to share my thoughts. Overall, I really enjoyed the [Positive Aspect] and the [Another Positive Aspect]. The story was engaging, and the ending left me wanting more. Highly recommend!', '2025-04-01 18:00:00', 3, 22, 101),
('Best Strategy for [Specific Game Challenge]?', 'Im stuck on the [Specific Game Challenge] in [Game Title]. What are some of the best strategies youve found to overcome it? Any particular units or tactics that work well?', '2025-04-02 09:15:00', 7, 11, 105),
('Fan Art - [Game Title] Character', 'Just wanted to share a piece of fan art I created of my favorite character from [Game Title], [Character Name]. Hope you like it!', '2025-04-02 11:00:00', 9, 35, 101),
('Lore Discussion: The History of [In-Game Faction]', 'Lets delve into the lore of [In-Game Faction] in [Game Title]. What are your theories about their origins and their role in the game world?', '2025-04-02 13:30:00', 2, 18, 105),
('Looking for a Clan/Guild in [Game Title]', 'Active player looking for a friendly and active clan or guild in [Game Title]. Im level [Your Level] and enjoy [Types of Activities]. Let me know if your group is recruiting!', '2025-04-02 15:45:00', 15, 6, 103),
('Is [New Feature] Worth It?', 'Has anyone tried out the new [New Feature] in [Game Title] yet? Is it worth the [Cost/Effort]? What are your initial impressions?', '2025-04-02 17:00:00', 1, 14, 103),
('General Discussion Thread', 'A place for general discussion about anything gaming related! What are you currently playing? Any exciting news you want to share?', '2025-03-31 20:00:00', 10, 25, NULL),
('Help with [Specific Technical Issue]', 'Im having a technical issue with [Game Title]. [Describe the issue]. Has anyone else encountered this and found a solution?', '2025-03-31 16:30:00', 6, 7, 101),
('Favorite Moments in Gaming?', 'What are some of your all-time favorite moments in gaming? Could be anything from a challenging boss fight to a heartwarming story beat!', '2025-03-30 12:00:00', 11, 29, NULL),
('Suggestion for a New Game Mode in [Game Title]', 'I had an idea for a new game mode in [Game Title] where [Describe the game mode]. What do you all think? Would this be something youd like to see?', '2025-03-30 09:00:00', 4, 12, 105);


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


INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (1, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Game Store Update</h1><h2>Big news in gaming</h2><p>Our game store has just launched a new section for indie titles. Enjoy great discounts and curated lists!</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Click here for a secret discount code</span></p></body></html>', '2025-03-31 10:00:00', 120, 3, 15);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (2, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Exclusive Release</h1><h2>New Game Trailer</h2><p>Check out the trailer for the highly anticipated game release this month. Experience next-level graphics and immersive gameplay.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Spoiler: The main character is unexpected!</span></p></body></html>', '2025-03-31 11:15:00', 85, 2, 8);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (3, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Store Update</h1><h2>New Payment Options</h2><p>We''ve added several new payment methods to make your shopping easier and more secure.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Learn more about our new secure payment gateway</span></p></body></html>', '2025-03-31 12:30:00', 200, 5, 20);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (4, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Special Offer</h1><h2>Limited Time Discount</h2><p>Enjoy a limited time discount on all your favorite games. Hurry, offer ends soon!</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Secret code revealed upon clicking</span></p></body></html>', '2025-03-31 13:45:00', 150, 4, 10);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (5, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Community News</h1><h2>Top Fan Reviews</h2><p>Read what gamers have to say about the latest releases and updates.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Click to view hidden fan ratings</span></p></body></html>', '2025-03-31 14:10:00', 95, 2, 7);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (5, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Developer Update</h1><h2>Patch Notes Released</h2><p>Discover the latest patch notes for improved performance and bug fixes.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">See hidden technical details</span></p></body></html>', '2025-03-31 15:25:00', 210, 8, 18);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (4, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Sales Alert</h1><h2>Weekend Flash Sale</h2><p>Grab your favorite games at unbeatable prices this weekend only.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Reveal extra promo code</span></p></body></html>', '2025-03-31 16:40:00', 175, 6, 9);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (3, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>New Arrivals</h1><h2>Fresh Game Releases</h2><p>Discover the latest additions to our game collection, carefully selected for every type of gamer.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Hidden review snippet</span></p></body></html>', '2025-03-31 18:05:00', 130, 3, 11);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (2, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Tech Insights</h1><h2>Behind the Scenes</h2><p>Learn how our store is using cutting-edge technology to enhance your shopping experience.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">See our tech secret</span></p></body></html>', '2025-03-31 19:20:00', 160, 4, 14);
INSERT INTO NewsPosts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments) VALUES (1, N'<html><head><style>body{font-family:''Segoe UI'',sans-serif;margin:0;padding:0;color:#333;white-space:pre-wrap;overflow:scroll;}body::-webkit-scrollbar{display:none;}img{display:block;margin:0 auto;max-width:80%;max-height:500px;}h2{margin-top:0;color:#0066cc;font-size:18px;}.spoiler{background-color:black;user-select:none;color:black;cursor:pointer;padding:2px 5px;border-radius:3px;transition:color 0.2s ease-in-out;}.spoiler.revealed{color:white;}</style></head><body><h1>Market Trends</h1><h2>Weekly Analysis</h2><p>Our experts review the latest market trends and share insights into upcoming game releases and technology shifts.</p><p><span class="spoiler" onclick="this.classList.toggle(''revealed'')">Click for a hidden insight</span></p></body></html>', '2025-03-31 20:35:00', 145, 3, 10);

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

    GameId INT PRIMARY KEY , 
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
INSERT INTO Games (GameId, Title, Description, ReleaseDate, CoverImage) VALUES
(0, 'Eternal Odyssey', 'An epic space exploration game.', '2022-11-05', NULL),
(1, 'Shadow Relic', 'A mystical action RPG set in ancient ruins.', '2021-06-14', NULL),
(2, 'Pixel Racers', 'Fast-paced arcade racing game.', '2023-02-20', NULL);

-- Insert Sample Reviews
INSERT INTO Reviews (Title, Content, IsRecommended, Rating, HelpfulVotes, FunnyVotes, HoursPlayed, CreatedAt, UserId, GameId) VALUES
('Loved it!', 'This game was amazing. I spent so many hours exploring!', 1, 4.8, 3, 1, 35, GETDATE(), 1, 0),
('Too buggy at launch', 'Crashes often but has potential.', 0, 2.5, 5, 0, 10, GETDATE(), 2, 1),
('Solid gameplay', 'It reminds me of old-school racers with a twist.', 1, 4.2, 2, 3, 20, GETDATE(), 3, 2),
('Good but needs polish', 'Fun mechanics but graphics are a bit outdated.', 1, 3.9, 1, 0, 18, GETDATE(), 1, 1),
('Underrated gem', 'A surprisingly deep and rewarding experience.', 1, 4.5, 4, 2, 40, GETDATE(), 2, 0);


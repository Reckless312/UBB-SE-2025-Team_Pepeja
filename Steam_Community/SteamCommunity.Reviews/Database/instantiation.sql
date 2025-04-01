-- Create the Game Table 

CREATE TABLE Games (

    GameId INT PRIMARY KEY  IDENTITY (1,1) , 
    Title NVARCHAR (100)  NOT NULL, 
    Description NVARCHAR (MAX) , 
    ReleaseDate DATE , 
    CoverImage  VARBINARY (MAX)  -- For storing Images as BLOB  (Binary Large Object) 

) ; 


-- Create the Users Table 

CREATE TABLE Users ( 
   UserId  INT PRIMARY KEY IDENTITY(1,1), 
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


   CONSTRAINT FK_Review_User  FOREIGN KEY (UserId) REFERENCES Users(UserId), 
   CONSTRAINT FK_Review_Game FOREIGN KEY (GameId) REFERENCES Games(GameId)

);


-- index for sorting/filtering

CREATE NONCLUSTERED INDEX IX_Reviews_GameId_CreatedAt ON Reviews(GameId, CreatedAt); 
CREATE NONCLUSTERED INDEX IX_Reviews_Rating ON Reviews(Rating); 
CREATE NONCLUSTERED INDEX IX_Reviews_IsRecommended ON Reviews(IsRecommended); 


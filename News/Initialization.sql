USE News
GO

DROP TABLE IF EXISTS Ratings
DROP TABLE IF EXISTS Comments
DROP TABLE IF EXISTS Posts

CREATE TABLE Posts (
	id INT PRIMARY KEY IDENTITY(1, 1),
	authorId INT,
	content NVARCHAR(MAX),
	uploadDate DATETIME,
	nrLikes INT,
	nrDislikes INT,
	nrComments INT
)

CREATE TABLE Comments (
	id INT PRIMARY KEY IDENTITY(1, 1),
	authorId INT,
	postId INT NULL FOREIGN KEY REFERENCES Posts(id) ON DELETE CASCADE,
	content NVARCHAR(MAX),
	uploadDate DATETIME,
)

CREATE TABLE Ratings (
	postId INT FOREIGN KEY REFERENCES Posts(id),
	authorId INT,
	ratingType BIT
	PRIMARY KEY(postId, authorId)
)
USE News
GO

DROP TABLE IF EXISTS Posts
DROP TABLE IF EXISTS Comments

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
	postId INT NULL FOREIGN KEY REFERENCES Posts(id),
	commentId INT NULL FOREIGN KEY REFERENCES Comments(id),
	content NVARCHAR(MAX),
	uploadDate DATETIME,
	nrLikes INT,
	nrDislikes INT
)

-- TODO: Maybe add another table to track likes / dislikes for users (to be discussed with forum)
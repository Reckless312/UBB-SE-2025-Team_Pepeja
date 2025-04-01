

-- Insert Sample Users
INSERT INTO Users (Name, ProfilePicture) VALUES 
('Alex Johnson', NULL),
('Sam Carter', NULL),
('Taylor Kim', NULL);

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



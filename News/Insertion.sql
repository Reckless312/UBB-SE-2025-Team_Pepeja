USE News
GO

DELETE FROM Posts
DELETE FROM Comments

INSERT INTO Posts (authorId, content, uploadDate, nrLikes, nrDislikes, nrComments)
VALUES 
(
    1, 
    '<html><head></head><body><h1>Pixel Pioneers: Major Performance Update</h1>Our latest patch addresses memory leaks and improves frame rates by <b>35%</b>. Feedback from the <i>community</i> has been crucial in identifying these optimizations.<h2>Key Improvements</h2>Reduced CPU usage and smoother rendering on mid-tier hardware.</body></html>',
    '2024-03-15 10:30:00', 
    245, 
    12, 
    37
),
(
    2, 
    '<html><head></head><body><h1>Roguelike Realms: New Character Class Incoming!</h1>Get ready for the <b>Shadow Weaver</b>, a stealthy new class that completely changes gameplay dynamics. Launching <sup>next week</sup>.<h2>Unique Abilities</h2>Teleportation and invisibility mechanics that will revolutionize our combat system.</body></html>',
    '2024-03-16 14:45:00', 
    156, 
    8, 
    22
),
(
    3, 
    '<html><head></head><body><h1>Cosmic Crusaders: Multiplayer Beta Announcement</h1>After months of internal testing, we''re opening our <i>multiplayer beta</i>. Limited spots available for our most dedicated <b>space exploration enthusiasts</b>.<h3>Beta Details</h3>Sign-ups begin <sub>this Friday</sub> at midnight GMT.</body></html>',
    '2024-03-17 09:15:00', 
    312, 
    17, 
    54
),
(
    4, 
    '<html><head></head><body><h1>Dungeon Craft: Mod Support Expansion</h1>We''re introducing full modding capabilities with our new <b>Creator''s Kit</b>. Indie developers can now build entire <i>game experiences</i> within our engine.<h2>Community Integration</h2>Steam Workshop support coming <sup>next month</sup>.</body></html>',
    '2024-03-18 11:20:00', 
    189, 
    5, 
    28
),
(
    5, 
    '<html><head></head><body><h1>Neon Nights: Cross-Platform Update</h1>Major breakthrough in our cross-platform synchronization. Now supporting <b>PC, Switch, and mobile</b> with full <i>save state</i> transfers.<h3>Technical Achievement</h3>Completely rewritten networking layer to ensure <sub>seamless experience</sub>.</body></html>',
    '2024-03-19 16:55:00', 
    276, 
    14, 
    46
);
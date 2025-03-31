use Forum
go

drop table if exists UserLikedComment
drop table if exists UserLikedPost
drop table if exists UserDislikedComment
drop table if exists UserDislikedPost
drop table if exists Comments
drop table if exists Posts

create table Posts (
	post_id int primary key identity(1,1),
	title nvarchar(max),
	body nvarchar(max),
	creation_date datetime,
	author_id int, -- should be foreign key referencing users table
	score int,
	game_id int null, -- should be foreign key referencing games table
)

create table Comments (
	comment_id int primary key identity(1,1),
	body nvarchar(max),
	creation_date datetime,
	author_id int,
	score int,
	post_id int foreign key references Posts(post_id) on delete cascade
)

create table UserLikedComment (
	userId int, -- should be foreign key referencing users table
	comment_id int foreign key references Comments(comment_id) on delete cascade,
	primary key (userId, comment_id)
)

create table UserLikedPost (
	userId int, -- should be foreign key referencing users table
	post_id int foreign key references Posts(post_id) on delete cascade,
	primary key (userId, post_id)
)

create table UserDislikedComment (
	userId int, -- should be foreign key referencing users table
	comment_id int foreign key references Comments(comment_id) on delete cascade,
	primary key (userId, comment_id)
)

create table UserDislikedPost (
	userId int, -- should be foreign key referencing users table
	post_id int foreign key references Posts(post_id) on delete cascade,
	primary key (userId, post_id)
)
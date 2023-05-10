create table profile (
   nickName varchar(255),
   bio varchar(255),
   userId varchar(255),
   primary key (userId)
);

INSERT INTO profile (nickName, bio, userId) VALUES ('yurbur', 'This is my message.', 'google-oauth2|106008494241933327801');

:!! echo Creating the Decks Table

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Decks')
	drop table [dbo].[Decks];

create table [dbo].[Decks]
(
	[Id] int identity not null primary key,
	[Name] varchar(50) not null, 
    [OwnerId] int not null, 
    [Permissions] int not null default 0
)

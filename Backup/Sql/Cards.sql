:!! echo Creating the Cards Table

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Cards')
	drop table [dbo].[Cards];

create table [dbo].[Cards]
(
	[Id] int identity not null primary key, 
    [DeckId] int not null, 
    [CardName] varchar(50) not null
)

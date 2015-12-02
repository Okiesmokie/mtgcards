:!! echo Creating the Accounts Table

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Accounts')
	drop table [dbo].[Accounts];

create table [dbo].[Accounts]
(
	[Id] int identity not null primary key,
    [Username] nvarchar(50) not null, 
    [Password] varchar(50) not null, 
    [Email] varchar(50) not null, 
    [LoginHash] char(32) not null
)

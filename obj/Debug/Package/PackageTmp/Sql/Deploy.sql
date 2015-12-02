/* Usage: sqlcmd -E -Stcp:SERVERNAME\INSTANCENAME -iDeploy.sql */
:setvar DatabaseName "mtgcards"
:on error exit

:!! echo Deploying $(DatabaseName)

if not exists (select [name] from sys.databases where [name] = "$(DatabaseName)")
	create database $(DatabaseName)
go

use [$(DatabaseName)]

:r Accounts.sql
go

:r Cards.sql
go

:r Decks.sql
go

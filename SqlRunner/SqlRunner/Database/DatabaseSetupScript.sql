USE [SqlRunner];
GO

/****** Object:  Table [ACC-INFO\jfast].[Scripts]    Script Date: 12/16/2016 3:50:02 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[RunStatus]
    (
      [RunStatusId] [INT] PRIMARY KEY
                          IDENTITY(1, 1) ,
      [StatusName] [NVARCHAR](MAX) NOT NULL
    );

CREATE TABLE dbo.[RunnableScripts]
    (
      [RunnableScriptId] [INT] PRIMARY KEY
                               IDENTITY(1, 1)
                               NOT NULL ,
      [Contents] [NVARCHAR](MAX) NOT NULL ,
      [Domain] [NVARCHAR](MAX) NULL ,
      [Username] [NVARCHAR](MAX) NULL ,
      [HashedPassword] [NVARCHAR](MAX) NULL ,
      [Server] [NVARCHAR](MAX) NOT NULL ,
      [Database] [NVARCHAR](MAX) NULL ,
      [UseWindowsAuthentication] [BIT] NOT NULL ,
      [RequestedRunDatetime] [DATETIME] NULL ,
      [ActualRunDatetime] [DATETIME] NULL ,
      [ProcessingMachine] [NVARCHAR](MAX) NULL ,
      [ProcessId] [INT]  NULL ,
      [RunStatusId] [INT] NULL ,
	  [ErrorText] NVARCHAR(MAX) NULL,
      CONSTRAINT RunnableScripts_RunStatus FOREIGN KEY ( [RunStatusId] ) REFERENCES [dbo].[RunStatus] ( [RunStatusId] )
    );
GO

INSERT INTO dbo.RunStatus
        ( StatusName )
VALUES  ( N'Picked Up' ),
		( N'In Process'),
		( N'Completed' ),
		( N'Errored Out')
GO
-- Script to add missing columns to existing database
-- Run this if you want to keep existing data

USE ITEC275LiveQuiz;
GO

-- Check if columns exist, if not add them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'FullName')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [FullName] NVARCHAR(100) NULL;
    PRINT 'Added FullName column';
END
ELSE
BEGIN
    PRINT 'FullName column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Email')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Email] NVARCHAR(100) NULL;
    PRINT 'Added Email column';
END
ELSE
BEGIN
    PRINT 'Email column already exists';
END

-- Update demo user
UPDATE [dbo].[Users]
SET FullName = 'Demo User', Email = 'demo@example.com'
WHERE Username = 'demo' AND FullName IS NULL;

PRINT 'Database updated successfully!';
GO

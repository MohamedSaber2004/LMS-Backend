-- Help Center tables (HelpCategories + HelpArticles).
-- Regenerate with:
--   dotnet ef migrations script 20261103092630_AddInstructorAvailability AddHelpCenter
--     --project LMS.Persistence --startup-project LMS.API --context LmsDbContext
--     -o docs/plans/2026-11-03-help-center.sql
-- Prerequisite: docs/plans/2026-11-03-instructor-availability.sql was applied first.
-- This script is ranged from that migration, so it does not recreate those tables.
BEGIN TRANSACTION;
GO

CREATE TABLE [HelpCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Order] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [DeletedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [Version] rowversion NULL,
    CONSTRAINT [PK_HelpCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [HelpArticles] (
    [Id] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [Question] nvarchar(300) NOT NULL,
    [Answer] nvarchar(max) NOT NULL,
    [Order] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [DeletedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [Version] rowversion NULL,
    CONSTRAINT [PK_HelpArticles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HelpArticles_HelpCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [HelpCategories] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_HelpArticles_CategoryId] ON [HelpArticles] ([CategoryId]);
GO

CREATE INDEX [IX_HelpArticles_CategoryId_Order] ON [HelpArticles] ([CategoryId], [Order]);
GO

CREATE INDEX [IX_HelpArticles_IsDeleted] ON [HelpArticles] ([IsDeleted]);
GO

CREATE INDEX [IX_HelpCategories_IsDeleted] ON [HelpCategories] ([IsDeleted]);
GO

CREATE UNIQUE INDEX [IX_HelpCategories_Name] ON [HelpCategories] ([Name]) WHERE [IsDeleted] = 0;
GO

CREATE INDEX [IX_HelpCategories_Order] ON [HelpCategories] ([Order]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261103111527_AddHelpCenter', N'8.0.33');
GO

COMMIT;
GO


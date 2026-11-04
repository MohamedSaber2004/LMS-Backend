/* ============================================================================
   Scheduling (§1.2) — schema hand-off
   Migration : 20261104102902_AddSchedulingTables  (LmsDbContext)
   Date      : 2026-11-04
   Target    : shared SQL Server (Development and Production point at the same DB)

   Adds: Instructors.VacationMode, Countries.TimezoneId and the tables
   AvailabilityInstances, Sessions, SessionBookings, RescheduleRequests.

   Apply with `dotnet ef database update` (Qoder never runs it) OR the idempotent
   DDL below, then run docs/plans/2026-11-04-country-timezones.sql to fill the
   anchor, then docs/plans/2026-11-04-hangfire-schema.sql for Hangfire itself.
   ========================================================================== */

﻿BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    ALTER TABLE [Instructors] ADD [VacationMode] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    ALTER TABLE [Countries] ADD [TimezoneId] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE TABLE [AvailabilityInstances] (
        [Id] uniqueidentifier NOT NULL,
        [InstructorId] uniqueidentifier NOT NULL,
        [WeeklySlotId] uniqueidentifier NULL,
        [Date] date NOT NULL,
        [StartsAtUtc] datetime2 NOT NULL,
        [EndsAtUtc] datetime2 NOT NULL,
        [IsOpen] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Version] rowversion NULL,
        CONSTRAINT [PK_AvailabilityInstances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AvailabilityInstances_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE TABLE [Sessions] (
        [Id] uniqueidentifier NOT NULL,
        [InstructorId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NULL,
        [Type] int NOT NULL,
        [StartsAtUtc] datetime2 NOT NULL,
        [EndsAtUtc] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [Price] decimal(10,2) NOT NULL,
        [SeatCapacity] int NOT NULL,
        [SeatsBooked] int NOT NULL,
        [CancelledByUserId] uniqueidentifier NULL,
        [CancelReason] nvarchar(1000) NULL,
        [CancelledAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Version] rowversion NULL,
        CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sessions_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE TABLE [RescheduleRequests] (
        [Id] uniqueidentifier NOT NULL,
        [SessionId] uniqueidentifier NOT NULL,
        [RequestedByUserId] uniqueidentifier NOT NULL,
        [RequestedByType] int NOT NULL,
        [FromUtc] datetime2 NOT NULL,
        [ToUtc] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [RespondedByUserId] uniqueidentifier NULL,
        [RespondedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Version] rowversion NULL,
        CONSTRAINT [PK_RescheduleRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RescheduleRequests_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE TABLE [SessionBookings] (
        [Id] uniqueidentifier NOT NULL,
        [SessionId] uniqueidentifier NOT NULL,
        [LearnerId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [CancelledByUserId] uniqueidentifier NULL,
        [CancelReason] nvarchar(1000) NULL,
        [CancelledAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Version] rowversion NULL,
        CONSTRAINT [PK_SessionBookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionBookings_Learners_LearnerId] FOREIGN KEY ([LearnerId]) REFERENCES [Learners] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SessionBookings_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_Instructors_VacationMode] ON [Instructors] ([VacationMode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_AvailabilityInstances_InstructorId] ON [AvailabilityInstances] ([InstructorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_AvailabilityInstances_InstructorId_Date] ON [AvailabilityInstances] ([InstructorId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_AvailabilityInstances_IsDeleted] ON [AvailabilityInstances] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_AvailabilityInstances_StartsAtUtc_EndsAtUtc] ON [AvailabilityInstances] ([StartsAtUtc], [EndsAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_RescheduleRequests_IsDeleted] ON [RescheduleRequests] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_RescheduleRequests_RequestedByUserId] ON [RescheduleRequests] ([RequestedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_RescheduleRequests_SessionId] ON [RescheduleRequests] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_RescheduleRequests_SessionId_Status] ON [RescheduleRequests] ([SessionId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_SessionBookings_IsDeleted] ON [SessionBookings] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_SessionBookings_LearnerId] ON [SessionBookings] ([LearnerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionBookings_SessionId_LearnerId] ON [SessionBookings] ([SessionId], [LearnerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_SessionBookings_Status] ON [SessionBookings] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_Sessions_InstructorId] ON [Sessions] ([InstructorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_Sessions_InstructorId_StartsAtUtc] ON [Sessions] ([InstructorId], [StartsAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_Sessions_IsDeleted] ON [Sessions] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_Sessions_StartsAtUtc_EndsAtUtc] ON [Sessions] ([StartsAtUtc], [EndsAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    CREATE INDEX [IX_Sessions_Status] ON [Sessions] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104102902_AddSchedulingTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104102902_AddSchedulingTables', N'8.0.33');
END;
GO

COMMIT;
GO


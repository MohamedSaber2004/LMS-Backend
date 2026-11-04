BEGIN TRANSACTION;
GO

CREATE TABLE [InstructorAvailabilitySlots] (
    [Id] uniqueidentifier NOT NULL,
    [InstructorId] uniqueidentifier NOT NULL,
    [Day] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [DeletedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [Version] rowversion NULL,
    CONSTRAINT [PK_InstructorAvailabilitySlots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InstructorAvailabilitySlots_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_InstructorAvailabilitySlots_InstructorId] ON [InstructorAvailabilitySlots] ([InstructorId]);
GO

CREATE INDEX [IX_InstructorAvailabilitySlots_InstructorId_Day_StartTime] ON [InstructorAvailabilitySlots] ([InstructorId], [Day], [StartTime]);
GO

CREATE INDEX [IX_InstructorAvailabilitySlots_IsDeleted] ON [InstructorAvailabilitySlots] ([IsDeleted]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261103092630_AddInstructorAvailability', N'8.0.33');
GO

COMMIT;
GO


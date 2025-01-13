IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'marin2')
BEGIN
    CREATE DATABASE marin2;
END;
GO

USE marin2;
GO

CREATE TABLE [AccountType] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_AccountType] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AppUserLoginHistory] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [UtcLogIn] datetime2 NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [OS] nvarchar(50) NOT NULL,
    [IpAddress] nvarchar(50) NOT NULL,
    [Browser] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_AppUserLoginHistory] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Role] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Account] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [AccountTypeId] decimal(20,0) NOT NULL,
    [Owner] nvarchar(100) NOT NULL,
    [Name] nvarchar(100) NULL,
    [UtcExpirationDate] datetime2 NULL,
    [Description] nvarchar(250) NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Account_AccountType_AccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountType] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [InviteToApplication] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [Email] nvarchar(255) NOT NULL,
    [AccountTypeId] decimal(20,0) NOT NULL,
    [UtcExpiration] datetime2 NULL,
    [UserMessage] nvarchar(1024) NULL,
    [UtcAcceptedOn] datetime2 NULL,
    [UtcRejectedOn] datetime2 NULL,
    [RejectedReason] nvarchar(512) NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_InviteToApplication] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InviteToApplication_AccountType_AccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountType] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AppConnection] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [ProviderName] nvarchar(100) NOT NULL,
    [OwnerEmail] nvarchar(255) NOT NULL,
    [ConnectionEmail] nvarchar(255) NULL,
    [AccountId] decimal(20,0) NOT NULL,
    [AccessToken] nvarchar(max) NOT NULL,
    [RefreshToken] nvarchar(max) NULL,
    [Scope] nvarchar(max) NULL,
    [UtcIssuedOn] datetime2 NOT NULL,
    [DurationInSeconds] bigint NOT NULL,
    [TokenType] nvarchar(50) NULL,
    [TokenId] nvarchar(1000) NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UtcUpdatedOn] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_AppConnection] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AppConnection_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AppUser] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [DisplayName] nvarchar(255) NULL,
    [Email] nvarchar(255) NOT NULL,
    [ProviderKey] nvarchar(255) NOT NULL,
    [ProviderType] nvarchar(50) NOT NULL,
    [ProfilePictureUrl] nvarchar(500) NULL,
    [OAuthAccessToken] nvarchar(500) NULL,
    [OAuthTokenType] nvarchar(50) NULL,
    [OAuthRefreshToken] nvarchar(250) NULL,
    [OAuthTokenUtcExpiresAt] datetime2 NULL,
    [UtcActiveUntil] datetime2 NULL,
    [UtcLastLogin] datetime2 NULL,
    [Timezone] nvarchar(100) NULL,
    [Metadata] nvarchar(max) NULL,
    [AccountId] decimal(20,0) NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_AppUser] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AppUser_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [InviteToAccount] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [AccountId] decimal(20,0) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [UtcExpiration] datetime2 NOT NULL,
    [UserMessage] nvarchar(1024) NULL,
    [UtcAcceptedOn] datetime2 NULL,
    [UtcRejectedOn] datetime2 NULL,
    [RejectedReason] nvarchar(512) NULL,
    [RoleId] decimal(20,0) NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_InviteToAccount] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InviteToAccount_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InviteToAccount_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AppUserRole] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [AppUserId] decimal(20,0) NOT NULL,
    [RoleId] decimal(20,0) NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_AppUserRole] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AppUserRole_AppUser_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AppUser] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AppUserRole_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [RefreshToken] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [AppUserId] decimal(20,0) NOT NULL,
    [Token] nvarchar(500) NOT NULL,
    [DurationInSeconds] decimal(20,0) NOT NULL,
    [UtcExpiresOn] datetime2 NOT NULL,
    [IsValid] bit NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UtcUpdatedOn] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_RefreshToken] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshToken_AppUser_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AppUser] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_Account_AccountTypeId] ON [Account] ([AccountTypeId]);
GO


CREATE UNIQUE INDEX [IX_Account_Name] ON [Account] ([Name]) WHERE [Name] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_Account_Owner] ON [Account] ([Owner]);
GO


CREATE UNIQUE INDEX [IX_AccountType_Name] ON [AccountType] ([Name]);
GO


CREATE INDEX [IX_AppConnection_AccountId] ON [AppConnection] ([AccountId]);
GO


CREATE UNIQUE INDEX [IX_AppConnection_ProviderName_OwnerEmail] ON [AppConnection] ([ProviderName], [OwnerEmail]);
GO


CREATE INDEX [IX_AppUser_AccountId] ON [AppUser] ([AccountId]);
GO


CREATE UNIQUE INDEX [IX_AppUser_Email] ON [AppUser] ([Email]);
GO


CREATE INDEX [IX_AppUserRole_AppUserId] ON [AppUserRole] ([AppUserId]);
GO


CREATE INDEX [IX_AppUserRole_RoleId] ON [AppUserRole] ([RoleId]);
GO


CREATE INDEX [IX_InviteToAccount_AccountId] ON [InviteToAccount] ([AccountId]);
GO


CREATE INDEX [IX_InviteToAccount_RoleId] ON [InviteToAccount] ([RoleId]);
GO


CREATE INDEX [IX_InviteToApplication_AccountTypeId] ON [InviteToApplication] ([AccountTypeId]);
GO


CREATE UNIQUE INDEX [IX_InviteToApplication_Email] ON [InviteToApplication] ([Email]);
GO


CREATE INDEX [IX_RefreshToken_AppUserId] ON [RefreshToken] ([AppUserId]);
GO


CREATE UNIQUE INDEX [IX_RefreshToken_Token] ON [RefreshToken] ([Token]);
GO



CREATE TABLE [GenAIBot] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [AccountId] decimal(20,0) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NULL,
    [ImageUrl] nvarchar(500) NULL,
    [SystemPrompt] nvarchar(max) NULL,
    [SafetyPrompt] nvarchar(max) NULL,
    [SystemColor] nvarchar(25) NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UtcUpdatedOn] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_GenAIBot] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ChatSession] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [GenAIBotId] decimal(20,0) NOT NULL,
    [ChatReference] nvarchar(450) NULL,
    [Title] nvarchar(150) NOT NULL,
    [CanShare] bit NOT NULL,
    [HasMedia] bit NOT NULL,
    [IsArchived] bit NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UtcUpdatedOn] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_ChatSession] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatSession_GenAIBot_GenAIBotId] FOREIGN KEY ([GenAIBotId]) REFERENCES [GenAIBot] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ChatMessage] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [ChatSessionId] decimal(20,0) NOT NULL,
    [UserMessage] nvarchar(max) NOT NULL,
    [AgentResponse] nvarchar(max) NOT NULL,
    [AgentResponseMediaUrl] nvarchar(max) NULL,
    [Model] nvarchar(max) NULL,
    [ProviderName] nvarchar(max) NULL,
    [InputTokens] int NOT NULL,
    [OutputTokens] int NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UtcUpdatedOn] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_ChatMessage] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessage_ChatSession_ChatSessionId] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSession] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ChatMessageMedia] (
    [Id] decimal(20,0) NOT NULL IDENTITY,
    [ChatMessageId] decimal(20,0) NOT NULL,
    [MediaUrl] nvarchar(500) NOT NULL,
    [ProviderFileName] nvarchar(500) NULL,
    [FileName] nvarchar(500) NULL,
    [ContentType] nvarchar(50) NULL,
    [ContentMD5] nvarchar(32) NULL,
    [ProviderName] nvarchar(50) NOT NULL,
    [UtcCreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UtcUpdatedOn] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [Version] bigint NOT NULL,
    CONSTRAINT [PK_ChatMessageMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessageMedia_ChatMessage_ChatMessageId] FOREIGN KEY ([ChatMessageId]) REFERENCES [ChatMessage] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_ChatMessage_ChatSessionId] ON [ChatMessage] ([ChatSessionId]);
GO


CREATE INDEX [IX_ChatMessageMedia_ChatMessageId] ON [ChatMessageMedia] ([ChatMessageId]);
GO


CREATE INDEX [IX_ChatSession_ChatReference] ON [ChatSession] ([ChatReference]);
GO


CREATE INDEX [IX_ChatSession_GenAIBotId] ON [ChatSession] ([GenAIBotId]);
GO


CREATE INDEX [IX_GenAIBot_AccountId] ON [GenAIBot] ([AccountId]);
GO


CREATE UNIQUE INDEX [IX_GenAIBot_AccountId_Name] ON [GenAIBot] ([AccountId], [Name]);
GO




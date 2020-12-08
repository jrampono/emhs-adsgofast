/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
CREATE TABLE [dbo].[DataFactory] (
    [Id]                      BIGINT           IDENTITY (1, 1) NOT NULL,
    [Name]                    VARCHAR (255)    NULL,
    [ResourceGroup]           VARCHAR (255)    NULL,
    [SubscriptionUid]         UNIQUEIDENTIFIER NULL,
    [DefaultKeyVaultURL]      VARCHAR (255)    NULL,
    [LogAnalyticsWorkspaceId] UNIQUEIDENTIFIER NULL
);






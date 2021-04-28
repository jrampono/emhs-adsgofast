/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Rest;

namespace AdsGoFast
{
    public partial class Shared
    {

        public static partial class Azure
        {
            public static partial class DataFactory
            {
                internal class DataFactoryClient
                {
                    public static DataFactoryManagementClient CreateDataFactoryClient(string SubscriptionId)
                    {
                        string token = Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken("https://management.azure.com/");
                        ServiceClientCredentials cred = new TokenCredentials(token);

                        DataFactoryManagementClient adfClient = new DataFactoryManagementClient(cred)
                        {
                            SubscriptionId = SubscriptionId
                        };

                        return adfClient;
                    }
                }
            }
        }
    }
}

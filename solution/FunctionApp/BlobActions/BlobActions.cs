/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AdsGoFast
{
    public static class JsonBlob
    {
        /* 
           Helper function to facilitate archival of blob folders
           Note: Path Parameter super picky about slashes: eg. unprocessed/usage/20190615/ 

        */
       
        public static string JsonBlobCore(HttpRequest req, Logging logging)
        {

            string reqbody = new StreamReader(req.Body).ReadToEndAsync().Result;
            string BlobStorageAccountName;
            string BlobStorageContainerName;
            string BlobStorageFolderPath;

            BlobStorageAccountName = Shared.GlobalConfigs.GetStringRequestParam("BlobStorageAccountName", req, reqbody).ToString();
            BlobStorageContainerName = Shared.GlobalConfigs.GetStringRequestParam("BlobStorageContainerName", req, reqbody).ToString();
            BlobStorageFolderPath = Shared.GlobalConfigs.GetStringRequestParam("BlobStorageFolderPath", req, reqbody).ToString();

            TokenCredential StorageToken = new TokenCredential(Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken(string.Format("https://storage.azure.com/", BlobStorageAccountName), Shared._ApplicationOptions.UseMSI));

            string TargetFileName = Shared.GlobalConfigs.GetStringRequestParam("TargetFileName", req, reqbody).ToString();
            string DownloadResult = Shared.Azure.Storage.ReadFile(BlobStorageAccountName, BlobStorageContainerName, BlobStorageFolderPath, TargetFileName, StorageToken);
            return DownloadResult;
        }

    }




}

/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/


using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AdsGoFast.Services
{    
    public interface ISecurityAccessProvider
    {
        public bool IsAuthorised(HttpRequest req, ILogger log);
    }
  
}

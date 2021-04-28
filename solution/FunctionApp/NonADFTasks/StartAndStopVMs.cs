/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdsGoFast
{
    public static class StartAndStopVMs
    {
        [FunctionName("StartAndStopVMs")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context, System.Security.Claims.ClaimsPrincipal principal)
        {
            bool Allowed = false;
            var roles = principal.Claims.Where(e => e.Type == "roles").Select(e => e.Value);

            foreach (string r in roles)
            {
                if (r == "All") { Allowed = true; }
            }

            if (!Allowed && !Shared._ApplicationOptions.ServiceConnections.CoreFunctionsURL.Contains("localhost"))
            {
                string err = "Request was rejected as user is not allowed to perform this action";
                log.LogError(err);
                return new BadRequestObjectResult(new { Error = err });
            }

            System.Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = StartAndStopVMsCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "StartAndStopVMs", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }
        }


        public static async Task<JObject> StartAndStopVMsCore(HttpRequest req, Logging logging)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);
            string _TaskInstanceId = data["TaskInstanceId"].ToString();
            string _ExecutionUid = data["ExecutionUid"].ToString();
            try
            {
                logging.LogInformation("StartAndStopVMs function processed a request.");
                string Subscription = data["Target"]["SubscriptionUid"].ToString();
                string VmName = data["Target"]["VMname"].ToString();
                string VmResourceGroup = data["Target"]["ResourceGroup"].ToString();
                string VmAction = data["Target"]["Action"].ToString();

                Microsoft.Azure.Management.Fluent.Azure.IAuthenticated azureAuth = Microsoft.Azure.Management.Fluent.Azure.Configure().WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders).Authenticate(Shared.Azure.AzureSDK.GetAzureCreds(Shared._ApplicationOptions.UseMSI));
                IAzure azure = azureAuth.WithSubscription(Subscription);
                logging.LogInformation("Selected subscription: " + azure.SubscriptionId);
                IVirtualMachine vm = azure.VirtualMachines.GetByResourceGroup(VmResourceGroup, VmName);
                if (vm.PowerState == Microsoft.Azure.Management.Compute.Fluent.PowerState.Deallocated && VmAction.ToLower() == "start")
                {
                    logging.LogInformation("VM State is: " + vm.PowerState.Value.ToString());
                    vm.StartAsync().Wait(5000);
                    logging.LogInformation("VM Start Initiated: " + vm.Name);
                }

                if (vm.PowerState != Microsoft.Azure.Management.Compute.Fluent.PowerState.Deallocated && VmAction.ToLower() == "stop")
                {
                    logging.LogInformation("VM State is: " + vm.PowerState.Value.ToString());
                    vm.DeallocateAsync().Wait(5000);
                    logging.LogInformation("VM Stop Initiated: " + vm.Name);
                }

                JObject Root = new JObject { ["Result"] = "Complete" };

                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                if (VmName != null)
                {   Root["Result"] = "Complete";
                   
                }
                else
                { 
                    Root["Result"] = "Please pass a name, resourcegroup and action to request body";
                    TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.FailedRetry, System.Guid.Empty, "Task missing VMname, ResourceGroup or SubscriptionUid in Target element.");
                    return Root;
                }
                //Update Task Instance
                
                TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.Complete, System.Guid.Empty, "");

                return Root;
            }
            catch (System.Exception TaskException)
            {
                logging.LogErrors(TaskException);
                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.FailedRetry, System.Guid.Empty, "Failed when trying to start or stop VM");

                JObject Root = new JObject
                {
                    ["Result"] = "Failed"
                };

                return Root;

            }
        }



    }
}

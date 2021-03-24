/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AdsGoFast
{

    public class FrameworkRunner : IDisposable
    {
        private Logging LogHelper { get; set; }

        public FrameworkRunner(ILogger log, Guid _ExecutionUid)
        {
            LogHelper = new Logging();
            Logging.ActivityLogItem activityLogItem = new Logging.ActivityLogItem
            {
                LogSource = "AF",
                ExecutionUid = _ExecutionUid
            };
            LogHelper.InitializeLog(log, activityLogItem);

        }

        public FrameworkRunnerResult Invoke(string CallingMethodName, FrameworkRunnerWorker WorkerFunction)
        {
            FrameworkRunnerResult r = FrameworkRunnerCore(null, CallingMethodName, WorkerFunction, null);
            return r;
        }

        public FrameworkRunnerResult Invoke(HttpRequest req, string CallingMethodName, FrameworkRunnerWorkerWithHttpRequest WorkerFunction)
        {
            FrameworkRunnerResult r = FrameworkRunnerCore(req, CallingMethodName, null, WorkerFunction);
            return r;

        }

        private FrameworkRunnerResult FrameworkRunnerCore(HttpRequest req, string CallingMethodName, FrameworkRunnerWorker WorkerFunction, FrameworkRunnerWorkerWithHttpRequest WorkerFunctionWithHttp)
        {
            LogHelper.DefaultActivityLogItem.StartDateTimeOffset = DateTimeOffset.UtcNow;
            LogHelper.DefaultActivityLogItem.EndDateTimeOffset = DateTimeOffset.UtcNow;
            FrameworkRunnerResult r = new FrameworkRunnerResult();
            LogHelper.DefaultActivityLogItem.ActivityType = CallingMethodName;

            if (req != null)
            {
                if (req.Body != null)
                {
                    req.EnableBuffering();

                    // Leave the body open so the next middleware can read it.
                    using System.IO.StreamReader reader = new System.IO.StreamReader(req.Body, encoding: System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                    string requestBody = reader.ReadToEndAsync().Result;
                    // Reset the request body stream position so the next middleware can read it
                    req.Body.Position = 0;
                    if (requestBody.Length > 0)
                    {
                        dynamic data = JsonConvert.DeserializeObject(requestBody);

                        LogHelper.DefaultActivityLogItem.TaskInstanceId = System.Convert.ToInt64(Shared.JsonHelpers.GetDynamicValueFromJSON(LogHelper, "TaskInstanceId", data, null, false));

                        LogHelper.DefaultActivityLogItem.AdfRunUid = Guid.Parse(Shared.JsonHelpers.GetDynamicValueFromJSON(LogHelper, "AdfRunUid", data, "00000000-0000-0000-0000-000000000000", false));

                        if (LogHelper.DefaultActivityLogItem.AdfRunUid == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                        {
                            LogHelper.DefaultActivityLogItem.AdfRunUid = Guid.Parse(Shared.JsonHelpers.GetDynamicValueFromJSON(LogHelper, "RunId", data, "00000000-0000-0000-0000-000000000000", false));
                        }

                        LogHelper.DefaultActivityLogItem.ExecutionUid = Guid.Parse(Shared.JsonHelpers.GetDynamicValueFromJSON(LogHelper, "ExecutionUid", data, "00000000-0000-0000-0000-000000000000", false));


                    }

                }
            }

            LogHelper.LogInformation(string.Format("Azure Function '{0}' started.", CallingMethodName));
            try
            {
                if (WorkerFunctionWithHttp == null)
                {
                    dynamic result = WorkerFunction.Invoke(LogHelper);
                    if (result.GetType().FullName.Contains("Task"))
                    {
                        r.ReturnObject = JsonConvert.SerializeObject(result.Result).ToString();
                    }
                    else
                    {
                        r.ReturnObject = JsonConvert.SerializeObject(result).ToString();
                    }
                }
                else
                {
                    dynamic result = WorkerFunctionWithHttp.Invoke(req, LogHelper);
                    if (result.GetType().FullName.Contains("Task"))
                    {
                        r.ReturnObject = JsonConvert.SerializeObject(result.Result).ToString();
                    }
                    else
                    {
                        r.ReturnObject = JsonConvert.SerializeObject(result).ToString();
                    }
                }
                LogHelper.LogInformation(string.Format("Azure Function '{0}' finished.", CallingMethodName));
                EndProcessAndPersistLog(CallingMethodName);
                r.Succeeded = true;
                return r;

            }
            catch (Exception e)
            {
                LogHelper.LogErrors(new Exception(string.Format("Azure Function '{0}' finished with Errors.", CallingMethodName)));
                LogHelper.LogErrors(e);
                EndProcessAndPersistLog(CallingMethodName);
                r.Succeeded = false;
                r.ReturnObject = JsonConvert.SerializeObject(new { }).ToString();
                return r;
            }

        }

        public void EndProcessAndPersistLog(string CallingMethodName)
        {
            LogHelper.DefaultActivityLogItem.EndDateTimeOffset = DateTimeOffset.UtcNow;
            TimeSpan Duration = LogHelper.DefaultActivityLogItem.EndDateTimeOffset - LogHelper.DefaultActivityLogItem.StartDateTimeOffset;
            LogHelper.LogPerformance(string.Format("Azure Function '{0}' - Execution Time: {1} seconds.", CallingMethodName, Duration.TotalSeconds.ToString()));
            LogHelper.PersistLogToDatabase();

        }

        public delegate dynamic FrameworkRunnerWorker(Logging LogHelper1);

        public delegate dynamic FrameworkRunnerWorkerWithHttpRequest(HttpRequest req, Logging LogHelper);

        

        public class FrameworkRunnerResult
        {
            public bool Succeeded { get; set; }
            public string ReturnObject { get; set; }
        }

        public void Dispose()
        {
            //ToDO: Add Disposal Clean-up
        }

    }
}

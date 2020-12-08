using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class TaskInstanceController : BaseController
    {
        public async Task<IActionResult> IndexDataTable()
        {
            return View();
        }

        public JObject GridCols()
        {
            JObject GridOptions = new JObject();

            JArray cols = new JArray();
            cols.Add(JObject.Parse("{ 'data':'TaskInstanceId', 'name':'TaskInstanceId', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMaster.TaskMasterName', 'name':'TaskMaster', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'ScheduleInstance.ScheduledDateTimeOffset', 'name':'Scheduled', 'autoWidth':true,'ads_format': 'datetime' }"));
            cols.Add(JObject.Parse("{ 'data':'UpdatedOn', 'name':'LastUpdated', 'autoWidth':true,'ads_format': 'datetime' }"));
            cols.Add(JObject.Parse("{ 'data':'LastExecutionStatus', 'name':'Status', 'autoWidth':true, 'ads_format':'taskstatus' }"));
            cols.Add(JObject.Parse("{ 'data':'LastExecutionComment', 'name':'Comment', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'NumberOfRetries', 'name':'Retries', 'autoWidth':true }"));

            HumanizeColumns(cols);

            JArray pkeycols = new JArray();
            pkeycols.Add("TaskInstanceId");

            JArray Navigations = new JArray();
            //Navigations.Add(JObject.Parse("{'Url':'/TaskInstanceExecution?&TaskInstanceId=','Description':'View Task Instance Executions', 'Icon':'bolt','ButtonClass':'btn-primary'}"));
            Navigations.Add(JObject.Parse("{'Url':'/TaskInstanceExecution/IndexDataTable?&TaskInstanceId=','Description':'View Task Instance Executions', 'Icon':'bolt','ButtonClass':'btn-primary'}"));

            GridOptions["GridColumns"] = cols;
            GridOptions["ModelName"] = "TaskMaster";
            GridOptions["PrimaryKeyColumns"] = pkeycols;
            GridOptions["Navigations"] = Navigations;
            GridOptions["InitialOrder"] = JArray.Parse("[[2,'desc']]");
            GridOptions["SuppressCrudButtons"] = true;

            return GridOptions;
        }

        public ActionResult GetGridOptions()
        {
            return new OkObjectResult(JsonConvert.SerializeObject(GridCols()));
        }

        public ActionResult GetGridData()
        {
            try
            {

                Int16 draw = System.Convert.ToInt16(Request.Form["draw"]);
                string start = Request.Form["start"];
                string length = Request.Form["length"];
                string sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"] + "][data]"];
                string sortColumnDir = Request.Form["order[0][dir]"];

                string searchValue = Request.Form["search[value]"];

                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Customer data    
                var modelDataAll = (from temptable in _context.TaskInstance
                                    select temptable);



                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    modelDataAll = modelDataAll.OrderBy(sortColumn + " " + sortColumnDir);
                }
                


                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    modelDataAll = modelDataAll.Where(m => m.TaskMaster.TaskMasterName.Contains(searchValue));
                }

                //Filter based on querystring params
                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[TaskMasterId]"])))
                {
                    var tasktypefilter = System.Convert.ToInt64(Request.Form["QueryParams[TaskMasterId]"]);
                    modelDataAll = modelDataAll.Where(t => t.TaskMasterId == tasktypefilter);
                }

                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[TaskInstanceId]"])))
                {
                    var filter = System.Convert.ToInt64(Request.Form["QueryParams[TaskInstanceId]"]);
                    modelDataAll = modelDataAll.Where(t => t.TaskInstanceId == filter);
                }

                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[ScheduleInstanceId]"])))
                {
                    var filter = System.Convert.ToInt64(Request.Form["QueryParams[ScheduleInstanceId]"]);
                    modelDataAll = modelDataAll.Where(t => t.ScheduleInstanceId == filter);
                }

                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[LastExecutionStatus]"])))
                {
                    var filter = System.Convert.ToString(Request.Form["QueryParams[LastExecutionStatus]"]);
                    if (filter.ToLower().StartsWith("failed"))
                    {
                        modelDataAll = modelDataAll.Where(t => t.LastExecutionStatus == TaskExecutionStatus.FailedNoRetry || t.LastExecutionStatus == TaskExecutionStatus.FailedRetry);
                    }
                    if (filter.ToLower() == "complete")
                    {
                        modelDataAll = modelDataAll.Where(t => t.LastExecutionStatus == TaskExecutionStatus.Complete);
                    }
                    if (filter.ToLower() == "untried")
                    {
                        modelDataAll = modelDataAll.Where(t => t.LastExecutionStatus == TaskExecutionStatus.Untried);
                    }
                    if (filter.ToLower() == "inprogress")
                    {
                        modelDataAll = modelDataAll.Where(t => t.LastExecutionStatus == TaskExecutionStatus.InProgress);
                    }
                }


                //Custom Includes
                modelDataAll = modelDataAll
                    .Include(t => t.TaskMaster)
                    .Include(t => t.ScheduleInstance)
                    .AsNoTracking();



                //total number of rows count     
                recordsTotal = modelDataAll.Count();
                //Paging               
                var data = modelDataAll.Skip(skip).Take(pageSize).ToList();
                //Returning Json Data    
                var jserl = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };

                return new OkObjectResult(JsonConvert.SerializeObject(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, jserl));

            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult UpdateTaskInstanceStatus()
        {
            List<Int64> Pkeys = JsonConvert.DeserializeObject<List<Int64>>(Request.Form["Pkeys"]);
            string Status = Request.Form["Status"];
            var entitys = _context.TaskInstance.Where(ti => Pkeys.Contains(ti.TaskInstanceId));            
            entitys.ForEachAsync(ti => 
                {
                    ti.LastExecutionStatus = (TaskExecutionStatus)System.Enum.Parse(typeof(TaskExecutionStatus), Status);
                    ti.LastExecutionComment = "Manually Updated Status to " + Status + " using WebApp";
                    ti.NumberOfRetries = 0;
                    if (Status != "InProgress") { ti.TaskRunnerId = null; }
                }).Wait();
            _context.SaveChanges();

            //TODO: Add Error Handling
            return new OkObjectResult(new { });
        }

    }


}

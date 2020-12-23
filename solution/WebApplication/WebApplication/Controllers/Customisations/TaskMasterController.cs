using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Controllers.Customisations;
using WebApplication.Framework;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public partial class TaskMasterController : BaseController
    {
        [ChecksUserAccess]
        public async Task<IActionResult> IndexDataTable()
        {
            var adsGoFastContext = _context.TaskMaster.Take(1);
            return View(await adsGoFastContext.ToListAsync());
        }

        public JObject GridCols()
        {
            JObject GridOptions = new JObject();

            JArray cols = new JArray();
            cols.Add(JObject.Parse("{ 'data':'TaskMasterId', 'name':'Id' }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMasterName', 'name':'Name', }"));
            cols.Add(JObject.Parse("{ 'data':'TaskType.TaskTypeName', 'name':'Task Type'}"));
            cols.Add(JObject.Parse("{ 'data':'ScheduleMaster.ScheduleDesciption', 'name':'Schedule'}"));
            cols.Add(JObject.Parse("{ 'data':'SourceSystem.SystemName', 'name':'Source' }"));
            cols.Add(JObject.Parse("{ 'data':'TargetSystem.SystemName', 'name':'Target' }"));
            cols.Add(JObject.Parse("{ 'data':'TaskDatafactoryIr', 'name':'Data Factory IR'}"));
            cols.Add(JObject.Parse("{ 'data':'ActiveYn', 'name':'ActiveYn', 'autoWidth':true, 'ads_format': 'bool' }"));

            HumanizeColumns(cols);

            JArray pkeycols = new JArray();
            pkeycols.Add("TaskMasterId");

            JArray Navigations = new JArray();
            Navigations.Add(JObject.Parse("{'Url':'/TaskInstance/IndexDataTable?TaskMasterId=','Description':'View Task Instances', 'Icon':'list-alt','ButtonClass':'btn-primary'}"));
            Navigations.Add(JObject.Parse("{'Url':'/TaskMasterWaterMark/IndexDataTable?TaskMasterId=','Description':'View Water Mark', 'Icon':'water','ButtonClass':'btn-primary'}"));

            GridOptions["GridColumns"] = cols;
            GridOptions["ModelName"] = "TaskMaster";
            GridOptions["PrimaryKeyColumns"] = pkeycols;
            GridOptions["Navigations"] = Navigations;
            GridOptions["AutoWidth"] = false;
            GridOptions["CrudButtons"] = GetSecurityFilteredActions("Create,EditPlus,Details,Delete");

            return GridOptions;
        }


        [ChecksUserAccess]
        public ActionResult GetGridOptions()
        {
            return new OkObjectResult(JsonConvert.SerializeObject(GridCols()));
        }

        [ChecksUserAccess]
        public ActionResult GetGridData()
        {
            try
            {

                string draw = Request.Form["draw"];
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
                var modelDataAll = (from temptable in _context.TaskMaster
                                    select temptable);

                //filter the list by permitted roles
                if (!CanPerformCurrentActionGlobally())
                {
                    var permittedRoles = GetPermittedGroupsForCurrentAction();
                    var identity = User.Identity.Name;

                    modelDataAll =
                        (from md in modelDataAll
                         join tg in _context.TaskGroup
                            on md.TaskGroupId equals tg.TaskGroupId
                         join rm in _context.SubjectAreaRoleMap
                            on tg.SubjectAreaId equals rm.SubjectAreaId
                         where
                             GetUserAdGroupUids().Contains(rm.AadGroupUid)
                             && permittedRoles.Contains(rm.ApplicationRoleName)
                             && rm.ExpiryDate > DateTimeOffset.Now
                             && rm.ActiveYn
                         select md).Distinct();
                }



                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    modelDataAll = modelDataAll.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    modelDataAll = modelDataAll.Where(m => m.TaskMasterName.Contains(searchValue));
                }

                //Filter based on querystring params
                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[TaskTypeId]"])))
                {
                    var tasktypefilter = System.Convert.ToInt64(Request.Form["QueryParams[TaskTypeId]"]);
                    modelDataAll = modelDataAll.Where(t => t.TaskTypeId == tasktypefilter);
                }

                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[TaskGroupId]"])))
                {
                    var filter = System.Convert.ToInt64(Request.Form["QueryParams[TaskGroupId]"]);
                    modelDataAll = modelDataAll.Where(t => t.TaskGroupId == filter);
                }

                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[ScheduleMasterId]"])))
                {
                    var filter = System.Convert.ToInt64(Request.Form["QueryParams[ScheduleMasterId]"]);
                    modelDataAll = modelDataAll.Where(t => t.ScheduleMasterId == filter);
                }

                //Custom Includes
                modelDataAll = modelDataAll
                    .Include(t => t.TaskGroup)
                    .Include(t => t.ScheduleMaster)
                    .Include(t => t.SourceSystem)
                    .Include(t => t.TargetSystem)
                    .Include(t => t.TaskType).AsNoTracking();


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


        [ChecksUserAccess]
        public async Task<IActionResult> CopyTaskMaster()
        {
            List<Int64> Pkeys = JsonConvert.DeserializeObject<List<Int64>>(Request.Form["Pkeys"]);
            var entitys = await _context.TaskMaster.Where(ti => Pkeys.Contains(ti.TaskMasterId)).AsNoTracking().ToArrayAsync();

            foreach (var tm in entitys)
            {
                if (!await CanPerformCurrentActionOnRecord(tm))
                    return Forbid();

                tm.TaskMasterId = 0;
                tm.TaskMasterName = tm.TaskMasterName + " Copy";
                _context.Add(tm);
            }
            _context.SaveChanges();

            //TODO: Add Error Handling
            return new OkObjectResult(new { });
        }

        [ChecksUserAccess]
        public async Task<IActionResult> UpdateTaskMasterActiveYN()
        {
            List<Int64> Pkeys = JsonConvert.DeserializeObject<List<Int64>>(Request.Form["Pkeys"]);
            bool Status = JsonConvert.DeserializeObject<bool>(Request.Form["Status"]);
            var entitys = await _context.TaskMaster.Where(ti => Pkeys.Contains(ti.TaskMasterId)).ToArrayAsync();

            foreach (var ti in entitys)
            {
                if (!await CanPerformCurrentActionOnRecord(ti))
                    return Forbid();

                ti.ActiveYn = Status;
            }
            _context.SaveChanges();

            //TODO: Add Error Handling
            return new OkObjectResult(new { });
        }

        [ChecksUserAccess]
        public async Task<IActionResult> EditPlus(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMaster = await _context.TaskMaster.FindAsync(id);

            if (taskMaster == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return Forbid();

            ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskMaster.TaskGroupId);
            ViewData["TaskTypeId"] = new SelectList(_context.TaskType, "TaskTypeId", "TaskTypeName", taskMaster.TaskTypeId);
            ViewData["SourceSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(t => t.SystemName), "SystemId", "SystemName", taskMaster.SourceSystemId);
            ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(t => t.SystemName), "SystemId", "SystemName", taskMaster.TargetSystemId);
            ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(t => t.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", taskMaster.ScheduleMasterId);
            return View(taskMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> EditPlus(long id, [Bind("TaskMasterId,TaskMasterName,TaskTypeId,TaskGroupId,ScheduleMasterId,SourceSystemId,TargetSystemId,DegreeOfCopyParallelism,AllowMultipleActiveInstances,TaskDatafactoryIr,TaskMasterJson,ActiveYn,DependencyChainTag,DataFactoryId")] TaskMaster taskMaster)
        {
            if (id != taskMaster.TaskMasterId)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskMasterExists(taskMaster.TaskMasterId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexDataTable));
            }
            ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster, "ScheduleMasterId", "ScheduleCronExpression", taskMaster.ScheduleMasterId);
            ViewData["SourceSystemId"] = new SelectList(_context.SourceAndTargetSystems, "SystemId", "SystemAuthType", taskMaster.SourceSystemId);
            ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems, "SystemId", "SystemAuthType", taskMaster.TargetSystemId);
            ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup, "TaskGroupId", "TaskGroupName", taskMaster.TaskGroupId);
            ViewData["TaskTypeId"] = new SelectList(_context.TaskType, "TaskTypeId", "TaskTypeName", taskMaster.TaskTypeId);
            return View(taskMaster);
        }

    }


}

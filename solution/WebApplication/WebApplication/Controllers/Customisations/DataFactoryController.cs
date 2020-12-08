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
    public partial class DataFactoryController : BaseController
    {
        public async Task<IActionResult> IndexDataTable()
        {
            return View();
        }

        public JObject GridCols()
        {
        JObject GridOptions = new JObject();

            JArray cols = new JArray();
            cols.Add(JObject.Parse("{ 'data':'Id', 'name':'Id', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'Name', name:'Name', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'ResourceGroup', 'name':'Resource Group', 'autoWidth':true, 'width':'30%' }"));
            cols.Add(JObject.Parse("{ 'data':'SubscriptionUid', 'name':'Subscription', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'DefaultKeyVaultUrl', 'name':'Default KeyVault Url', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'LogAnalyticsWorkspaceId', 'name':'LogAnalytics Workspace', 'autoWidth':true }"));

            HumanizeColumns(cols);

            JArray pkeycols = new JArray();
            pkeycols.Add("Id");

            JArray Navigations = new JArray();
            
            GridOptions["GridColumns"] = cols;
            GridOptions["ModelName"] = "DataFactory";
            GridOptions["PrimaryKeyColumns"] = pkeycols;
            GridOptions["Navigations"] = Navigations;
            GridOptions["CrudButtons"] = GetSecurityFilteredActions("Create,Edit,Details,Delete");

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
                var modelDataAll = (from temptable in _context.DataFactory
                                    select temptable);

                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    modelDataAll = modelDataAll.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    modelDataAll = modelDataAll.Where(m => m.Name.Contains(searchValue) 
                    || m.ResourceGroup.Contains(searchValue)
                    || (m.SubscriptionUid != null && m.SubscriptionUid.ToString().Contains(searchValue))
                    || (m.LogAnalyticsWorkspaceId != null && m.LogAnalyticsWorkspaceId.ToString().Contains(searchValue)));
                }

                //Custom Includes
                modelDataAll = modelDataAll
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


        public ActionResult UpdateTaskMasterActiveYN()
        {
            List<Int64> Pkeys = JsonConvert.DeserializeObject<List<Int64>>(Request.Form["Pkeys"]);
            bool Status = JsonConvert.DeserializeObject<bool>(Request.Form["Status"]);
            var entitys = _context.TaskMasterWaterMark.Where(ti => Pkeys.Contains(ti.TaskMasterId));

            entitys.ForEachAsync(ti =>
            {
                ti.ActiveYn = Status;
            }).Wait();
            _context.SaveChanges();

            //TODO: Add Error Handling
            return new OkObjectResult(new { });
        }

        public async Task<IActionResult> EditPlus(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMaster = await _context.TaskMaster.FindAsync(id);
            if (taskMaster == null)
            {
                return NotFound();
            }
            
            return View(taskMaster);
        }
    }

    
}

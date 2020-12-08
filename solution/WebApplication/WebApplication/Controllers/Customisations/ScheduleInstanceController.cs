using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class ScheduleInstanceController : BaseController
    {
        public async Task<IActionResult> IndexDataTable()
        {
            var adsGoFastContext = _context.ScheduleInstance.Take(1);
            return View(await adsGoFastContext.ToListAsync());
        }

        public JObject GridCols()
        {
            JObject GridOptions = new JObject();

            JArray cols = new JArray();
            cols.Add(JObject.Parse("{ 'data':'ScheduleInstanceId', 'name':'ScheduleInstanceId', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'ScheduleMaster.ScheduleDesciption', 'name':'ScheduleMaster', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'ScheduledDateTimeOffset', 'name':'ScheduledDateTime', 'autoWidth':true, 'ads_format': 'datetime' }"));
            cols.Add(JObject.Parse("{ 'data':'ActiveYn', 'name':'ActiveYn', 'autoWidth':true, 'ads_format': 'bool' }"));

            HumanizeColumns(cols);

            JArray pkeycols = new JArray();
            pkeycols.Add("ScheduleInstanceId");

            JArray Navigations = new JArray();
            Navigations.Add(JObject.Parse("{'Url':'/TaskInstance/IndexDataTable?ScheduleInstanceId=','Description':'View Task Instances'}"));

            GridOptions["GridColumns"] = cols;
            GridOptions["ModelName"] = "ScheduleInstance";
            GridOptions["PrimaryKeyColumns"] = pkeycols;
            GridOptions["Navigations"] = Navigations;
            GridOptions["SuppressCrudButtons"] = true;
            GridOptions["InitialOrder"] = JArray.Parse("[[2,'desc']]");

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
                var modelDataAll = (from temptable in _context.ScheduleInstance
                                    select temptable);

                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    modelDataAll = modelDataAll.OrderBy(sortColumn + " " + sortColumnDir);
                }


                //Filter based on querystring params
                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[ScheduleMasterId]"])))
                {
                    var filter = System.Convert.ToInt64(Request.Form["QueryParams[ScheduleMasterId]"]);
                    modelDataAll = modelDataAll.Where(t => t.ScheduleMasterId == filter);
                }


                //Custom Includes
                modelDataAll = modelDataAll
                    .Include(t => t.ScheduleMaster).AsNoTracking();

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

    }
}


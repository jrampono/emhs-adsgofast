using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Framework;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public partial class SubjectAreaFormController : BaseController
    {
        public async Task<IActionResult> IndexDataTable()
        {
            //note - this is just here so that we don't ahve to re-scaffold
            return RedirectToAction(nameof(Index));
        }

    }
}

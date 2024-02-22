using JWTAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using ASPNETWebApp48.Models;
using ASPNETWebApp48.Services;

namespace ASPNETWebApp48.Controllers.Api
{
    /// <summary>
    /// Description of SampleController.
    /// </summary>
    public class GoogleIntegrationController : ApiController
	{
        [HttpPost]
        [Route("api/GoogleIntegration/PushToGoogleSheet")]
        public IHttpActionResult GetSheet(int monthInt = 2, int yearInt = 2024)
        {
            var sheet = GsheetIntegration.GetData(monthInt, yearInt);
            return Ok(sheet);
        }

    }



}
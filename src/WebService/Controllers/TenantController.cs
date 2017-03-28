using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Query;
using System.Text;
using System.Net.Http;
using Common;

namespace WebService.Controllers
{
    [Route("api/tenantservice")]
    public class TenantController : Controller
    {
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext context;

        public TenantController(FabricClient fabricClient, StatelessServiceContext context)
        {
            this.fabricClient = fabricClient;
            this.context = context;
        }

        [HttpGet]
        public Task<ApplicationList> Get()
        {
            return fabricClient.QueryManager.GetApplicationListAsync();
        }

        [HttpPost]
        [Route("{tenantName}/{parameters}")]
        public Task Post(string tenantName, string parameters)
        {
            var app = new ApplicationDescription()
            {
                ApplicationName = new Uri($"fabric:/{tenantName}"),
                ApplicationTypeName = "MyShopType",
                MaximumNodes = 3,
                ApplicationTypeVersion = "1.0.0"
            };

            app.ApplicationParameters.Add("WebService_AppPath", tenantName);

            return fabricClient.ApplicationManager.CreateApplicationAsync(app);
        }
        
    }
}

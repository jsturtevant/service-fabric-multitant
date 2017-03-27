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
    [Route("api/jobservice")]
    public class JobController : Controller
    {
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext context;

        public JobController(FabricClient fabricClient, StatelessServiceContext context)
        {
            this.fabricClient = fabricClient;
            this.context = context;
        }

        [HttpGet]
        public Task<ServiceList> Get()
        {
            string applicationName = this.context.CodePackageActivationContext.ApplicationName;

            return fabricClient.QueryManager.GetServiceListAsync(new Uri(applicationName));
        }

        [HttpPost]
        [Route("{jobName}/{parameters}")]
        public Task Post(string jobName, string parameters)
        {
            var app = new ApplicationDescription()
            {
                ApplicationName = new Uri($"fabric:/{jobName}"),
                ApplicationTypeName = "MyShopType",
                MaximumNodes = 3,
                ApplicationTypeVersion = "1.0.0"
            };

            app.ApplicationParameters.Add("WebService_AppPath", jobName);

            return fabricClient.ApplicationManager.CreateApplicationAsync(app);
            //return fabricClient.ServiceManager.CreateServiceAsync(serviceDescription);
        }
        
    }
}

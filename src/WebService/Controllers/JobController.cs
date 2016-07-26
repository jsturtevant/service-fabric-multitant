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
            string applicationName = this.context.CodePackageActivationContext.ApplicationName;

            StatefulServiceDescription serviceDescription = new StatefulServiceDescription()
            {
                ApplicationName = new Uri(applicationName),
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription()
                {
                    LowKey = 0,
                    HighKey = 10,
                    PartitionCount = 1
                },
                HasPersistedState = true,
                InitializationData = Encoding.UTF8.GetBytes(parameters),
                ServiceTypeName = "JobServiceType",
                ServiceName = new Uri($"{applicationName}/jobs/{jobName}")
            };

            return fabricClient.ServiceManager.CreateServiceAsync(serviceDescription);
        }
        
    }
}

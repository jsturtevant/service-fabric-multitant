
namespace WorkService
{
    using Common;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class WorkService : StatefulService
    {
        public WorkService(StatefulServiceContext serviceContext)
            : base(serviceContext)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                {
                    return new WebHostCommunicationListener(context, "ServiceEndpoint", uri =>
                        new WebHostBuilder().UseWebListener()
                                           .UseContentRoot(Directory.GetCurrentDirectory())
                                           .ConfigureServices(services => services
                                                .AddSingleton<IReliableStateManager>(this.StateManager)
                                                .AddSingleton<StatefulServiceContext>(context))
                                           .UseStartup<Startup>()
                                           .UseUrls(uri)
                                           .Build());
                })
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            IReliableQueue<string> queue = await this.StateManager.GetOrAddAsync<IReliableQueue<string>>("jobQueue");
            IReliableDictionary<string, Job> dictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Job>>("jobs");

            // need to restart any existing jobs after failover
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await dictionary.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(cancellationToken))
                {
                    Job job = enumerator.Current.Value;

                    Task t = this.StartJob(job, cancellationToken);
                }
            }

            // start processing new jobs from the queue.
            while (true)
            {
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    ConditionalValue<string> result = await queue.TryDequeueAsync(tx);

                    if (!result.HasValue)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        continue;
                    }

                    ConditionalValue<Job> job = await dictionary.TryGetValueAsync(tx, result.Value);

                    if (job.HasValue)
                    {
                        Task t = this.StartJob(job.Value, cancellationToken);
                    }

                    await tx.CommitAsync();
                }
            }
        }

        private Task StartJob (Job job, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("store://state/jobstate");

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        var result = await myDictionary.TryGetValueAsync(tx, job.Name, LockMode.Update);

                        string iteration = result.HasValue ? result.Value.ToString() : "0";
                        ServiceEventSource.Current.ServiceMessage(this, $"Work {job.Name}: {job.Parameters}. Iteration: {iteration}");

                        await myDictionary.AddOrUpdateAsync(tx, job.Name, 0, (key, value) => ++value);

                        // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                        // discarded, and nothing is saved to the secondary replicas.
                        await tx.CommitAsync();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }, cancellationToken);
        }
    }
}

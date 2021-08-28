using Blogbook.ApiClients;
using Blogbook.Data;
using Blogbook.Managers;
using Blogbook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blogbook.ScheduledServices
{
    internal class ImportBlogsService : BackgroundService
    {
        public IServiceProvider Services { get; }

        public ImportBlogsService(IServiceProvider services)
        {
            Services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await DoWork(stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using IServiceScope scope = Services.CreateScope();
            var scopedProcessingService =
           scope.ServiceProvider
               .GetRequiredService<IScopedProcessingService>();

            await scopedProcessingService.DoWork(stoppingToken);
        }
    }

    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class ScopedProcessingService : IScopedProcessingService
    {
        private readonly IPostManager _postManager;
        private readonly IConfiguration _configuration;

        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;
        private readonly string Schedule; //Every day at midnight

        public ScopedProcessingService(IConfiguration configuration, IPostManager postManager)
        {
            _configuration = configuration;
            Schedule = _configuration["ImportBlogTask:Schedule"];

            _schedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            //_nextRun = _schedule.GetNextOccurrence(DateTime.Today);
            _postManager = postManager;

        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
             while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now > _nextRun)
                {
                    var posts = await ApiBlogClient.GetPostsAsync();

                    foreach (Post post in posts) await _postManager.AddPostAsync(post, "admin");

                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
                await Task.Delay(5000, stoppingToken);
            }
        }
    }   
}
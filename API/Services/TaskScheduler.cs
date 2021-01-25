﻿using API.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    public class TaskScheduler : ITaskScheduler
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<TaskScheduler> _logger;
        private readonly IScannerService _scannerService;
        public BackgroundJobServer Client => new BackgroundJobServer();

        public TaskScheduler(ICacheService cacheService, ILogger<TaskScheduler> logger, IScannerService scannerService)
        {
            _cacheService = cacheService;
            _logger = logger;
            _scannerService = scannerService;

            _logger.LogInformation("Scheduling/Updating cache cleanup on a daily basis.");
            RecurringJob.AddOrUpdate(() => _cacheService.Cleanup(), Cron.Daily);
            RecurringJob.AddOrUpdate(() => _scannerService.ScanLibraries(), Cron.Daily);
        }

        public void ScanSeries(int libraryId, int seriesId)
        {
            _logger.LogInformation($"Enqueuing series scan for series: {seriesId}");
            BackgroundJob.Enqueue(() => _scannerService.ScanSeries(libraryId, seriesId));
        }

        public void ScanLibrary(int libraryId, bool forceUpdate = false)
        {
            _logger.LogInformation($"Enqueuing library scan for: {libraryId}");
            BackgroundJob.Enqueue(() => _scannerService.ScanLibrary(libraryId, forceUpdate));
        }

        public void CleanupVolumes(int[] volumeIds)
        {
            BackgroundJob.Enqueue(() => _cacheService.CleanupVolumes(volumeIds));
            
        }
        
    }
}
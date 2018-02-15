using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.AzureRepositories;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class SettingsRootService : ICustomStartup, ISettingsRootService
    {
        private readonly object _updateLock = new object();
        [CanBeNull] private SettingsRoot _cache;

        private readonly ISettingsValidationService _settingsValidationService;
        private readonly IAlertService _alertService;
        private readonly ISettingsStorageService _settingsStorageService;

        public SettingsRootService(ISettingsStorageService settingsStorageService,
            ISettingsValidationService settingsValidationService, IAlertService alertService)
        {
            _settingsStorageService = settingsStorageService;
            _settingsValidationService = settingsValidationService;
            _alertService = alertService;
        }

        public SettingsRoot Get()
        {
            return _cache.RequiredNotNull("_cache != null");
        }

        public void Set([NotNull] SettingsRoot settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _settingsValidationService.Validate(settings);
            lock (_updateLock)
            {
                _settingsStorageService.Write(settings);
                _cache = settings;
            }
        }

        public void Initialize()
        {
            _cache = _settingsStorageService.Read()
                     ?? new SettingsRoot(ImmutableSortedDictionary<string, ExchangeSettings>.Empty,
                         TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10), false);

            try
            {
                _settingsValidationService.Validate(_cache);
            }
            catch (Exception e)
            {
                _alertService.AlertRiskOfficer(string.Empty, "Found invalid settings on service start: " + e.Message,
                    EventTypeEnum.InvalidSettingsFound);
            }
        }
    }
}
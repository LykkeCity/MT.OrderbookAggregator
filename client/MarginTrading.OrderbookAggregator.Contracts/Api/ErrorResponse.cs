﻿using JetBrains.Annotations;

namespace MarginTrading.OrderbookAggregator.Contracts.Api
{
    [UsedImplicitly] // from startup.cs only in release configuration
    public class ErrorResponse
    {
        [UsedImplicitly]
        public string ErrorMessage { get; set; }

        [UsedImplicitly]
        public string Details { get; set; }
    }
}
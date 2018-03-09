![Build status](http://teamcity.lykkex.net/app/rest/builds/aggregated/strob:(buildType:(project:(id:MarginTrading_OrderbookAggregator)))/statusIcon.svg)

# Lykke MarginTrading Orderbook Aggregator 
This service collects orderbooks from external exchange connectors, applies markups to them and sends to RabbitMq in MessagePack format for the MT Backend services to consume them. 
These orderbooks are then used for the STP mode trading.

This service also provides a configuration api with validations, a config storage and diagnostic slack alerter.

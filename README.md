# Setup

To run this function, you will need a `.\src\Defra.Trade.Events.DAERA.GCNotifier\local.settings.json` file with the following structure:

```jsonc 
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ServiceBus:ConnectionString": "<secret>",
    "ConfigurationServer:ConnectionString": "<secret>",
    "ConfigurationServer:TenantId": "<secret>"
  }
}
```

Secrets can be found [here](https://dev.azure.com/defragovuk/DEFRA-TRADE-APIS/_wiki/wikis/DEFRA-TRADE-APIS.wiki/26086)

Putting messages onto the `defra.trade.events.remos.gcnotification`, queue will then be picked up and processed by this function app.

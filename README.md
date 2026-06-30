# AzuFlow

## Docker

Build the image:

```sh
docker build -t azu-flow .
```

Run the container on <http://localhost:8080>:

```sh
docker run --rm -p 8080:8080 \
  -e Azure__SubscriptionId="<subscription-guid>" \
  -e Azure__TenantId="<tenant-guid>" \
  azu-flow
```

You can optionally add topology definitions that cannot be discovered from Azure Functions bindings, for example manual Service Bus receive/send flows. Mount a JSON file at `/config/topology-supplement.json`:

```sh
docker run --rm -p 8080:8080 \
  -e Azure__SubscriptionId="<subscription-guid>" \
  -e Azure__TenantId="<tenant-guid>" \
  -v "$PWD/topology-supplement.json:/config/topology-supplement.json:ro" \
  azu-flow
```

The supplement file uses the same graph shape returned by `/api/topology`; discovered nodes and edges win when IDs overlap. Add the JSON Schema reference for editor completion:

```json
{
  "$schema": "./schemas/topology-supplement.schema.json",
  "nodes": [
    {
      "id": "manual:function:sender",
      "type": "Function",
      "name": "ManualSender",
      "resourceGroup": "manual"
    },
    {
      "id": "manual:queue:orders",
      "type": "ServiceBusQueue",
      "name": "orders",
      "resourceGroup": "manual"
    }
  ],
  "edges": [
    {
      "id": "manual:function:sender>manual:queue:orders",
      "sourceNodeId": "manual:function:sender",
      "targetNodeId": "manual:queue:orders"
    }
  ]
}
```

The image serves the React frontend and the ASP.NET Core API from the same container. The frontend calls the API through `/api`.

For non-interactive Azure authentication, pass credentials supported by `DefaultAzureCredential`, for example service principal credentials:

```sh
docker run --rm -p 8080:8080 \
  -e Azure__SubscriptionId="<subscription-guid>" \
  -e Azure__TenantId="<tenant-guid>" \
  -e AZURE_TENANT_ID="<tenant-guid>" \
  -e AZURE_CLIENT_ID="<client-id>" \
  -e AZURE_CLIENT_SECRET="<client-secret>" \
  azu-flow
```

By default, AzuFlow authenticates during startup, tries `DefaultAzureCredential`, and then falls back to device code authentication. Azure SDK token refresh can still happen later while handling requests. You can force the Azure authentication method at startup with `Azure__AuthenticationMethod`. Supported values are `DefaultThenDeviceCode`, `Default`, `DeviceCode`, and `InteractiveBrowser`.

```sh
docker run --rm -p 8080:8080 \
  -e Azure__SubscriptionId="<subscription-guid>" \
  -e Azure__TenantId="<tenant-guid>" \
  -e Azure__AuthenticationMethod="DeviceCode" \
  azu-flow
```

When running the API directly, you can use the shorter command-line option:

```sh
dotnet run --project backend/AzuFlow.Api -- \
  --Azure:SubscriptionId "<subscription-guid>" \
  --Azure:TenantId "<tenant-guid>" \
  --auth-method DeviceCode
```

For local development on a machine with a browser, you can open the browser sign-in flow explicitly:

```sh
dotnet run --project backend/AzuFlow.Api -- \
  --Azure:SubscriptionId "<subscription-guid>" \
  --Azure:TenantId "<tenant-guid>" \
  --auth-method InteractiveBrowser
```

For local development, you can omit the service principal environment variables. When device code authentication is used, the API prints the sign-in instructions in the container logs:

```sh
docker logs -f <container-id>
```

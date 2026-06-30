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

For local development, you can omit the service principal environment variables. When the API first needs Azure access, it falls back to device code authentication and prints the sign-in instructions in the container logs:

```sh
docker logs -f <container-id>
```

# CAPI Integration Example
This repo contains projects to help with the integrationsd with CAPI (Concerto API), the repo is split into deployment and functions folders. Deployment contains bicep files and a powershell script to deploy the necessary resources to Azure for running this example. Functions has the Azure functions project for processing events from a webhook and sending those to queues.

Both the bicep and Azure functions projects can be edited/debugged using VS Code, all required extensions should install automatically when opening the workspaces.

## Deployment
To deploy the resources to Azure you will need:
- A Microsoft account with an Azure subscription
- Azure CLI installed locally
- Azure CLI authenticated with Microsoft account

After installing Azure CLI and using `az login` to authenticate the resources can be deployed with the `deployment.ps1` powershell script. The script will prompt for a resource group name and an Azure location, leave them blank for defaults.

Once the deployment is finished the resource group should contain a function application, service bus namespace and a storage account.

To deploy the function app using VS Code use the Azure extension, find the deployed Function App resource in Azure Explorer and Right Click > Deploy to function app.

## Function App
The function app implements two functions, one with a `HTTP Trigger` and the other a `Service Bus Queue Trigger`. It also contains a CAPI client that can be used with HttpClient DI, it will automatically authenticate with CAPI and add the auth headers to each request. 

The HTTP triggered function is intended to be used with CAPI webhooks to recieve event payloads, inspect the eventType and pass this onto a Service Bus topic for distribution.

The Service Bus queue triggered function accepts messages from an order event type queue, requests more order information from CAPI and then sends an enriched message to a channel topic for distribution.

### Settings
A template configuation file has been added to the function app project called template.settings.json. These values can be copied into a local.settings.json to be consumed by the function app.

The settings include:
- capi-webhook-secret: Plaintext secret setup on the webhook integrations page of CAPI.
- capi-api-key: API key retrieved from CAPI portal when creating a new API key.
- capi-api-secret: API secret retrieved from CAPI portal when creating a new API key.
- capi-base-path: Base path of the CAPI portal instance.
- sas-capi-read-write: A connection string to the service bus resource on Azure. This can be found in the application properties of the function app resource or in the `Shared access policies` page of the Service Bus resource after Bicep deployment.
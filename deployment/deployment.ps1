$resourceGroupName = Read-Host -Prompt 'Input the resource group name, leave blank for "rg-capi-dev"'
if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "rg-capi-dev"
}

$location = Read-Host -Prompt 'Input the Azure location, leave blank for UKWest'
if ([string]::IsNullOrWhiteSpace($location)) {
    $location = "ukwest"
}

$BicepFile = $PSScriptRoot + "\main.bicep"

$resourceGroupExists = az group exists -n $resourceGroupName

# Check if Resource Group exists
if ($resourceGroupExists -eq $false) {
    Write-Output("Resource Group: $resourceGroupName does not exist. Creating...")
    az group create -l $location -n $resourceGroupName
}
else {
    Write-Output("Resource Group: $resourceGroupName already exists. Skipping create.")
}

Write-Output("Deploying Azure resources...")

az deployment group create --name CapiExampleDeployment --resource-group $resourceGroupName --template-file $BicepFile
Desired fields with the cloud events schema:
{
  "source": "/subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.ApiManagement/service/{resource-instance}",
  "subject": "/apis/{api-id}/releases/{release-id}",
  "type": "Microsoft.ApiManagement.APIReleaseUpdated",
  "time": "2021-07-12T23:13:44.9048323Z",
  "id": "95015754-aa51-4eb6-98d9-9ee322b82ad7",
  "data": {
    "resourceUri": "/subscriptions/subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.ApiManagement/service/{resource-instance}/apis/{api-id}/releases/{release-id}"
  },
  "specversion": "1.0"
}

Desired fields with the event grid schema:
{
  "id": "95015754-aa51-4eb6-98d9-9ee322b82ad7",
  "topic": "/subscriptions/{subscription-id}/resourceGroups/resource-group}/providers/Microsoft.ApiManagement/service/{resource-instance}",
  "subject": "/apis/{api-id}/releases/{release-id}",
  "data": {
    "resourceUri": "/subscriptions/subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.ApiManagement/service/{resource-instance}/apis/{api-id}/releases/{release-id}"
  },
  "eventType": "Microsoft.ApiManagement.APIReleaseUpdated",
  "dataVersion": "1",
  "metadataVersion": "1",
  "eventTime": "2021-07-12T23:13:44.9048323Z"
}

Desired variables:
- {subscription-id} random guid
- {resource-group} random words in lowercase
- {resource-instance} random words in lowercase
- {api-id} random guid
- {release-id} random guid

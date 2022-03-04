param AppInsightsResourceId string
param workbookDisplayName string = 'Function Analysis Workbook'
param workbookType string = 'workbook'
param workbookName string = newGuid()
param location string = resourceGroup().location

var workbookContent = loadTextContent('workbookContent.json')


resource funcworkbook 'Microsoft.Insights/workbooks@2021-08-01' = {
  name: workbookName
  location: location
  kind: 'shared'
  properties: {
    category: workbookType
    displayName: workbookDisplayName
    serializedData: workbookContent
    sourceId: AppInsightsResourceId
  }
}

 resource Account 'Microsoft.DocumentDB/databaseAccounts@2021-05-15' = {
   name: 'csmsbicep${uniqueString(resourceGroup().id)}'
   location: resourceGroup().location
   properties: {
     databaseAccountOfferType: 'Standard'
     locations: [
       { 
         locationName: 'westeurope'
       }
       { 
         locationName: 'eastus2'
       }
     ]
     enableMultipleWriteLocations: true
   }
 }


# Energy Management Data Service

EMDataService is a windows service, which does below things :
1)Fetch PowerScout,Weather data from local SQL database, process 30 mins entry per powerscout and inserts single entry to Azure SQL Database.
2)Fetch Sensor data from local SQL database, inserts entry to Azure SQL database of every minute.Also generates notification if any new sensor attached to the system.

## Getting Started


### Prerequisites

1)Visual Studio 2017 with Other Project types(Visual Studio installer type) installed.
2)EMS (Energy Management Solution) deployed(Purchase Energy Management Solution from Azure Marketplace).
3)Virtual Machine with Microsoft SQL server installed (A VM which we get after purchase of EMS)
4)Osi Soft's PI Server installed on VM.
5)Azure SQL Database connection string(Azure SQL database purchased inside EMS)
6)Local SQL Database connection string(Pi Server Database string)
7)Firebase Credentials (Update by user via Admin utility on Web portal)
8)Azure storage connection string (Storage account purchased inside EMS)
9)Azure App Insight's Instrumentation key.

### Installing

1)Build EMDataServiceSetup project in a Release mode.
2)Double click .msi file at ProjectPath/bin/Release/
3)Follow the instructions of the setup wizard.
4)Open Services in your computer -> Find DataServiceEM service -> Right Click -> Start


## Deployment
1)Download this project.
2)Configure App.config file of DataService project.
3)Deployment of Azure assets can be take place automatically when you purchase EMS from azure marketplace.
5)Configure things via WebApp admin utility.So that Dataservice get all related stuff from server.

Add additional notes about how to deploy this on a live system

## Built With

* [Visual Studio 2017](https://www.visualstudio.com/downloads/) - IDE


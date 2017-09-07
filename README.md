# Energy Management Data Service

EMDataService is a windows service, which does below things :
* Fetch PowerScout,Weather data from local SQL database, process 30 mins entry per powerscout and inserts single entry to Azure SQL Database.
* Fetch Sensor data from local SQL database, inserts entry to Azure SQL database of every minute.Also generates notification if any new sensor attached to the system.

## Getting Started


### Prerequisites

* Visual Studio 2017 with Other Project types(Visual Studio installer type) installed.
* EMS (Energy Management Solution) deployed(Purchase Energy Management Solution from Azure Marketplace).
* Virtual Machine with Microsoft SQL server installed (A VM which we get after purchase of EMS)
* Osi Soft's PI Server installed on VM.
* Azure SQL Database connection string(Azure SQL database purchased inside EMS)
* Local SQL Database connection string(Pi Server Database string)
* Firebase Credentials (Update by user via Admin utility on Web portal)
* Azure storage connection string (Storage account purchased inside EMS)
* Azure App Insight's Instrumentation key.

### Installing

* Build EMDataServiceSetup project in a Release mode.
* Double click .msi file at ProjectPath/bin/Release/
* Follow the instructions of the setup wizard.
* Open Services in your computer -> Find DataServiceEM service -> Right Click -> Start

## Deployment
* Download this project.
* Configure App.config file of DataService project.
* Deployment of Azure assets can be take place automatically when you purchase EMS from azure marketplace.
* Configure things via WebApp admin utility.So that Dataservice get all related stuff from server.

## Built With

* [Visual Studio 2017](https://www.visualstudio.com/downloads/) - IDE


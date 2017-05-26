# Energy Management Data Service

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.
### Prerequisites

------What things you need to install the software and how to install them

1)Energy Management Solution deployed(Purchase Energy Management Solution from Azure Marketplace).
2)Virtual Machine with Microsoft SQL server installed (A VM which we get after purchase of EMS)
3)Osi Soft's PI Server
4)Class Schedule file.
5)Valid Google-Firebase credentials.


### Installing

A step by step series of examples that tell you have to get a development env running

1)Install OSI Soft's PI server on VM if it is not installed.
2)Install Microsoft SQL Server on the Same VM
3)Update PC Date time based on the timezone where University is situated.
4)Configure App.config file of this data service.Update it with PiserverName and Azure database connection string.

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment
1)Download it from Github.com.
2)Do its configuration.
3)Deployment of Azure assets can be take place automatically when you purchase EMS from azure marketplace.
4)You should have a valid IOT interfaces which inserts data into PI server(SQL Server).And it is alwys up and running.
5)Configure things via WebApp admin utility.So that Dataservice get all related stuff from server.

Add additional notes about how to deploy this on a live system

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md] for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## License

Currently keep this open
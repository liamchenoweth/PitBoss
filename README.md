# PitBoss
Dotnet Core based Pipeline Runner designed to run scaled pipelines across many machines.

## Installation 
PitBoss runs on containers, so it can run anywhere containers can. The basics you will need to run PitBoss are the Boss, the Container Service, The Distributed Step Service and the Operation Group Container Service. The container service will also need access to create new containers.

### Automated Installation
#### Docker Compose
This repository comes with a pre-built docker compose file [here](https://github.com/liamchenoweth/PitBoss/blob/master/_deployment/docker-compose.yml) which you can configure for your system.

#### Helm
It also comes with a pre-built helm deployment, it is currently not part of a repository, but you can link a custom repo in helm to this repository, the values file has all the available options with documentation.

## Components

### The Boss
This is the interface between the internals of PitBoss and your instructions (ie. it's a fancy API).

### The Container Service
This is a standalone service that does the creation, management and destruction of the worker containers, to do this, it needs access to manage containers in your environment.

### The Distributed Step Service
This service monitors asyncronous distributed steps to determine when paralell processes have finished to keep continuity between descreet steps.

### The Operation Group Container Service
This service keeps track of containers in pipelines and the operations they are running, it is the service that sends new tasks to each container.

## Contributing

This is a personal project that was created to allow for easy creation of simple but effective pipelines. I will personally be adding features to the project as I come across new problems, but I will not be taking feature requests.

That being said, if you would like a feature added to the project I will be active in pull requests and would be more than happy to incorporate work into the repository for all to use.
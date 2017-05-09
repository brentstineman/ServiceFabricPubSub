## Objective ##
This project, to a certain extent, is an excuse. An excuse to dive deeper into some topics related to Service Fabric stateful services. The scenario, while not tied to any of my partners, is something the product team even admits is asked for frequently. Additionally, there's a common debate between building something like this, versus just using something like Service Bus. 
 
With this in mind, this project has three objectives:
* get experience with service fabric stateful services
* give the PG a reference project for the "pub/sub queues" project
* get experience trying to "build" this solution so we can properly advise partners on the level of complexity. 

If this doesn't stress it enough, this is only a reference project and should NOT be used as the basis for a production solution. If you decide to just copy/paste this into your production solution, please don't come back and complain that something fell down.

## Overview ##
In short, this solution is a multi-tenant application that allows tenants to create topics to which they can publish message. Each topic then has one or more subscribers, creating a publisher, subscriber queue pattern. The interaction with the model is very simple and its feature set is limited to basic "get", "put" operations. A get automatically deletes a message. 

Tenant administration is handled via certificate (a single cert to work with the tenant administration), or by the use of the tenant ID and one of two account keys when working with the topics and queues. 

Addressing an object within the solution is done via a simple URL pattern/route: /\<tenant\>/\<topic\>/\<subscriber\>
 
> If you are famliar with Azure Storage Queues and Service Bus Topics, you'll see aspects of both in this design. However, since this is a reference project, you'll note that what this solution will provide will be far short of the features available from either of those production services. This is as intended as we are not trying to create a replacement for either solution.

The solution consists of two service fabric applications and a console app. 

**Front End** - The front end application contains two default, stateless service. An Administrative service handles the creation of new tenants and interaction with the back end tenant specific administration service. 

**Tenant Application** - This application is instantiated each time a new tenant is created. It contains a single default service (administration) that enables the creation of topics and the reset of the two account keys that were created when the tenant was instantiated.

**Test Console** - The test application is a console app that allows you to interact with the solution. Alternatively, you can opt to use a 3rd party REST API tool such as Fiddler or PostMan. 

## Technical References ##
The following links discuss details that are pertinent to how this solution was architected. 

[Multi-tenant and dynamic application/service instantiation](https://azure.microsoft.com/en-us/resources/samples/service-fabric-dotnet-iot/)

[Reliable Statement Manager & Notifications](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-notifications)

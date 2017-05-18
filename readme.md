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

## How to test
### Prereq
* Ensure both 'TenantApplication' and 'FrontEnd' Service Fabric applications are deployed locally.
* For the 'ClientApp' project, specify the following as command line arguments:
```  
ClientApp.exe http://localhost:8277/ http://localhost:8979/
```
 
### Create a tenant
* Use the 'ClientApp' project to create a new tenant. The tenant name must be lowercase and 8-15 characters.
* In Service Fabric Explorer, you should see a 'fabric:/{YOUR-TENANT-NAME}' application under the 'TenantApplicationType' application.  There should also be a 'fabric:/{YOUR-TENANT-NAME}/Admin' service.
 
### Get a key for the tenant
* Invoke the following via PostMan:  
```
GET http://localhost:19081/{YOUR-TENANT-NAME}/Admin/api/keys/key1
```
* Use the returned GUID as a key (HTTP header 'x-request-key')
 
### Create a topic
* Set the 'x-request-key' header with a value of the tenant's key (key 1 or key 2)
* Invoke the following via PostMan:
```
PUT http://localhost:19081/{YOUR-TENANT-NAME}/Admin/api/topics/{YOUR-TOPIC-NAME}
```
 
* The above (creating a topic) may be possible via the ClientApp but I haven't tried.
 
### Post a message to a topic
* Invoke the following via PostMan:
```
	   POST http://localhost:8277/api/request/{YOUR-TENANT-NAME}/{YOUR-TOPIC-NAME}
	   Header Key: x-request-key
	   Header Value: one of the tenant's keys
	 
	   Header Key: Content-Type
	   Header Value: application/json
	 
	   Body: some JSON formatted string
```
### Load tests
* Get and build https://github.com/PiDiBi/SuperBenchmarker
Use it this way: 
```
sb.exe -u http://localhost:8277/api/request/davidbures/topic1 -m POST -t template.txt -n 1000 -c 10
```
Template.txt :
```
x-request-key: d7f25172-7391-4274-b657-key1-or-2

"test message"
```

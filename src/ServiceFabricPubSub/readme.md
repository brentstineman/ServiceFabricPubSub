# Tenant Application
Open TenantApplication.sln to develop and debug ServiceFabric project. This project consist from:

* PubSubDotnetSDK - library with basic interfaces for services and PubSubMessage object

* TopicService - main service
 * holds output qeueu for subscribers, subscriber's qeueu are added dynamicaly (RegisterSubscriber)
 * provide Push method to enqueue message - this messages are added to input api qeueu and handled separately with DuplicateMethod
 * duplicate messages to output qeueus for subscribed subscribers
 * RunAsync - adds testing message every second

* SubscriberService
 * can be run multiple times, every instance call TopicService.RegisterSubscriber
 * can Pop messege from it's dynamicaly created qeueu from TopicService

* TestWebApi
 * api/pop/subscriberName=Subcriber1 - pops message for subscriber
 * api/push - push message

## TODO
* remove hardcoded names for services
* move output qeueus from Topic to Subscribers
* do transactional handling 

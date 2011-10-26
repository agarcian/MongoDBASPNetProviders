#ASPNet Providers for Membership, Roles, and Profile using MongoDB

This project is an implementation of the ASPNet Providers for __Membership__, __Roles__, and __Profile__ using MongoDB.

##Instructions

This Visual Studio 2010 solution contains the implementation of the Membership, Roles, and Profile providers.

Download the solution and compile to use in your own project.

- __ASPNetProvidersForMongoDB__: This project contains the actual implementation in addition to a ProfileCommon implementation that you will need for the Membership provider.
- __TestProjectForMongoDBASPNETProviders__: This project is an incomplete suite of tests against the project. 


In order to run the application you need to:

- Create a Mongo database and get the connection string.
- Apply to the web.config the following elements:
 - `configuration/connectionStrings`
 - `configuration/system.web/machinekey`
 - `configuration/system.web/membership`
 - `configuration/system.web/roleManager`
 - `configuration/system.web/profile`  (Here you will use the `ProfileCommon` custom class you will create)
- Define the `ProfileCommon` object for your needs.


### Notes on SafeMode:

Note that we are using SafeMode for the transactions in order to be able to validate the results.  
If you are concerned about performance, set it to false, and change the code to not make use of the Result object since it will be null.

My recommendation is to leave it as the transactions in SafeMode won't significantly impact your application's performances (or so I believe).

### Want to help?
Please comment on anything you think can be improved.
Also you can help adding more test cases to make sure the implementation is completely validated.  I will be providing more test cases over time.
=======
#ASPNet Providers for Membership, Roles, and Profile using MongoDB

This project is an implementation of the ASPNet Providers for __Membership__, __Roles__, and __Profile__ using MongoDB.  

Currently using the CSharp Driver 1.2 

##Instructions

This Visual Studio 2010 solution contains the implementation of the Membership, Roles, and Profile providers.

Download the solution and compile to use in your own project.

- __ASPNetProvidersForMongoDB__: This project contains the actual implementation in addition to a ProfileCommon implementation that you will need for the Membership provider.
- __TestProjectForMongoDBASPNETProviders__: This project is an incomplete suite of tests against the project. 


In order to run the application you need to:

- Create a Mongo database and get the connection string.
- Apply to the web.config the following elements:
 - `configuration/connectionStrings`
 - `configuration/system.web/machinekey`
 - `configuration/system.web/membership`
 - `configuration/system.web/roleManager`
 - `configuration/system.web/profile`  (Here you will use the `ProfileCommon` custom class you will create)
- Define the `ProfileCommon` object for your needs.


### Notes on SafeMode:

Note that we are using SafeMode for the transactions in order to be able to validate the results.  
If you are concerned about performance, set it to false, and change the code to not make use of the Result object since it will be null.

My recommendation is to leave it as the transactions in SafeMode won't significantly impact your application's performances (or so I believe).

### Want to help?
Please comment on anything you think can be improved.
Also you can help adding more test cases to make sure the implementation is completely validated.  I will be providing more test cases over time.

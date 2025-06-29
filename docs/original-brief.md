# Original brief

## Context from Matt

Please find the test attached.

* Use SOLID principles
* Implement unit tests
* Add a readme file to explain decision making

Let me know when you complete it and send it back to me.

## 01 - Task

Ensuring the Acceptance Criteria are met, build a C# Web API that connects to an instance of a database and persists the contents of the Meter Reading CSV file.

We have provided you with a list of test customers along with their respective Account IDs (please refer to Test_Accounts.csv). Please seed the Test_Accounts.csv data into your chosen data storage technology and validate the Meter Read data against the accounts.

It is advised to use a publicly accessible Git repository to commit and share your work with us. 

## 02 - User story

As an Energy Company Account Manager, I want to be able to load a CSV file of Customer Meter Readings so that we can monitor their energy consumption and charge them accordingly

## 03 - Acceptance Criteria

### Must have:

* Create the following endpoint:
	
	POST => /meter-reading-uploads

* The endpoint should be able to process a CSV of meter readings. An example CSV file has been provided (Meter_reading.csv)

* Each entry in the CSV should be validated and if valid, stored in a DB.

* After processing, the number of successful/failed readings should be returned.

* Validation: 
	* You should not be able to load the same entry twice
	* A meter reading must be associated with an Account ID to be deemed valid
	* Reading values should be in the format NNNNN


### Nice to have:

* Create a client in the technology of your choosing to consume the API. You can use angular/react/whatever you like
* When an account has an existing read, ensure the new read isn’t older than the existing read


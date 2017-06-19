Feature: Test


@GetTest
Scenario: Get a TestObject
	Given I have a controller
		And my Id is '1'
	Then get the object
		And it is valid
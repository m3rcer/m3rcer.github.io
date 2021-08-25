---
title: Implementing an Abstract class with the override method and an Interface.
date: 2021-05-29 11:45:47 +07:00
categories: greyhatc_intro
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Implementing an Abstract class with the override method and an Interface.
---


## Here is an example that represents a public servant data structure for public servants using abstract and override methods. 

- Classes and interfaces can have properties(variables that get/set values) or methods(functions that execute on the class/interface).

- A subclass inherits properties and methods from its parent class whereas interfaces force a class to imlement methods/properties that aren't inherited.

- Abstract classes cannot be instantiated but only inherited through subclassing.

- Use the override method to instantiate the inherited  parent abstract class properties in a subclass. 


### Code:


```Csharp
using System;

// Base/Parent Class (abstract class - can't be instantiated here)
public abstract class PublicServant
{
        // Property
	public int PensionAmount { get; set; }
	
	// Method
	public abstract void DriveToPlaceOfInterest();
}


//interface creation
public interface IPerson
{
	string Name { get; set; }
	int Age { get; set; }
}




// Sublclassing from the Parent Abstract class and implementing the interface
public class Firefighter : PublicServant , IPerson
{
     public Firefighter(string name, int age)  //constructor
     {
           this.Name = name;
           this.Age = age;
     }

     public string Name { get; set; }
     public int Age { get; set; }
     
     //override method to define abstract class
     public override void DriveToPlaceOfInterest() 
     {
	     GetInFiretruck();
	     TurnOnSiren();
	     FollowDirections();
     }
     
     private void GetInFiretruck() {}
     private void TurnOnSiren() {}
     private void FollowDirections() {}
}


// Sublclassing from the Parent Abstract class and implementing the interface along with a bool op to turn on siren if true.
public class PoliceOfficer : PublicServant , IPerson
{
	private bool _hasEmergency;

	public PoliceOfficer(string name, int age)
	{
		this.Name = name;
		this.Age = age;
		_hasEmergency = false;
	}

	public string Name { get; set; }
	public int Age { get; set; }

	public bool HasEmergency
	{
		get { return _hasEmergency; }
		set { _hasEmergency = value; }
	}

	public override void DriveToPlaceOfInterest()
	{
		GetInPoliceCar();
		if (this.HasEmergency)
			TurnOnSiren();
		FollowDirections();
	}

	private void GetInPoliceCar() {}
	private void TurnOnSiren() {}
	private void FollowDirections() {}
}







namespace ch1_the_fighters
{
	public class MainClass
	{
		public static void Main(string[] args)
		{
		        //Instantiating classes
			Firefighter firefighter = new Firefighter("blossom" , 23);
			firefighter.PensionAmount = 5000;

			PrintNameAndAge(firefighter);
			PrintPensionAmount(firefighter);

			firefighter.DriveToPlaceOfInterest();

			PoliceOfficer officer = new PoliceOfficer("bubbles" , 25);
			officer.PensionAmount = 10000;
			officer.HasEmergency = true;

			PrintNameAndAge(officer);
			PrintPensionAmount(officer);

			officer.DriveToPlaceOfInterest();
		}

		static void PrintNameAndAge(IPerson person)
		{
			Console.WriteLine("Name: " + person.Name);
			Console.WriteLine("Age :" + person.Age);
		}

		static void PrintPensionAmount(PublicServant servant)
		{
			if (servant is Firefighter)
				Console.WriteLine("Pension of firefighter: " + servant.PensionAmount);
			else if (servant is PoliceOfficer)
				Console.WriteLine("Pension of officer: " + servant.PensionAmount);
		}
	}
}
```		

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/IntroScripts/abstract/abstract.png)	  

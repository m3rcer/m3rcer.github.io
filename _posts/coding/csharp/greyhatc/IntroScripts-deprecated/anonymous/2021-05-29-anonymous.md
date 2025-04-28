---
title: Using a Delegate and referencing through Anonymous methods.
date: 2021-05-29 12:45:47 +07:00
categories: greyhatcch1
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Using a Delegate and referencing through Anonymous methods.
---


## Here is the same example as before for the public servant class but altered to use delegates and anonymous methods.

- Instead of overriding a Base Abstract class's methods to define it in a sublcass we can use Delegates. Delegates is an object created that holds a reference to the method that is created. 
  - In this case we create a delegate in the parent class which is refernced by subclasses through anonymous methods to dynamically assign ("DriveToPlaceOfInterest" method) the method in the respective sublcasses.
- This is an alternative to the "abstract class - override" method.


### Code:

```csharp
using System;


public abstract class PublicServant
{
	public int PensionAmount { get; set; }
	
	// Delegate object created
	public delegate void DriveToPlaceOfInterestDelegate();
	// Delegate assigned to property
	public DriveToPlaceOfInterestDelegate DriveToPlaceOfInterest { get; set; } 
}



public interface IPerson
{
	string Name { get; set; }
	int Age { get; set; }
}





public class Firefighter : PublicServant , IPerson
{
     public Firefighter(string name, int age)
     {
           this.Name = name;
           this.Age = age;
     
            // Instantiating the Delegated property using - Anonymous Methods	   
            this.DriveToPlaceOfInterest = delegate
	    {
		    Console.WriteLine("Driving Firetruck");
	            GetInFiretruck();
	            TurnOnSiren();
	            FollowDirections();
	    };
     }
     
     
     public string Name { get; set; }
     public int Age { get; set; }


     private void GetInFiretruck() {}
     private void TurnOnSiren() {}
     private void FollowDirections() {}
}



	public class PoliceOfficer : PublicServant, IPerson
	{
		private bool _hasEmergency = false;

		public PoliceOfficer (string name, int age, bool hasEmergency = false) {
			this.Name = name;
			this.Age = age;
			this.HasEmergency = hasEmergency;

			if (this.HasEmergency) {
				this.DriveToPlaceOfInterest += delegate {
					Console.WriteLine ("Driving the police car with siren");
					GetInPoliceCar ();
					TurnOnSiren ();
					FollowDirections ();
				};
			} else {
				this.DriveToPlaceOfInterest += delegate {
					Console.WriteLine ("Driving the police car");
					GetInPoliceCar ();
					FollowDirections ();
				};
			}
		}

		//implement the IPerson interface
		public string Name { get; set; }
		public int Age { get; set; }

		public bool HasEmergency {
			get { return _hasEmergency; }
			set { _hasEmergency = value; }
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
			Firefighter firefighter = new Firefighter("blossom" , 23);
			firefighter.PensionAmount = 5000;

			PrintNameAndAge(firefighter);
			PrintPensionAmount(firefighter);

			firefighter.DriveToPlaceOfInterest();

			PoliceOfficer officer = new PoliceOfficer("bubbles" , 25);
			officer.PensionAmount = 10000;

			PrintNameAndAge(officer);
			PrintPensionAmount(officer);

			officer.DriveToPlaceOfInterest();

			officer = new PoliceOfficer("buttercup", 26, true);
			PrintNameAndAge(officer);
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

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/IntroScripts/anonymous/anonymous.png)

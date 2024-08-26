![.NET Core Build and Test](https://github.com/shawnjots/ElevatorChallenge/actions/workflows/build.yml/badge.svg)

# Elevator Challenge

## Description
The Elevator Challenge is a C# application designed to simulate the operation of elevators within a multi-floor building. It provides functionalities such as dispatching elevators to requested floors, loading and unloading passengers, and moving elevators between floors based on passenger requests.

## Features
Dispatching the nearest available elevator to a requested floor
Loading passengers into elevators
Unloading passengers from elevators at their destination floors
Moving elevators between floors based on passenger requests
Real-time logging of elevator operations
Technologies Used
C# programming language
.NET Framework
NUnit testing framework for unit testing

## Project Layout
ElevatorChallenge: Contains the main application logic and classes, including the ElevatorService, Elevator, Passenger, and other related classes.
ElevatorChallenge.DTOs: Defines data transfer objects (DTOs) used for transferring data between different parts of the application.
ElevatorChallenge.Interfaces: Contains interfaces used for dependency injection and loose coupling of components.
ElevatorChallenge.Models: Defines models used to represent elevator-related entities.
ElevatorChallenge.Mappers: Contains classes responsible for mapping between DTOs and models.
ElevatorChallenge.Utilities: Contains utility classes and constants used throughout the application.
ElevatorChallenge.Tests: Contains unit tests for testing the functionality of the application.

## Getting started
Clone the repository to your local machine.
Open the solution in Visual Studio or your preferred IDE.
Build the solution to ensure all dependencies are resolved.
Run the unit tests to verify the functionality of the application.
Run the application and interact with it to simulate elevator operations.
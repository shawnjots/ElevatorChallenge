using ElevatorChallenge.DTOs.Passenger;
using ElevatorChallenge.Models;
using EventChallenge.Services;
using EventChallenge.Services.Interfaces;
using Moq;
using NUnit.Framework.Internal;

namespace ElevatorChallenge.UnitTests
{
	[TestFixture]
	public class Tests
	{
		private Mock<IMapper> _mapperMock;
		private ElevatorService _elevatorService;

		[SetUp]
		public void Setup()
		{
			_mapperMock = new Mock<IMapper>();
			_elevatorService = new ElevatorService(_mapperMock.Object);
		}

		[Test]
		public void Constructor_ShouldInitializeCollections()
		{
			Assert.IsNotNull(_elevatorService.PassengerEvents);
			Assert.IsNotNull(_elevatorService.ElevatorEvents);
		}


		[Test]
		public void QueuePassenger_ShouldInitializeFloorIfNotExist()
		{
			// Arrange
			var floor = 2;
			var passengerDto = new PassengerDTO { CurrentFloor = 1, DestinationFloor = 2 };
			var passenger = new Passenger { DestinationFloor = 2, Requestfloor = 1 };
			_mapperMock.Setup(m => m.Map<PassengerDTO, Passenger>(passengerDto)).Returns(passenger);

			// Act
			_elevatorService.QueuePassenger(floor, passengerDto);

			// Assert
			Assert.IsTrue(_elevatorService.GetFloors().ContainsKey(floor));
			Assert.IsTrue(_elevatorService.GetFloors()[floor].ScheduledPassengers.Contains(passenger));
		}

	}
}
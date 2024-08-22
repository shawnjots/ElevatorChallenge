namespace ElevatorChallenge.Models
{
	public class Elevator
	{
		#region FIELDS

		public int Id { get; private set; }
		private static int _id = 1;
		public int MaximumCapacity { get; private set; }
		public int CurrentFloor { get; set; }
		public List<Passenger> Passengers { get; private set; }
		public Direction CurrentDirection { get; set; }
		public Status CurrentStatus { get; set; }
		public bool IsAvailable => CurrentStatus == Status.Stationary && CurrentDirection == Direction.None;
		public bool IsFull => Passengers?.Count >= MaximumCapacity;

		#endregion

		#region METHODS

		public Elevator(int capacity)
		{
			Id = _id++;
			MaximumCapacity = capacity;
			CurrentFloor = 1;
			CurrentDirection = Direction.None;
			CurrentStatus = Status.Stationary;
			Passengers = new List<Passenger>();
		}

		public void LoadPassenger(Passenger passenger)
		{
			Passengers?.Add(passenger);
		}

		public void UnloadPassenger(Passenger passenger)
		{
			if (Passengers?.Contains(passenger) ?? false)
			{
				Passengers.Remove(passenger);
			}
			else
			{
				throw new InvalidOperationException("Passenger not on elevator");
			}
		}

		public void MoveToFloor(int floor)
		{
			if (CurrentFloor == floor)
			{
				CurrentStatus = Status.DoorsOpen;
				return;
			}

			CurrentStatus = Status.Moving;
			CurrentDirection = CurrentFloor < floor ? Direction.Up : Direction.Down;
			CurrentFloor = Math.Clamp(floor, 1, 23);
		}

		#endregion

		#region ENUMS

		public enum Direction
		{
			None,
			Up,
			Down
		}

		public enum Status
		{
			Stationary,
			DoorsOpen,
			Moving,
		}

		#endregion
	}
}
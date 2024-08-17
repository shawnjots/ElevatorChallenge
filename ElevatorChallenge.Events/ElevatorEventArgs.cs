namespace ElevatorChallenge.Events
{
	public class ElevatorEventArgs
	{
		public int ElevatorId { get; set; }
		public Status NewStatus { get; set; }
		public int CurrentFloor { get; set; }

		public enum Status
		{
			Moving,
			Stationary,
			DoorsOpen
		}
	}
}

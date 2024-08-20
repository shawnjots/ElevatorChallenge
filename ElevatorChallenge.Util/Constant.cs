namespace ElevatorChallenge.Util
{
	public static class Constant
	{
		public static int MinFloor = 1;
		public static int MaxFloor = 10;
		public static double LoadSensitivity { get; set; } = 0.3;
		public static double DensityWeight { get; set; } = 0.3;
		public static int MaxElevators = 4;
		public static int MaxPassengers = 8;
		public static int MaxWaitPeriod { get; set; } = 60;
		public static int PassengerOptimizationLimit { get; set; }
		public static double WaitFactor { get; set; } = 0.5;
		public static byte DensityLimit { get; set; } = 8;
		public static double DensityFactor { get; set; } = 0.4;
		public static int WaitTimeFactor { get; set; } = 2;
		public static int ProximityFactor { get; set; } = 2;

	}
}

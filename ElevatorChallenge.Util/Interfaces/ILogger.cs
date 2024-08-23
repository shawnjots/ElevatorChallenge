using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.Util.Interfaces
{
	public interface ILogger
	{
		void LogInformation(string message);
		void LogInforation(Exception ex);
	}
}
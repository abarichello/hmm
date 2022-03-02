using System;

namespace HeavyMetalMachines.CompetitiveMode.Prizes.Exceptions
{
	public class DivisionNotFoundException : Exception
	{
		public DivisionNotFoundException(int divisionIndex) : base(string.Format("Divison of index {0} could not be found.", divisionIndex))
		{
		}
	}
}

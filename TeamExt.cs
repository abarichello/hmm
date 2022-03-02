using System;
using System.Text;
using ClientAPI.Objects;

namespace HeavyMetalMachines
{
	public static class TeamExt
	{
		public static string WriteToString(this Team team)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[Team=");
			stringBuilder.Append(team.Name);
			stringBuilder.Append(" Id=");
			stringBuilder.Append(team.Id);
			stringBuilder.Append(" Tag=");
			stringBuilder.Append(team.Tag);
			stringBuilder.Append(" Bag=");
			stringBuilder.Append(team.Bag);
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}
	}
}

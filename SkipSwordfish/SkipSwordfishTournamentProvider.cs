using System;
using System.Collections.Generic;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.Infra;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTournamentProvider : ITournamentProvider
	{
		public IObservable<TournamentConfiguration[]> GetAllActive()
		{
			return Observable.Return<TournamentConfiguration[]>(this.CreateTournamentConfigurations());
		}

		public IObservable<Tournament[]> GetAllActiveWithTeamStatus(Guid teamId)
		{
			return Observable.Return<Tournament[]>(this.CreateTournaments());
		}

		private TournamentConfiguration[] CreateTournamentConfigurations()
		{
			int num = 4;
			List<TournamentConfiguration> list = new List<TournamentConfiguration>();
			for (int i = 0; i < num; i++)
			{
				list.Add(this.CreateConfiguration(i, "REGION_SOUTH_AMERICA", i < num / 2));
			}
			for (int j = 0; j < num; j++)
			{
				int id = j + num;
				list.Add(this.CreateConfiguration(id, "REGION_EUROPE", j < num / 2));
			}
			return list.ToArray();
		}

		private Tournament[] CreateTournaments()
		{
			int num = 4;
			Tournament[] array = new Tournament[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.CreateTournament(i, i < num / 2);
			}
			return array;
		}

		private Tournament CreateTournament(int id, bool isBeginner)
		{
			Tournament tournament = new Tournament
			{
				Id = (long)id
			};
			tournament.Configuration = this.CreateConfiguration(id, "REGION_SOUTH_AMERICA", true);
			return tournament;
		}

		private TournamentConfiguration CreateConfiguration(int id, string regionName, bool isBeginner)
		{
			DateTime utcNow = DateTime.UtcNow;
			TournamentConfiguration result = default(TournamentConfiguration);
			result.Id = (long)id;
			result.TierId = ((!isBeginner) ? 2L : 1L);
			result.RegionName = regionName;
			result.Name = ((!isBeginner) ? "TOURNAMENT_LIST_PRO_NAME" : "TOURNAMENT_LIST_BEGINNER_NAME");
			result.Season = "10";
			result.Description = ((!isBeginner) ? "TOURNAMENT_REWARDS_DESCRIPTION_PRO" : "TOURNAMENT_REWARDS_DESCRIPTION_BEGINNER");
			result.LogoImageName = ((!isBeginner) ? "logo_tournament_top" : "logo_tournament_beginner");
			result.LogoImageNameSmall = ((!isBeginner) ? "logo_list_tournament_pro_logo" : "logo_list_tournament_beginner_logo");
			result.Begin = utcNow;
			result.End = utcNow + TimeSpan.FromDays(10.0);
			result.SortOrder = ((!isBeginner) ? 1 : 0);
			result.Criteria = this.GetCriteria(isBeginner);
			result.Steps = new List<Step>
			{
				new Step
				{
					Id = 1L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(1.0),
							End = utcNow + TimeSpan.FromMinutes(2.0)
						}
					}
				},
				new Step
				{
					Id = 2L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(3.0),
							End = utcNow + TimeSpan.FromMinutes(4.0)
						}
					}
				},
				new Step
				{
					Id = 3L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(5.0),
							End = utcNow + TimeSpan.FromMinutes(6.0)
						}
					}
				},
				new Step
				{
					Id = 4L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(7.0),
							End = utcNow + TimeSpan.FromMinutes(8.0)
						}
					}
				},
				new Step
				{
					Id = 5L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(9.0),
							End = utcNow + TimeSpan.FromMinutes(10.0)
						}
					}
				},
				new Step
				{
					Id = 6L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(11.0),
							End = utcNow + TimeSpan.FromMinutes(12.0)
						}
					}
				},
				new Step
				{
					Id = 7L,
					Name = string.Empty,
					Periods = new StepPeriod[]
					{
						new StepPeriod
						{
							Begin = utcNow + TimeSpan.FromMinutes(13.0),
							End = utcNow + TimeSpan.FromMinutes(14.0)
						}
					}
				}
			};
			return result;
		}

		private TournamentCriteria GetCriteria(bool isBeginner)
		{
			return new TournamentCriteria
			{
				Victories = new MinMaxTuple
				{
					Min = ((!isBeginner) ? new int?(61) : null),
					Max = ((!isBeginner) ? null : new int?(60))
				},
				CompetitiveDivision = new MinMaxTuple
				{
					Min = ((!isBeginner) ? new int?(1) : null),
					Max = ((!isBeginner) ? null : new int?(0))
				},
				CompetitiveSubdivision = new MinMaxTuple
				{
					Min = ((!isBeginner) ? new int?(4) : null),
					Max = ((!isBeginner) ? null : new int?(0))
				},
				CriteriaOperatorDraft = ((!isBeginner) ? "TOURNAMENT_CRITERION_OR_LABEL" : "TOURNAMENT_CRITERION_AND_LABEL"),
				LogoDraft = ((!isBeginner) ? "TOURNAMENT_PRO_NAME" : "TOURNAMENT_BEGINNER_NAME"),
				TitleDraft = ((!isBeginner) ? "TOURNAMENT_LIST_PRO_NAME" : "TOURNAMENT_LIST_BEGINNER_NAME")
			};
		}
	}
}

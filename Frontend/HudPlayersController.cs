using System;
using System.Collections.Generic;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudPlayersController : HudWindow, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public GameObject PlayersIconsContainerObject
		{
			get
			{
				return this.WindowGameObject;
			}
		}

		public void Awake()
		{
			this.PlayerObjectReference.gameObject.SetActive(false);
			this.WindowGameObject.SetActive(true);
			base.SetWindowVisibility(false);
			this.ApplyArenaConfig();
		}

		private void ApplyArenaConfig()
		{
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			this._allyTeamOnRightSide = (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == gameArenaInfo.TugOfWarFlipTeam);
			if (this._allyTeamOnRightSide)
			{
				this._allyComponents = new HudPlayersController.HudPlayersTeamComponents
				{
					Grid = this.RightPlayerStatsGrid,
					IconRotation = this.RightTeamIconRotation
				};
				this._enemyComponents = new HudPlayersController.HudPlayersTeamComponents
				{
					Grid = this.LeftPlayerStatsGrid,
					IconRotation = Quaternion.identity
				};
			}
			else
			{
				this._allyComponents = new HudPlayersController.HudPlayersTeamComponents
				{
					Grid = this.LeftPlayerStatsGrid,
					IconRotation = Quaternion.identity
				};
				this._enemyComponents = new HudPlayersController.HudPlayersTeamComponents
				{
					Grid = this.RightPlayerStatsGrid,
					IconRotation = this.RightTeamIconRotation
				};
			}
		}

		public void TutorialAddPlayers()
		{
			this.BuildCompleteAllPlayers(GameHubBehaviour.Hub.Players.PlayersAndBots);
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			this._playerBuildCount++;
			if (this._playerBuildCount < GameHubBehaviour.Hub.Players.Players.Count + GameHubBehaviour.Hub.Players.Bots.Count)
			{
				return;
			}
			this.BuildCompleteAllPlayers(GameHubBehaviour.Hub.Players.PlayersAndBots);
		}

		private void BuildCompleteAllPlayers(List<PlayerData> playerDatas)
		{
			HudUtils.PlayerDataComparer comparer = new HudUtils.PlayerDataComparer(GameHubBehaviour.Hub, HudUtils.PlayerDataComparer.PlayerDataComparerType.InstanceId);
			playerDatas.Sort(comparer);
			int num = (!GameHubBehaviour.Hub.User.IsNarrator) ? GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId : -1;
			for (int i = 0; i < playerDatas.Count; i++)
			{
				PlayerData playerData = playerDatas[i];
				if (playerData.CharacterInstance.ObjId == num)
				{
					this.BuildPlayerStats(playerData, true);
					break;
				}
			}
			for (int j = 0; j < playerDatas.Count; j++)
			{
				PlayerData playerData2 = playerDatas[j];
				bool flag = playerData2.CharacterInstance.ObjId == num;
				if (!flag)
				{
					if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == playerData2.Team)
					{
						this.BuildPlayerStats(playerData2, true);
					}
				}
			}
			for (int k = 0; k < playerDatas.Count; k++)
			{
				PlayerData playerData3 = playerDatas[k];
				if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team != playerData3.Team)
				{
					this.BuildPlayerStats(playerData3, false);
				}
			}
			this.LeftPlayerStatsGrid.Reposition();
			this.RightPlayerStatsGrid.Reposition();
			this._playerBuildCount = 0;
		}

		private void BuildPlayerStats(PlayerData playerdata, bool isAlly)
		{
			HudPlayersObject hudPlayersObject = UnityEngine.Object.Instantiate<HudPlayersObject>(this.PlayerObjectReference, Vector3.zero, Quaternion.identity);
			Transform transform;
			if (isAlly)
			{
				transform = this._allyComponents.Grid.transform;
				hudPlayersObject.Thumb.transform.rotation = this._allyComponents.IconRotation;
				hudPlayersObject.SetBotControlledTeamIconColor(this.AllyColor);
			}
			else
			{
				transform = this._enemyComponents.Grid.transform;
				hudPlayersObject.Thumb.transform.rotation = this._enemyComponents.IconRotation;
				hudPlayersObject.SetBotControlledTeamIconColor(this.EnemyColor);
			}
			hudPlayersObject.transform.parent = transform;
			hudPlayersObject.transform.localScale = transform.localScale;
			hudPlayersObject.Setup(playerdata.CharacterInstance.GetBitComponent<PlayerStats>(), playerdata.CharacterInstance.GetBitComponent<CombatObject>(), playerdata.CharacterInstance.GetBitComponent<SpawnController>(), playerdata.UserId);
			hudPlayersObject.PlayerNameLabel.text = playerdata.Name;
			hudPlayersObject.HidePlayerNameLabel();
			string playerIconName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, playerdata.Character.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size64);
			hudPlayersObject.Thumb.SpriteName = playerIconName;
			hudPlayersObject.ThumbGrey.enabled = false;
			bool isCurrentPlayer = GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerId == playerdata.PlayerId;
			bool isRightSide = isAlly == this._allyTeamOnRightSide;
			hudPlayersObject.SetTeamColor(isCurrentPlayer, isAlly, isRightSide);
			hudPlayersObject.gameObject.SetActive(true);
		}

		public HudPlayersObject PlayerObjectReference;

		public UIGrid LeftPlayerStatsGrid;

		public UIGrid RightPlayerStatsGrid;

		public Color AllyColor;

		public Color EnemyColor;

		public Quaternion RightTeamIconRotation;

		private HudPlayersController.HudPlayersTeamComponents _allyComponents;

		private HudPlayersController.HudPlayersTeamComponents _enemyComponents;

		private bool _allyTeamOnRightSide;

		private int _playerBuildCount;

		private struct HudPlayersTeamComponents
		{
			public UIGrid Grid;

			public Quaternion IconRotation;
		}
	}
}

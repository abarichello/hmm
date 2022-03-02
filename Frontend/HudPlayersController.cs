using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Players.Presenting;
using Pocketverse;
using UnityEngine;
using Zenject;

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
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			this._allyTeamOnRightSide = (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == currentArena.TugOfWarFlipTeam);
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
			int instanceId = GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId.GetInstanceId();
			HudUtils.PlayerDataComparer comparer = new HudUtils.PlayerDataComparer(instanceId, HudUtils.PlayerDataComparer.PlayerDataComparerType.InstanceId);
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
			Transform transform = (!isAlly) ? this._enemyComponents.Grid.transform : this._allyComponents.Grid.transform;
			GameObject gameObject = this._container.InstantiatePrefab(this.PlayerObjectReference, Vector3.zero, Quaternion.identity, transform);
			HudPlayersObject component = gameObject.GetComponent<HudPlayersObject>();
			if (isAlly)
			{
				component.Thumb.transform.rotation = this._allyComponents.IconRotation;
				component.SetBotControlledTeamIconColor(this.AllyColor);
			}
			else
			{
				component.Thumb.transform.rotation = this._enemyComponents.IconRotation;
				component.SetBotControlledTeamIconColor(this.EnemyColor);
			}
			component.transform.parent = transform;
			component.transform.localScale = transform.localScale;
			component.Setup(playerdata.CharacterInstance.GetBitComponent<PlayerStats>(), playerdata.CharacterInstance.GetBitComponent<CombatObject>(), playerdata.CharacterInstance.GetBitComponent<SpawnController>(), playerdata.UserId);
			IGetDisplayableNickName getDisplayableNickName = this._container.Resolve<IGetDisplayableNickName>();
			component.PlayerNameLabel.text = ((!playerdata.IsBot) ? getDisplayableNickName.GetFormattedNickNameWithPlayerTag(playerdata.PlayerId, playerdata.Name, new long?(playerdata.PlayerTag)) : playerdata.Name);
			component.HidePlayerNameLabel();
			string playerIconName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, playerdata.Character.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size64);
			component.Thumb.SpriteName = playerIconName;
			component.ThumbGrey.enabled = false;
			bool isCurrentPlayer = GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerId == playerdata.PlayerId;
			bool isRightSide = isAlly == this._allyTeamOnRightSide;
			component.SetTeamColor(isCurrentPlayer, isAlly, isRightSide);
			component.gameObject.SetActive(true);
		}

		[Inject]
		private DiContainer _container;

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

using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public class HMMContext : GameHubObject, IHMMContext
	{
		public HMMContext(IGameCamera gameCamera)
		{
			this._gameCamera = gameCamera;
		}

		public IGameCamera GameCamera
		{
			get
			{
				return this._gameCamera;
			}
		}

		public ICombatObject Bomb
		{
			get
			{
				return GameHubObject.Hub.BombManager.BombMovement.Combat;
			}
		}

		public ICombatObject[] RedTeam
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ICombatObject[] BlueTeam
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ICombatObject GetCombatObject(int id)
		{
			return CombatRef.GetCombat(id);
		}

		public ICombatObject GetCombatObject(Component component)
		{
			return CombatRef.GetCombat(component);
		}

		public IIdentifiable GetIdentifiable(int id)
		{
			return GameHubObject.Hub.ObjectCollection.GetObject(id);
		}

		public IArena Arena
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IScoreBoard ScoreBoard
		{
			get
			{
				return GameHubObject.Hub.BombManager.ScoreBoard;
			}
		}

		public IStats Stats
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IGadgetHud GadgetHud
		{
			get
			{
				return GameHubObject.Hub.State.Current.GetStateGuiController<GameGui>().GadgetHud;
			}
		}

		public IGameTime Clock
		{
			get
			{
				return GameHubObject.Hub.GameTime;
			}
		}

		public bool IsClient
		{
			get
			{
				return GameHubObject.Hub.Net.IsClient();
			}
		}

		public bool IsServer
		{
			get
			{
				return GameHubObject.Hub.Net.IsServer();
			}
		}

		public bool IsTest
		{
			get
			{
				return GameHubObject.Hub.Net.IsTest();
			}
		}

		public bool IsCarryingBomb(ICombatObject combatObject)
		{
			return GameHubObject.Hub.BombManager.IsCarryingBomb(combatObject.Identifiable.ObjId);
		}

		public IHudIconBar GetHudIconBar(ICombatObject combatObject)
		{
			GameGui stateGuiController = GameHubObject.Hub.State.Current.GetStateGuiController<GameGui>();
			HudLifebarPlayerObject lifebarObject = stateGuiController.HudLifebarController.GetLifebarObject(combatObject.Identifiable.ObjId);
			return lifebarObject.IconBar;
		}

		public IStateMachine StateMachine
		{
			get
			{
				return GameHubObject.Hub.State;
			}
		}

		public IHudEmotePresenter GetHudEmote(ICombatObject combatObject)
		{
			GameGui stateGuiController = GameHubObject.Hub.State.Current.GetStateGuiController<GameGui>();
			HudLifebarPlayerObject lifebarObject = stateGuiController.HudLifebarController.GetLifebarObject(combatObject.Identifiable.ObjId);
			return lifebarObject.hudEmotePresenter;
		}

		private IGameCamera _gameCamera;
	}
}

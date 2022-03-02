using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	[Serializable]
	public class PlayerData : GameHubObject, IBitStreamSerializable, IPlayerData
	{
		public PlayerData(BitStream stream)
		{
			this.ReadFromBitStream(stream);
		}

		public PlayerData(string userId, int slot, TeamKind team, byte address, int gridIndex, bool isBot, int botId, bool isNarrator, BattlepassProgress battlepassProgress)
		{
			this.IsNarrator = isNarrator;
			this.IsBot = isBot;
			this.BotId = botId;
			this.UserId = userId;
			this.TeamSlot = slot;
			this.Team = team;
			this.PlayerAddress = address;
			this.GridIndex = gridIndex;
			this.BattlepassProgress = battlepassProgress;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToPlayerDisconnected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToPlayerReconnected;

		public static bool operator ==(PlayerData a, PlayerData b)
		{
			return object.ReferenceEquals(a, b) || (a != null && b != null && a.PlayerAddress == b.PlayerAddress);
		}

		public static bool operator !=(PlayerData a, PlayerData b)
		{
			return !(a == b);
		}

		public override bool Equals(object other)
		{
			PlayerData playerData = other as PlayerData;
			if (playerData != null)
			{
				return this == playerData;
			}
			return base.Equals(other);
		}

		public override int GetHashCode()
		{
			return this.PlayerId.GetHashCode();
		}

		public IItemType CharacterItemType
		{
			get
			{
				return this._characterItemType;
			}
		}

		public User User
		{
			get
			{
				return this.UserSF;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PlayerData> ListenToPlayerConnectionStatusChanged;

		public bool Connected
		{
			get
			{
				return this._connected;
			}
			set
			{
				if (this._connected == value)
				{
					return;
				}
				this._connected = value;
				if (this.ListenToPlayerConnectionStatusChanged != null)
				{
					this.ListenToPlayerConnectionStatusChanged(this);
				}
			}
		}

		public PlayerBag Bag
		{
			get
			{
				return this._bag;
			}
			set
			{
				this._bag = value;
				this.Level = ((this._bag != null) ? this._bag.Level : 0);
			}
		}

		public Character[] SwordfishCharacters
		{
			get
			{
				return this._characters;
			}
		}

		public long PlayerId
		{
			get
			{
				return (this.PlayerSF != null) ? this.PlayerSF.Id : -1L;
			}
			set
			{
				if (this.PlayerSF != null)
				{
					return;
				}
				this.PlayerSF = new Player
				{
					Id = value
				};
			}
		}

		private void OnError(object state, Exception exception)
		{
			PlayerData.Log.Fatal("Swordfish exception", exception);
			Action<PlayerData> action = state as Action<PlayerData>;
			if (action != null)
			{
				action(this);
				return;
			}
			Action<int[]> action2 = state as Action<int[]>;
			if (action2 != null)
			{
				action2(new int[0]);
				return;
			}
			IFuture future = state as IFuture;
			if (future != null)
			{
				future.ExceptionThrowed = exception;
				return;
			}
		}

		public void SetPlayerData(ServerPlayerData serverPlayerData)
		{
			this.UserId = serverPlayerData.User.UniversalID;
			this.UserSF = serverPlayerData.User;
			this.PlayerSF = serverPlayerData.Player;
			this.Bag = (PlayerBag)this.PlayerSF.Bag;
			this.MMR = serverPlayerData.MMR;
			this.IsRookie = serverPlayerData.IsRookie;
			long? nameTag = serverPlayerData.Player.NameTag;
			this.PlayerTag = ((nameTag == null) ? -1L : nameTag.Value);
			this.FounderLevel = this.Bag.FounderPackLevel;
			this.Name = this.PlayerSF.Name;
			this._characters = serverPlayerData.Characters;
			this.SetPlayerItems(serverPlayerData.PlayerItems);
			PlayerData.Log.DebugFormat("Server player data loaded user={0} mmr={1} name={2}, IsRookie={3}, PlayerTag={4}", new object[]
			{
				this.UserId,
				this.MMR,
				this.Name,
				this.IsRookie,
				this.PlayerTag
			});
		}

		private void SetPlayerItems(ServerPlayerItems serverPlayerItems)
		{
			if (serverPlayerItems.CharacterItems != null)
			{
				int[] array = new int[serverPlayerItems.CharacterItems.Length];
				this._ownedCharacters = new int[serverPlayerItems.CharacterItems.Length];
				for (int i = 0; i < serverPlayerItems.CharacterItems.Length; i++)
				{
					Item item = serverPlayerItems.CharacterItems[i];
					int num;
					if (GameHubObject.Hub.InventoryColletion.CharacterTypesToId.TryGetValue(item.ItemTypeId, out num))
					{
						array[i] = num;
					}
					else
					{
						array[i] = -1;
					}
				}
				this._ownedCharacters = array;
			}
			else
			{
				this._ownedCharacters = new int[0];
			}
			if (serverPlayerItems.Customization != null)
			{
				this.Customizations = serverPlayerItems.Customization;
				this.Customizations.SyncDictionary();
				PlayerData.Log.InfoFormat("Set Customization. PlayerID ={0} Customizations={1}", new object[]
				{
					this.PlayerId,
					this.Customizations.ToString()
				});
			}
		}

		public void ReloadCustomizations(IFuture future)
		{
			Future future2 = new Future();
			future.DependsOn(future2);
			GameHubObject.Hub.ClientApi.character.GetAllCharacters(future2, this.PlayerSF.Id, new SwordfishClientApi.ParameterizedCallback<Character[]>(this.OnReloadCustomizations), new SwordfishClientApi.ErrorCallback(this.OnError));
		}

		private void OnReloadCustomizations(object state, Character[] obj)
		{
			this._characters = obj;
			IFuture future = state as IFuture;
			if (future != null)
			{
				future.Result = null;
			}
		}

		public void LoadPlayerInventory(Action<int[]> whenDone = null)
		{
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				if (this._ownedCharacters == null)
				{
					this._ownedCharacters = new int[0];
				}
				if (whenDone != null)
				{
					whenDone(this._ownedCharacters);
				}
				return;
			}
			if (this._ownedCharacters != null)
			{
				if (whenDone != null)
				{
					whenDone(this._ownedCharacters);
				}
				return;
			}
			new PlayerData.InventoryLoadingState(whenDone, this);
		}

		public bool IsBot { get; set; }

		public TeamKind Team { get; set; }

		public int GetPlayerCarObjectId()
		{
			return (!this.IsNarrator) ? ObjectId.New(ContentKind.PlayerCar.Byte(), (this.TeamSlot << 1) + ((this.Team != TeamKind.Red) ? 1 : 0)) : -1;
		}

		public int CharacterId
		{
			get
			{
				return this._characterId;
			}
		}

		public void SetCharacter(int charId, ICollectionScriptableObject inventoryCollection)
		{
			if (this._characterId != -1)
			{
				PlayerData.Log.FatalFormat("Trying to set character more than once for Player={0} character={1}", new object[]
				{
					this.PlayerAddress,
					this._characterId
				});
				return;
			}
			this._characterId = charId;
			Dictionary<int, IItemType> allCharactersByCharacterId = inventoryCollection.AllCharactersByCharacterId;
			allCharactersByCharacterId.TryGetValue(this._characterId, out this._characterItemType);
			if (this._characterItemType == null)
			{
				PlayerData.Log.ErrorFormat("Invalid character id={0} for player={1}", new object[]
				{
					charId,
					this.PlayerAddress
				});
			}
		}

		public void SetCharacter(Guid guid, ICollectionScriptableObject collectionScriptableObject)
		{
			this._characterItemType = collectionScriptableObject.Get(guid);
			this._characterId = this._characterItemType.GetComponent<CharacterItemTypeComponent>().CharacterId;
		}

		public Identifiable CharacterInstance
		{
			get
			{
				if (this._characterInstance == null)
				{
					if (GameHubObject.Hub == null)
					{
						return null;
					}
					if (GameHubObject.Hub.ObjectCollection == null)
					{
						return null;
					}
					this._characterInstance = GameHubObject.Hub.ObjectCollection.GetObject(this.GetPlayerCarObjectId());
				}
				return this._characterInstance;
			}
			set
			{
				this._characterInstance = value;
			}
		}

		public int PlayerCarId
		{
			get
			{
				return (!(this.CharacterInstance == null)) ? this.CharacterInstance.ObjId : this.GetPlayerCarObjectId();
			}
		}

		public bool IsCurrentPlayer
		{
			get
			{
				return GameHubObject.Hub.Net.IsClient() && GameHubObject.Hub.Players.CurrentPlayerData != null && GameHubObject.Hub.Players.CurrentPlayerData.PlayerCarId == this.PlayerCarId;
			}
		}

		public CharacterInfo Character
		{
			get
			{
				return this._character;
			}
			set
			{
				this._character = value;
			}
		}

		public virtual IPlayerStats PlayerStats
		{
			get
			{
				return this.CharacterInstance.GetBitComponent<PlayerStats>();
			}
		}

		public void UpdateCharacters()
		{
			if (!GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				GameHubObject.Hub.ClientApi.character.GetAllCharacters(null, this.PlayerId, new SwordfishClientApi.ParameterizedCallback<Character[]>(this.OnGetPlayerCharactersSuccess), new SwordfishClientApi.ErrorCallback(this.OnGetCharactersError));
			}
		}

		private void OnGetPlayerCharactersSuccess(object state, Character[] obj)
		{
			this._characters = obj;
		}

		private void OnGetCharactersError(object state, Exception exception)
		{
			PlayerData.Log.ErrorFormat("Error when Updating player characters. PlayerId:{0}, Exception:{1}", new object[]
			{
				this.PlayerId,
				exception
			});
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PlayerData> ListenToBotControlChanged;

		public bool IsBotControlled
		{
			get
			{
				return this._isBotControlled;
			}
			set
			{
				if (this._isBotControlled == value)
				{
					return;
				}
				this._isBotControlled = value;
				if (this.ListenToBotControlChanged != null)
				{
					this.ListenToBotControlChanged(this);
				}
			}
		}

		public void AddOnline()
		{
			GameHubObject.Hub.AddOnline(this.PlayerAddress, this.Team);
		}

		public void RemoveOnline()
		{
			GameHubObject.Hub.RemoveOnline(this.PlayerAddress);
		}

		public string GetDrivingStylesUsed()
		{
			if (!this.CharacterInstance)
			{
				return "null";
			}
			PlayerController bitComponent = this.CharacterInstance.GetBitComponent<PlayerController>();
			if (bitComponent == null)
			{
				return "null";
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			for (int i = 0; i < bitComponent.DrivingStylesUsed.Length; i++)
			{
				if (bitComponent.DrivingStylesUsed[i])
				{
					if (flag)
					{
						stringBuilder.Append(",");
					}
					StringBuilder stringBuilder2 = stringBuilder;
					CarInput.DrivingStyleKind drivingStyleKind = (CarInput.DrivingStyleKind)i;
					stringBuilder2.Append(drivingStyleKind.ToString());
					flag = true;
				}
			}
			if (!flag)
			{
				return "null";
			}
			return stringBuilder.ToString();
		}

		public bool CharSkinGridConfirmed
		{
			get
			{
				return this._characterId >= 0 && this.GridIndex >= 0;
			}
		}

		public bool CharConfirmed
		{
			get
			{
				return this._characterId >= 0;
			}
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedLong(this.PlayerId);
			bs.WriteString(this.UserId);
			bs.WriteBool(this.IsBot);
			bs.WriteCompressedInt(this.BotId);
			bs.WriteBool(this.IsBotControlled);
			bs.WriteBool(this.IsNarrator);
			bs.WriteBool(this.IsLeaver);
			bs.WriteString(this.Name);
			bs.WriteString(this.PublisherUserName);
			bs.WriteCompressedInt(this.PublisherId);
			bs.WriteCompressedInt(this._characterId);
			bs.WriteGuid(this.Customizations.GetGuidBySlot(1));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(5));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(2));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(4));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(3));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(60));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(59));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(40));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(41));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(42));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(43));
			bs.WriteGuid(this.Customizations.GetGuidBySlot(44));
			bs.WriteByte(this.PlayerAddress);
			bs.WriteTeamKind(this.Team);
			bs.WriteBits(4, this.TeamSlot);
			bs.WriteCompressedInt(this.GridIndex);
			bs.WriteBool(this.Connected);
			bs.WriteByte((byte)this.Level);
			bs.WriteByte(this.FounderLevel);
			bs.WriteBool(this.IsRookie);
			bs.WriteCompressedLong(this.PlayerTag);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.PlayerId = bs.ReadCompressedLong();
			this.UserId = bs.ReadString();
			this.IsBot = bs.ReadBool();
			this.BotId = bs.ReadCompressedInt();
			this.IsBotControlled = bs.ReadBool();
			this.IsNarrator = bs.ReadBool();
			this.IsLeaver = bs.ReadBool();
			this.Name = bs.ReadString();
			this.PublisherUserName = bs.ReadString();
			this.PublisherId = bs.ReadCompressedInt();
			int num = bs.ReadCompressedInt();
			this.Customizations.SetGuidAndSlot(1, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(5, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(2, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(4, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(3, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(60, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(59, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(40, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(41, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(42, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(43, bs.ReadGuid());
			this.Customizations.SetGuidAndSlot(44, bs.ReadGuid());
			this.PlayerAddress = bs.ReadByte();
			this.Team = bs.ReadTeamKind();
			this.TeamSlot = bs.ReadBits(4);
			this.GridIndex = bs.ReadCompressedInt();
			this.Connected = bs.ReadBool();
			this.Level = (int)bs.ReadByte();
			this.FounderLevel = bs.ReadByte();
			this.IsRookie = bs.ReadBool();
			this.PlayerTag = bs.ReadCompressedLong();
			if (num != this._characterId)
			{
				this.SetCharacter(num, GameHubObject.Hub.InventoryColletion);
			}
			PlayerData.Log.InfoFormat("Set Customization. PlayerID ={0} Customizations={1}", new object[]
			{
				this.PlayerId,
				this.Customizations.ToString()
			});
		}

		public override string ToString()
		{
			CharacterTarget characterTarget = (this.CharacterItemType == null) ? CharacterTarget.None : this.GetCharacter();
			return string.Format("Name={0} Character={1} IsCurrentPlayer={2} IsBot={3} CharId={4} Address={5} PlayerCarId={6} Team={7}/{8} GroupId={9} Connected={10} Leaver={11} HasCounselor={12}", new object[]
			{
				this.Name,
				characterTarget.ToString(),
				this.IsCurrentPlayer,
				this.IsBot,
				(!this.IsNarrator) ? this.CharacterId.ToString() : "Narrator",
				this.PlayerAddress,
				this.PlayerCarId,
				this.Team,
				this.TeamSlot,
				this.GroupId,
				this.Connected,
				this.IsLeaver,
				this.HasCounselor
			});
		}

		public int[] GetOwnedCharacters()
		{
			return this._ownedCharacters ?? new int[0];
		}

		public void ServerPlayerDisconnected()
		{
			if (this.ServerListenToPlayerDisconnected != null)
			{
				this.ServerListenToPlayerDisconnected();
			}
		}

		public void ServerPlayerReconnected()
		{
			if (this.ServerListenToPlayerReconnected != null)
			{
				this.ServerListenToPlayerReconnected();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PlayerData));

		protected IItemType _characterItemType;

		public string UserId;

		public User UserSF;

		private bool _connected;

		[NonSerialized]
		public bool Ready;

		[NonSerialized]
		public BattlepassProgress BattlepassProgress;

		public Player PlayerSF;

		public string Name;

		public int PublisherId;

		public string PublisherUserName;

		public long PlayerTag;

		private PlayerBag _bag;

		public int Level;

		public CharacterBag CharBag;

		public FounderLevel FounderLevel;

		public CustomizationContent Customizations = new CustomizationContent();

		private Character[] _characters;

		private int[] _ownedCharacters;

		public bool IsRookie;

		public Guid GroupId;

		public int BotId;

		public byte PlayerAddress;

		public int TeamSlot;

		public int GridIndex = -1;

		private int _characterId = -1;

		private Identifiable _characterInstance;

		private CharacterInfo _character;

		public bool IsLeaver;

		private bool _isBotControlled;

		public bool HasCounselor;

		public bool IsNarrator;

		public int MMR;

		public int SelectedChar = -1;

		public int SelectedGridIndex = -1;

		public int autoDesiredGrid = -1;

		public int autoGridPriority = -1;

		private class InventoryLoadingState
		{
			public InventoryLoadingState(Action<int[]> whenDone, PlayerData playerData)
			{
				this._playerData = playerData;
				this._whenDone = whenDone;
				ServerPlayer.GetServerPlayerItems(this._playerData.PlayerId, new SwordfishClientApi.ParameterizedCallback<string>(this.OnItemsLoaded), new SwordfishClientApi.ErrorCallback(this.OnLoadError));
			}

			private void OnLoadError(object state, Exception exception)
			{
				PlayerData.Log.Fatal("Failed to load inventories.", exception);
				this._playerData._ownedCharacters = new int[0];
				if (this._whenDone != null)
				{
					this._whenDone(this._playerData._ownedCharacters);
				}
			}

			private void Finalized()
			{
				if (this._whenDone != null)
				{
					this._whenDone(this._playerData._ownedCharacters);
				}
				this._whenDone = null;
				this._playerData = null;
			}

			private void OnItemsLoaded(object state, string strServerPlayerItems)
			{
				ServerPlayerItems playerItems = (ServerPlayerItems)((JsonSerializeable<!0>)strServerPlayerItems);
				this._playerData.SetPlayerItems(playerItems);
				this.Finalized();
			}

			private Action<int[]> _whenDone;

			private PlayerData _playerData;
		}
	}
}

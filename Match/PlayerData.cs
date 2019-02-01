using System;
using System.Diagnostics;
using System.Text;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	[Serializable]
	public class PlayerData : GameHubObject, IBitStreamSerializable
	{
		public PlayerData(BitStream stream)
		{
			this.ReadFromBitStream(stream);
		}

		public PlayerData(string userId, int slot, TeamKind team, byte address, int gridIndex, bool isBot, bool isNarrator, BattlepassProgress battlepassProgress)
		{
			this.IsNarrator = isNarrator;
			this.IsBot = isBot;
			this.UserId = userId;
			this.TeamSlot = slot;
			this.Team = team;
			this.PlayerAddress = address;
			this.GridIndex = gridIndex;
			this.BattlepassProgress = battlepassProgress;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event System.Action ServerListenToPlayerDisconnected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event System.Action ServerListenToPlayerReconnected;

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

		public string CharName
		{
			get
			{
				return (!(this.Character != null)) ? string.Empty : this.Character.Asset;
			}
		}

		public string CharLocalizedName
		{
			get
			{
				return (!(this.Character != null)) ? string.Empty : this.Character.LocalizedName;
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
			this.FounderLevel = (FounderLevel)this.Bag.FounderPackLevel;
			this.Name = this.PlayerSF.Name;
			this._characters = serverPlayerData.Characters;
			this.SetPlayerItems(serverPlayerData.PlayerItems);
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

		public void SetCharacter(int charId)
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
			GameHubObject.Hub.InventoryColletion.AvailableCharactersByInfoId.TryGetValue(this._characterId, out this._character);
			if (this._character == null)
			{
				PlayerData.Log.ErrorFormat("Invalid character id={0} for player={1}", new object[]
				{
					charId,
					this.PlayerAddress
				});
			}
		}

		public Identifiable CharacterInstance
		{
			get
			{
				return (!(this._characterInstance != null)) ? (this._characterInstance = GameHubObject.Hub.ObjectCollection.GetObject(this.GetPlayerCarObjectId())) : this._characterInstance;
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
			bs.WriteBool(this.IsBotControlled);
			bs.WriteBool(this.IsNarrator);
			bs.WriteBool(this.IsLeaver);
			bs.WriteString(this.Name);
			bs.WriteCompressedInt(this._characterId);
			bs.WriteGuid(this.Customizations.SelectedSprayItemTypeId);
			bs.WriteGuid(this.Customizations.SelectedRespawnVfxItemTypeId);
			bs.WriteGuid(this.Customizations.SelectedTakeOffVfxItemTypeId);
			bs.WriteGuid(this.Customizations.SelectedKillVfxItemTypeId);
			bs.WriteGuid(this.Customizations.SelectedScoreVfxItemTypeId);
			bs.WriteGuid(this.Customizations.SelectedPortraitItemTypeId);
			bs.WriteGuid(this.Customizations.SelectedSkin);
			bs.WriteByte(this.PlayerAddress);
			bs.WriteTeamKind(this.Team);
			bs.WriteBits(4, this.TeamSlot);
			bs.WriteCompressedInt(this.GridIndex);
			bs.WriteBool(this.Connected);
			bs.WriteByte((byte)this.Level);
			bs.WriteByte((byte)this.FounderLevel);
			bs.WriteBool(this.IsRookie);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.PlayerId = bs.ReadCompressedLong();
			this.UserId = bs.ReadString();
			this.IsBot = bs.ReadBool();
			this.IsBotControlled = bs.ReadBool();
			this.IsNarrator = bs.ReadBool();
			this.IsLeaver = bs.ReadBool();
			this.Name = bs.ReadString();
			int num = bs.ReadCompressedInt();
			this.Customizations.SelectedSprayItemTypeId = bs.ReadGuid();
			this.Customizations.SelectedRespawnVfxItemTypeId = bs.ReadGuid();
			this.Customizations.SelectedTakeOffVfxItemTypeId = bs.ReadGuid();
			this.Customizations.SelectedKillVfxItemTypeId = bs.ReadGuid();
			this.Customizations.SelectedScoreVfxItemTypeId = bs.ReadGuid();
			this.Customizations.SelectedPortraitItemTypeId = bs.ReadGuid();
			this.Customizations.SelectedSkin = bs.ReadGuid();
			this.PlayerAddress = bs.ReadByte();
			this.Team = bs.ReadTeamKind();
			this.TeamSlot = bs.ReadBits(4);
			this.GridIndex = bs.ReadCompressedInt();
			this.Connected = bs.ReadBool();
			this.Level = (int)bs.ReadByte();
			this.FounderLevel = (FounderLevel)bs.ReadByte();
			this.IsRookie = bs.ReadBool();
			if (num != this._characterId)
			{
				this.SetCharacter(num);
			}
		}

		public override string ToString()
		{
			return string.Format("Name={0} Character={1} IsCurrentPlayer={2} IsBot={3} CharId={4} Address={5} PlayerCarId={6} Team={7}/{8} GroupId={9} Connected={10} Leaver={11} HasCounselor={12}", new object[]
			{
				this.Name,
				(!this.Character) ? "null" : this.Character.Character.ToString(),
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

		public string UserId;

		public User UserSF;

		public bool Connected;

		[NonSerialized]
		public bool Ready;

		[NonSerialized]
		public BattlepassProgress BattlepassProgress;

		public Player PlayerSF;

		public string Name;

		private PlayerBag _bag;

		public int Level;

		public CharacterBag CharBag;

		public FounderLevel FounderLevel;

		public CustomizationContent Customizations = new CustomizationContent();

		private Character[] _characters;

		private int[] _ownedCharacters;

		public bool IsRookie;

		public Guid GroupId;

		public bool IsBot;

		public byte PlayerAddress;

		public TeamKind Team;

		public int TeamSlot;

		public int GridIndex = -1;

		private int _characterId = -1;

		private Identifiable _characterInstance;

		public CharacterInfo _character;

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
				ServerPlayerItems playerItems = (ServerPlayerItems)((JsonSerializeable<T>)strServerPlayerItems);
				this._playerData.SetPlayerItems(playerItems);
				this.Finalized();
			}

			private Action<int[]> _whenDone;

			private PlayerData _playerData;
		}
	}
}

using System;
using System.Collections.Generic;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public class UpdateStreamAccess : GameHubBehaviour
	{
		public UpdateDataStream<CombatData> CombatDataStream
		{
			get
			{
				UpdateDataStream<CombatData> result;
				if ((result = this._combatDataStream) == null)
				{
					result = (this._combatDataStream = new UpdateDataStream<CombatData>());
				}
				return result;
			}
		}

		public UpdateDataStream<CombatFeedback> CombatFeedStream
		{
			get
			{
				UpdateDataStream<CombatFeedback> result;
				if ((result = this._combatFeedStream) == null)
				{
					result = (this._combatFeedStream = new UpdateDataStream<CombatFeedback>());
				}
				return result;
			}
		}

		public UpdateDataStream<CombatAttributes> CombatAttStream
		{
			get
			{
				UpdateDataStream<CombatAttributes> result;
				if ((result = this._combatAttStream) == null)
				{
					result = (this._combatAttStream = new UpdateDataStream<CombatAttributes>());
				}
				return result;
			}
		}

		public UpdateDataStream<GadgetData> GadgetDataStream
		{
			get
			{
				UpdateDataStream<GadgetData> result;
				if ((result = this._gadgetDataStream) == null)
				{
					result = (this._gadgetDataStream = new UpdateDataStream<GadgetData>());
				}
				return result;
			}
		}

		public UpdateDataStream<SpawnController> SpawnControllerStream
		{
			get
			{
				UpdateDataStream<SpawnController> result;
				if ((result = this._spawnControllerStream) == null)
				{
					result = (this._spawnControllerStream = new UpdateDataStream<SpawnController>());
				}
				return result;
			}
		}

		public UpdateDataStream<PlayerStats> StatsStream
		{
			get
			{
				UpdateDataStream<PlayerStats> result;
				if ((result = this._statsStream) == null)
				{
					result = (this._statsStream = new UpdateDataStream<PlayerStats>());
				}
				return result;
			}
		}

		public StateUpdateSteram StateStream
		{
			get
			{
				StateUpdateSteram result;
				if ((result = this._stateStream) == null)
				{
					result = (this._stateStream = new StateUpdateSteram());
				}
				return result;
			}
		}

		public void Cleanup()
		{
			this.CombatDataStream.Cleanup();
			this.CombatFeedStream.Cleanup();
			this.CombatAttStream.Cleanup();
			this.GadgetDataStream.Cleanup();
			this.SpawnControllerStream.Cleanup();
			this.StatsStream.Cleanup();
			this.StateStream.Cleanup();
			this.ControllersList.Clear();
			this.ControllersById.Clear();
		}

		public void AddObject(Identifiable obj)
		{
			this.CombatDataStream.AddObject(obj);
			this.CombatFeedStream.AddObject(obj);
			this.CombatAttStream.AddObject(obj);
			this.GadgetDataStream.AddObject(obj);
			this.SpawnControllerStream.AddObject(obj);
			this.StatsStream.AddObject(obj);
			CombatController bitComponent = obj.GetBitComponent<CombatController>();
			if (bitComponent)
			{
				this.ControllersById[obj.ObjId] = bitComponent;
				if (!this.ControllersList.Contains(bitComponent))
				{
					this.ControllersList.Add(bitComponent);
				}
			}
		}

		public void Remove(int objId)
		{
			this.CombatDataStream.Remove(objId);
			this.CombatFeedStream.Remove(objId);
			this.CombatAttStream.Remove(objId);
			this.GadgetDataStream.Remove(objId);
			this.SpawnControllerStream.Remove(objId);
			this.StatsStream.Remove(objId);
			if (this.ControllersById.Remove(objId))
			{
				this.ControllersList.RemoveAll((CombatController x) => x == null || x.Id.ObjId == objId);
			}
		}

		private UpdateDataStream<CombatData> _combatDataStream;

		private UpdateDataStream<CombatFeedback> _combatFeedStream;

		private UpdateDataStream<CombatAttributes> _combatAttStream;

		private UpdateDataStream<GadgetData> _gadgetDataStream;

		private UpdateDataStream<SpawnController> _spawnControllerStream;

		private UpdateDataStream<PlayerStats> _statsStream;

		private StateUpdateSteram _stateStream;

		public readonly List<CombatController> ControllersList = new List<CombatController>();

		public readonly Dictionary<int, CombatController> ControllersById = new Dictionary<int, CombatController>();
	}
}

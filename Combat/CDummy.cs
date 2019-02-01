using System;
using System.Collections.Generic;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CDummy : GameHubBehaviour
	{
		private void Start()
		{
			this._transform = base.transform;
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			if (!this.Front)
			{
				this.Front = this._transform;
			}
			if (!this.Left)
			{
				this.Left = this._transform;
			}
			if (!this.Right)
			{
				this.Right = this._transform;
			}
			if (!this.Rear)
			{
				this.Rear = this._transform;
			}
			if (!this.Top)
			{
				this.Top = this._transform;
			}
			if (!this.Center)
			{
				this.Center = this._transform;
			}
			if (!this.Turret)
			{
				this.Turret = this._transform;
			}
			if (!this.TrapDrop)
			{
				this.TrapDrop = this._transform;
			}
			if (!this.SensorFwd)
			{
				this.SensorFwd = this._transform;
			}
			if (!this.SensorBack)
			{
				this.SensorBack = this._transform;
			}
			if (!this.BombHook)
			{
				this.BombHook = this._transform;
			}
			this.CarGenerator = base.GetComponent<CarGenerator>();
		}

		public void AddCustomDummy(Transform customDummy)
		{
			this.CustomDummies.Add(customDummy);
		}

		public void RemoveCustomDummy(Transform customDummy)
		{
			for (int i = 0; i < this.CustomDummies.Count; i++)
			{
				Transform x = this.CustomDummies[i];
				if (x == customDummy)
				{
					this.CustomDummies.Remove(customDummy);
					break;
				}
			}
		}

		public Transform GetCustomDummy(string dummyName)
		{
			for (int i = 0; i < this.CustomDummies.Count; i++)
			{
				Transform transform = this.CustomDummies[i];
				if (transform == null)
				{
					HeavyMetalMachines.Utils.Debug.Assert(false, "GetCustomDummy: Null Custom Dummy in GameObject " + base.gameObject.name, HeavyMetalMachines.Utils.Debug.TargetTeam.Sd);
				}
				else if (transform.name == dummyName)
				{
					return transform;
				}
			}
			CDummy.Log.WarnFormat("GetCustomDummy: {0} NOT found in GameObject {1}", new object[]
			{
				dummyName,
				base.gameObject.name
			});
			return null;
		}

		public Transform GetDummy(CDummy.DummyKind kind, string customDummyName)
		{
			this.EnsureValidTransform();
			switch (kind)
			{
			case CDummy.DummyKind.None:
				if (this.CarGenerator && this.CarGenerator.bodyGO)
				{
					return this.CarGenerator.bodyGO.transform;
				}
				return this._transform;
			case CDummy.DummyKind.Rear:
				return this.Rear;
			case CDummy.DummyKind.Front:
				return this.Front;
			case CDummy.DummyKind.Left:
				return this.Left;
			case CDummy.DummyKind.Right:
				return this.Right;
			case CDummy.DummyKind.Top:
				return this.Top;
			case CDummy.DummyKind.Center:
				return this.Center;
			case CDummy.DummyKind.Turret:
				return this.Turret;
			case CDummy.DummyKind.TrapDrop:
				return this.TrapDrop;
			case CDummy.DummyKind.SensorFwd:
				return this.SensorFwd;
			case CDummy.DummyKind.SensorBack:
				return this.SensorBack;
			case CDummy.DummyKind.BombHook:
				return this.BombHook;
			case CDummy.DummyKind.BombHookVFX:
				return this.BombHookVFX;
			default:
				if (kind == CDummy.DummyKind.Custom)
				{
					return this.GetCustomDummy(customDummyName);
				}
				if (this.CarGenerator && this.CarGenerator.bodyGO)
				{
					return this.CarGenerator.bodyGO.transform;
				}
				return this._transform;
			}
		}

		private void EnsureValidTransform()
		{
			if (!this._transform)
			{
				this.Start();
			}
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(CDummy));

		public Transform Front;

		public Transform Left;

		public Transform Right;

		public Transform Rear;

		public Transform Top;

		public Transform Center;

		public Transform Turret;

		public Transform TrapDrop;

		public Transform SensorFwd;

		public Transform SensorBack;

		public Transform BombHook;

		public Transform BombHookVFX;

		public List<Transform> CustomDummies = new List<Transform>();

		public CarGenerator CarGenerator;

		private Transform _transform;

		public enum DummyKind
		{
			None,
			Rear,
			Front,
			Left,
			Right,
			Top,
			Center,
			Turret,
			TrapDrop,
			SensorFwd,
			SensorBack,
			BombHook,
			BombHookVFX,
			Custom = 100
		}

		[Serializable]
		public struct ShotPosAndDir
		{
			public CDummy.DummyKind Dummy;

			[Tooltip("Only for custom dummy")]
			public string DummyName;

			[Tooltip("Make sure y is 0! [2D game ;)]")]
			public Vector3 OffsetPos;

			[Tooltip("Will use dummy forward, if true, will use Mouse/Thumbstick")]
			public bool UseTarget;

			public bool UseTargetAsOrigin;
		}
	}
}

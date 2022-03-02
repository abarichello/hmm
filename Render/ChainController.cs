using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class ChainController : GameHubBehaviour
	{
		public void Attach(Transform target, HookVFX.HookStage stage, bool hasUpgrade = false)
		{
			this._targetTransform = target;
			base.enabled = true;
			this._lineRender.enabled = true;
			this._stage = stage;
			if (hasUpgrade)
			{
				this._lineRender.OverlayMaterial = this.UpgradeMaterial;
			}
		}

		public void Dettach(HookVFX.HookStage stage, bool disableChain = true, bool hasUpgrade = false)
		{
			if (stage == this._stage)
			{
				this._targetTransform = null;
			}
			if (disableChain)
			{
				base.enabled = false;
				this._lineRender.enabled = false;
				this._shouldResetPhysics = true;
			}
			if (hasUpgrade)
			{
				this._lineRender.OverlayMaterial = null;
			}
		}

		private void Awake()
		{
			if (!GameHubBehaviour.Hub || (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest()))
			{
				Object.Destroy(this);
				return;
			}
			this._sourceTransform = base.transform;
			if (this.Dummy != CDummy.DummyKind.None)
			{
				CDummy component = base.transform.GetComponent<CDummy>();
				if (component)
				{
					Transform dummy = component.GetDummy(this.Dummy, this.CustomDummyName, null);
					if (dummy != null)
					{
						this._sourceTransform = dummy;
					}
				}
			}
			this._lineRender = base.gameObject.AddComponent<OverlayedLineRenderer>();
			this._lineRender.Material = this.ChainMaterial;
			this._lineRender.OverlayMaterial = null;
			this._lineRender.Speed = this.UpgradeAnimSpeed;
			this._lineRender.LineWidth = this.LineWidth;
			this._lineRender.TileSize = this.TileSize;
			this._targetTransform = null;
			this._lineRender.LinePoints = new Vector3[this.ChainPoints];
			SpringConstraint neighbor = null;
			this.Chain = new SpringConstraint[this.ChainPoints];
			for (int i = this.ChainPoints - 1; i >= 0; i--)
			{
				bool isAnchor = i == this.ChainPoints - 1 || i == 0;
				this.Chain[i] = new SpringConstraint(Vector3.zero, neighbor, isAnchor);
				this.Chain[i].gravity = new Vector3(0f, -9.8f, 0f);
				this.Chain[i].type = SpringConstraint.SystemType.Hard;
				neighbor = this.Chain[i];
			}
			this._lineRender.enabled = false;
			this._shouldResetPhysics = true;
		}

		private void OnEnable()
		{
			for (int i = 0; i < this._lineRender.LinePoints.Length; i++)
			{
				this._lineRender.LinePoints[i].Set(0f, 0f, 0f);
			}
			if (this._disableAllAfterDeath)
			{
				this.Dettach(HookVFX.HookStage.Go, true, false);
				this._disableAllAfterDeath = false;
			}
		}

		private void Start()
		{
			base.enabled = false;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
			if (componentInParent)
			{
				componentInParent.ListenToObjectUnspawn += this.ObjectUnspawn;
			}
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
			if (componentInParent)
			{
				componentInParent.ListenToObjectUnspawn -= this.ObjectUnspawn;
			}
			this.Chain = null;
		}

		private void LateUpdate()
		{
			this._source = this._sourceTransform.position;
			if (this._targetTransform != null)
			{
				this._target = this._targetTransform.position;
				this._target.y = Mathf.Max(this._source.y, 0.5f);
			}
			this.Chain[0].Position = this._source;
			this.Chain[this.ChainPoints - 1].Position = this._target;
			this.Chain[0].SimulateSystem(Time.deltaTime * this.AnimationSpeed);
			if (!this._shouldResetPhysics && this._grabLinkIndex < this.ChainPoints - 1)
			{
				if (this._stage == HookVFX.HookStage.Back || this._stage == HookVFX.HookStage.Fail)
				{
					for (int i = 0; i < this.GrabLinkCount; i++)
					{
						if (Vector3.Distance(this.Chain[this._grabLinkIndex].Position, this._source) >= this.GrabLinkDistance)
						{
							break;
						}
						this.Chain[this._grabLinkIndex].Position = this._source;
						this._grabLinkIndex++;
						if (this._grabLinkIndex >= this.ChainPoints - 1)
						{
							break;
						}
					}
				}
			}
			else
			{
				this._grabLinkIndex = 1;
			}
			for (int j = 0; j < this.ChainPoints; j++)
			{
				if (this._shouldResetPhysics)
				{
					this.Chain[j].Position = this._sourceTransform.position;
					this.Chain[j].Velocity = Vector3.zero;
				}
				this.Chain[j].restLength = this.restLength;
				if (this.Chain[j].Position.y < 0.5f)
				{
					Vector3 position = this.Chain[j].Position;
					position.y = 0.5f;
					this.Chain[j].Position = position;
					Vector3 velocity = this.Chain[j].Velocity;
					velocity.y = 0f;
					this.Chain[j].Velocity = velocity;
				}
				this._lineRender.LinePoints[this.ChainPoints - j - 1] = this.Chain[j].Position;
			}
			this._shouldResetPhysics = false;
		}

		public void ObjectUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			this._disableAllAfterDeath = true;
			if (obj.Player.PlayerCarId == msg.ObjId)
			{
				this.OnEnable();
			}
		}

		public void OnPhaseChange(BombScoreboardState state)
		{
			if (state == BombScoreboardState.Replay || state == BombScoreboardState.Shop)
			{
				this._targetTransform = null;
				base.enabled = false;
				this._lineRender.enabled = false;
				this._lineRender.OverlayMaterial = null;
			}
		}

		public CDummy.DummyKind Dummy;

		public string CustomDummyName = string.Empty;

		public float LineWidth;

		public float TileSize;

		public float AnimationSpeed = 1f;

		public float restLength;

		public Material ChainMaterial;

		public Material UpgradeMaterial;

		public float UpgradeAnimSpeed = 1f;

		public float GrabLinkDistance = 5f;

		public int GrabLinkCount = 3;

		public int ChainPoints = 10;

		[HideInInspector]
		public SpringConstraint[] Chain;

		private const float YPLANE = 0.5f;

		private OverlayedLineRenderer _lineRender;

		private Transform _sourceTransform;

		private Transform _targetTransform;

		private Vector3 _source;

		private Vector3 _target;

		private HookVFX.HookStage _stage;

		private bool _shouldResetPhysics;

		private bool _tensionEnabled;

		private Vector3 _gravity;

		private float _restMult;

		private int _grabLinkIndex;

		private bool _disableAllAfterDeath;
	}
}

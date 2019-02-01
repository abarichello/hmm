using System;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using NewParticleSystem;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class HelicopterController : GameHubBehaviour
	{
		[HideInInspector]
		public HelicopterController.State CurrentState { get; private set; }

		public void Connect(Transform target, bool followTarget)
		{
			base.enabled = true;
			this._target = target;
			if (this._heliInstance == null)
			{
				this._lifeTimer = 0f;
				this._heliInstance = (HelicopterComponents)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.HeliPrefab, this._target.position, this._target.rotation);
				this._heliInstance.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
				this._localAnimTime = 0f;
				this._heliInstance.HeliAnimator.SetLayerWeight(2, 0f);
				this._heliInstance.HeliAnimator.SetBool("isAlly", true);
				this._heliInstance.HeliAnimator.SetTrigger("move");
				if (base.Id)
				{
					PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(base.Id.ObjId);
					this._heliInstance.HeliAnimator.SetBool("isAlly", playerOrBotsByObjectId.Team == GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
				}
				this.CurrentState = HelicopterController.State.Arriving;
				this.HeliActiveParticles.Play();
				this.HeliCallParticles.Play();
				this._heliInstance.HeliCallParticles.Play();
				FMODAudioManager.PlayAt(this.HeliIntroVO, this._heliInstance.HelicopterTransform);
			}
			else if (followTarget)
			{
				this._movingAnimTime = 0f;
				this.CurrentState = HelicopterController.State.Moving;
				this._originalPos = this._heliInstance.transform.position;
				this._originalRot = this._heliInstance.HelicopterTransform.rotation;
				this._targetRot = Quaternion.LookRotation((this._target.position - this._heliInstance.transform.position).normalized);
				this._heliInstance.HeliCallLaser.enabled = true;
				this._heliInstance.HeliAnimator.SetTrigger("move");
				this.HeliCallParticles.Play();
				this._heliInstance.HeliCallParticles.Play();
			}
		}

		public void Disconnect(Transform target)
		{
			if (this._target == target)
			{
				this._target = null;
			}
		}

		private void Awake()
		{
			if (!GameHubBehaviour.Hub)
			{
				base.enabled = false;
				Debug.LogWarning("No cache available! Disabling Helicopter");
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			GameHubBehaviour.Hub.Resources.PrefabPreCache(this.HeliPrefab, 1);
			this._lookAtRotationCache = new Quaternion[this.HeliPrefab.LookAtDummies.Length];
		}

		private void Start()
		{
			base.enabled = false;
			CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
			if (componentInParent)
			{
				componentInParent.ListenToObjectUnspawn += this.ObjectUnspawn;
			}
		}

		private void OnDestroy()
		{
			this._lookAtRotationCache = null;
			CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
			if (componentInParent)
			{
				componentInParent.ListenToObjectUnspawn -= this.ObjectUnspawn;
			}
		}

		public void LateUpdate()
		{
			if (this._target)
			{
				this._targetPos = this._target.position;
			}
			if (this.CurrentState < HelicopterController.State.Disconnected)
			{
				for (int i = 0; i < this._heliInstance.LookAtDummies.Length; i++)
				{
					Transform transform = this._heliInstance.LookAtDummies[i];
					transform.rotation = Quaternion.LookRotation((this._targetPos - transform.position).normalized);
					this._lookAtRotationCache[i] = transform.rotation;
				}
			}
			if (this._localAnimTime < this.IntroTime && this.CurrentState < HelicopterController.State.Disconnected)
			{
				Vector3 position = Vector3.Lerp(this._heliInstance.DummyIntro.position, this._heliInstance.DummyIdle.position, LerpFunc.QuadOut(this._localAnimTime, 0f, 1f, this.IntroTime));
				this._heliInstance.HelicopterTransform.position = position;
				this._localAnimTime += Time.deltaTime;
			}
			switch (this.CurrentState)
			{
			case HelicopterController.State.Arriving:
				if (this._localAnimTime > this.IntroTime * 0.45f)
				{
					this._heliInstance.HeliAnimator.SetTrigger("stop");
					if (this._localAnimTime > this.IntroTime)
					{
						this.HeliCallParticles.Stop();
						this._heliInstance.HeliCallParticles.Stop();
						this.CurrentState = HelicopterController.State.Idle;
					}
				}
				goto IL_5DE;
			case HelicopterController.State.Idle:
				if (this._lifeTimer > this.LifeTime)
				{
					this.CurrentState = HelicopterController.State.Disconnected;
				}
				goto IL_5DE;
			case HelicopterController.State.Moving:
			{
				float t = LerpFunc.QuadInOut(this._movingAnimTime, 0f, 1f, 1f);
				this._heliInstance.transform.position = Vector3.Lerp(this._originalPos, this._targetPos, t);
				this._heliInstance.HelicopterTransform.rotation = Quaternion.Slerp(this._originalRot, this._targetRot, t);
				Quaternion rotation = Quaternion.LookRotation((this._heliInstance.HelicopterTransform.position - base.transform.position).normalized);
				this.HeliCallParticles.transform.parent.rotation = rotation;
				this._heliInstance.HeliCallParticles.transform.parent.rotation = Quaternion.Inverse(rotation);
				this._heliInstance.HeliCallLaser.SetPosition(0, this._heliInstance.HeliCallParticles.transform.position);
				this._heliInstance.HeliCallLaser.SetPosition(1, this.HeliCallParticles.transform.position);
				this._movingAnimTime += 0.01f * this.MovementSpeed;
				if (this._movingAnimTime > 1f)
				{
					this.CurrentState = HelicopterController.State.Idle;
					this._heliInstance.HeliAnimator.SetTrigger("stop");
					this._heliInstance.HeliCallLaser.enabled = false;
					this.HeliCallParticles.Stop();
					this._heliInstance.HeliCallParticles.Stop();
					if (this._lifeTimer > this.LifeTime)
					{
						this.CurrentState = HelicopterController.State.Disconnected;
					}
				}
				goto IL_5DE;
			}
			case HelicopterController.State.Disconnected:
				for (int j = 0; j < this._heliInstance.StopOnLeavingParticles.Length; j++)
				{
					HoplonParticleSystem hoplonParticleSystem = this._heliInstance.StopOnLeavingParticles[j];
					hoplonParticleSystem.Stop();
				}
				this._heliInstance.HeliSoundLoop.KeyOff();
				FMODAudioManager.PlayAt(this.HeliOutroVO, this._heliInstance.HelicopterTransform);
				this.HeliActiveParticles.Stop();
				this._localAnimTime = 0f;
				this.CurrentState = HelicopterController.State.PreLeaving;
				break;
			case HelicopterController.State.PreLeaving:
				break;
			case HelicopterController.State.Leaving:
			{
				if (this._localAnimTime > this.OutroTime)
				{
					this.CurrentState = HelicopterController.State.Finished;
				}
				float t2 = LerpFunc.QuadIn(this._localAnimTime, 0f, 1f, this.OutroTime);
				this._heliInstance.HelicopterTransform.rotation = Quaternion.Slerp(this._originalRot, this._targetRot, t2);
				Vector3 position2 = Vector3.Lerp(this._heliInstance.DummyIdle.position, this._heliInstance.DummyOutro.position, t2);
				this._heliInstance.HelicopterTransform.position = position2;
				this._localAnimTime += Time.deltaTime;
				goto IL_5DE;
			}
			case HelicopterController.State.Finished:
				GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.HeliPrefab, this._heliInstance);
				this._heliInstance = null;
				this._target = null;
				this.CurrentState = HelicopterController.State.None;
				base.enabled = false;
				goto IL_5DE;
			default:
				goto IL_5DE;
			}
			this._heliInstance.HeliAnimator.SetBool("ultimateDeactivate", true);
			for (int k = 0; k < this._heliInstance.LookAtDummies.Length; k++)
			{
				Transform transform2 = this._heliInstance.LookAtDummies[k];
				transform2.rotation = Quaternion.Slerp(this._lookAtRotationCache[k], transform2.rotation, this._localAnimTime / 1f);
			}
			this._localAnimTime += Time.deltaTime;
			if (this._localAnimTime > 1f)
			{
				Vector3 normalized = (this._heliInstance.DummyOutro.position - this._heliInstance.transform.position).normalized;
				normalized.y = 0f;
				this._originalRot = this._heliInstance.HelicopterTransform.rotation;
				this._targetRot = Quaternion.LookRotation(normalized);
				this._localAnimTime = 0f;
				this.CurrentState = HelicopterController.State.Leaving;
			}
			IL_5DE:
			this._lifeTimer += Time.deltaTime;
		}

		public void ObjectUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			if (this._heliInstance && base.enabled)
			{
				this._lifeTimer = this.LifeTime;
				this.HeliCallParticles.Stop();
				this.HeliActiveParticles.Stop();
				this._heliInstance.HeliCallParticles.Stop();
				this._heliInstance.HeliCallLaser.enabled = false;
				this._heliInstance.ActivateForcedUpdate(this);
			}
		}

		public HelicopterComponents HeliPrefab;

		public HoplonParticleSystem HeliCallParticles;

		public HoplonParticleSystem HeliActiveParticles;

		public FMODAsset HeliIntroVO;

		public FMODAsset HeliOutroVO;

		public float IntroTime = 2f;

		public float OutroTime = 2f;

		public float MovementSpeed = 1f;

		public float CustomAnimationSpeed = 1f;

		[HideInInspector]
		[NonSerialized]
		public float LifeTime = 20f;

		private HelicopterComponents _heliInstance;

		private Transform _target;

		private float _lifeTimer;

		private float _localAnimTime;

		private float _movingAnimTime;

		private Quaternion _originalRot;

		private Quaternion _targetRot;

		private Vector3 _originalPos;

		private Vector3 _targetPos;

		private Quaternion[] _lookAtRotationCache;

		public enum State
		{
			Arriving,
			Idle,
			Moving,
			Disconnected,
			PreLeaving,
			Leaving,
			Finished,
			None
		}
	}
}

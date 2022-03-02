using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Combat
{
	public class CombatMovement : GameHubBehaviour, IObjectSpawnListener, IPhysicalObject, ICombatMovement
	{
		public Vector3 Velocity
		{
			get
			{
				return this.LastVelocity;
			}
		}

		public Vector3 Position
		{
			get
			{
				return (!GameHubBehaviour.Hub.Net.IsServer()) ? this._trans.position : this._body.position;
			}
		}

		public Vector3 LastPosition
		{
			get
			{
				return this._lastPosition;
			}
		}

		public Vector3 AngularVelocity
		{
			get
			{
				return Vector3.up * this.LastAngularVelocity;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return (!GameHubBehaviour.Hub.Net.IsServer()) ? this._trans.rotation : this._body.rotation;
			}
		}

		public virtual MovementInfo Info
		{
			get
			{
				return this._info;
			}
		}

		public bool CanMove
		{
			get
			{
				return !this._lockMovement && !this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Freeze) && !GameHubBehaviour.Hub.Global.LockAllPlayers;
			}
		}

		public bool IsCollidingWithScenery { get; private set; }

		public Vector3 SceneryNormal { get; private set; }

		public Vector3 LastVelocity
		{
			get
			{
				if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Freeze))
				{
					return Vector3.zero;
				}
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					return this._body.velocity;
				}
				return this._lastVelocity;
			}
			set
			{
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					CombatMovement.Log.WarnFormat("{0} Trying to set velocity directly instead of through physics on server!", new object[]
					{
						this.Combat
					});
					return;
				}
				this._lastVelocity = value;
			}
		}

		public float LastSpeed
		{
			get
			{
				return (!this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Freeze)) ? this._lastSpeed : 0f;
			}
		}

		public float LastDragForce
		{
			get
			{
				Vector3 velocity = this._body.velocity;
				this.ApplyDrag(this.Info.Drag, this.Combat.Attributes.DragMod, this.Combat.Attributes.DragModPct);
				float result = (velocity - this._body.velocity).magnitude / Time.deltaTime;
				this._body.velocity = velocity;
				return result;
			}
		}

		public float LastAngularVelocity
		{
			get
			{
				return (!this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Freeze)) ? this._lastAngularVelocity.y : 0f;
			}
			set
			{
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					CombatMovement.Log.WarnFormat("{0} Trying to set angular velocity directly instead of through physics on server!", new object[]
					{
						this.Combat
					});
					return;
				}
				this._lastAngularVelocity = Vector3.up * value;
			}
		}

		public float MaxAngularSpeed
		{
			get
			{
				return (this.Info.MaxAngularSpeed + this.Combat.Attributes.MaxAngularSpeed) * (1f + this.Combat.Attributes.MaxAngularSpeedPct);
			}
		}

		public float Mass
		{
			get
			{
				return (this.Info.Mass + this.Combat.Attributes.Mass) * (1f + this.Combat.Attributes.MassPct);
			}
		}

		public List<ICombatLink> Links
		{
			get
			{
				return this._links;
			}
		}

		public Vector3 Up
		{
			get
			{
				return this._trans.up;
			}
		}

		public void BreakAllLinks()
		{
			for (int i = 0; i < this._links.Count; i++)
			{
				ICombatLink combatLink = this._links[i];
				combatLink.Break();
			}
		}

		public void IncrementUpdatedLinkCount(ICombatLink link)
		{
			if (++this._numLinksUpdated >= this._links.Count && this.CanMove)
			{
				this.ApplyDrag(this.Info.Drag, this.Combat.Attributes.DragMod, this.Combat.Attributes.DragModPct);
				this.SetNewVelocity();
				this.PhysicConstraint();
				this._numLinksUpdated = 0;
			}
		}

		public void PauseSimulation()
		{
			this._lockMovement = true;
		}

		public void UnpauseSimulation()
		{
			this._lockMovement = false;
		}

		protected virtual void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._meshValidatorDepth = new float[]
			{
				this.Info.DepthOfMeshValidators[0] + (float)this.Combat.Team
			};
			this._respawnMeshValidatorDepth = new float[]
			{
				(float)this.Combat.Team
			};
			this._body.centerOfMass = new Vector3(0f, 0f, this.Info.MaxCenterZ);
		}

		protected virtual void Awake()
		{
			this._trans = base.transform;
			if (this._body == null)
			{
				this._body = base.GetComponent<Rigidbody>();
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>(true);
				if (this._body)
				{
					Object.Destroy(this._body);
				}
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
				return;
			}
			if (!this.Combat)
			{
				this.Combat = base.GetComponent<CombatObject>();
			}
			this._body.constraints = 84;
			this._lastPosition = this._body.position;
		}

		private void FixedUpdate()
		{
			this._body.mass = this.Mass;
			this._lastPosition = this._body.position;
			if (!this.CanMove)
			{
				this.ResetCarMovementProperties();
				return;
			}
			if (this._lastVelocity.sqrMagnitude > 0f)
			{
				this._body.velocity = this._lastVelocity;
				this._lastVelocity = Vector3.zero;
			}
			this._lastSpeed = this._body.velocity.magnitude;
			this.MovementFixedUpdate();
			this.UpdateLinks();
			if (this.ThereAreNoActiveLinks())
			{
				this.ApplyDrag(this.Info.Drag, this.Combat.Attributes.DragMod, this.Combat.Attributes.DragModPct);
				this.SetNewVelocity();
			}
			this.PhysicConstraint();
		}

		private bool ThereAreNoActiveLinks()
		{
			return this._links.All((ICombatLink link) => !link.IsEnabled);
		}

		private void ResetCarMovementProperties()
		{
			if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Freeze) && this._body.velocity.sqrMagnitude > 0f)
			{
				this._lastVelocity = this._body.velocity;
			}
			this._lastAngularVelocity = Vector3.zero;
			this._body.angularVelocity = Vector3.zero;
			this._body.velocity = Vector3.zero;
			this._lastSpeed = 0f;
			if (!GameHubBehaviour.Hub.Global.LockAllPlayers)
			{
				this.UpdateLinks();
			}
		}

		protected virtual void ApplyDrag(AnimationCurve dragCurve, float dragMod, float dragModPct)
		{
			float drag = CombatMovement.GetDrag(this._lastSpeed, dragCurve, dragMod, dragModPct);
			if (this._lastSpeed < drag * Time.deltaTime)
			{
				this._body.velocity = Vector3.zero;
			}
			else
			{
				this._body.AddForce(this._body.velocity.normalized * drag * -1f, 5);
			}
		}

		protected virtual void SetNewVelocity()
		{
			this.SceneryNormal = Vector3.zero;
			this.IsCollidingWithScenery = false;
		}

		protected virtual void PhysicConstraint()
		{
			this._body.AddRelativeForce(this._body.velocity.y * Vector3.down, 2);
			Quaternion rotation = Quaternion.Euler(0f, this._body.rotation.eulerAngles.y, 0f);
			this._body.rotation = rotation;
		}

		protected virtual void UpdateLinks()
		{
			for (int i = this._links.Count - 1; i >= 0; i--)
			{
				if (this._links[i].IsBroken)
				{
					this._links.RemoveAt(i);
				}
				else
				{
					this._links[i].Update(this);
				}
			}
		}

		protected virtual void MovementFixedUpdate()
		{
			this._body.maxAngularVelocity = this.MaxAngularSpeed * 0.017453292f;
			this.SpeedZ = Vector3.Dot(this._trans.forward, this._body.velocity);
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			this.OnCollisionStay(collision);
			if (GameHubBehaviour.Hub == null || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			float time = Time.time;
			if (this._nextCollisionEvent > time)
			{
				return;
			}
			this._nextCollisionEvent = time + 0.5f;
			CollisionEvent evt = new CollisionEvent
			{
				ObjId = base.Id.ObjId,
				Point = collision.contacts[0].point,
				Normal = collision.contacts[0].normal,
				Intensity = collision.relativeVelocity.magnitude,
				OtherLayer = (byte)collision.contacts[0].otherCollider.gameObject.layer
			};
			this._dispatcher.SendData(evt);
		}

		protected virtual void OnCollisionStay(Collision collision)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.PhysicConstraint();
			if (this.OnCollisionWithBombBlocker != null && base.enabled && collision.collider.gameObject.layer == 19)
			{
				this.OnCollisionWithBombBlocker();
			}
		}

		public virtual void Push(Vector3 direction, bool pct, float magnitude = 0f, bool ignorePushReceived = false)
		{
			if (!base.enabled || !this.CanMove || PauseController.Instance.IsGamePaused)
			{
				return;
			}
			if (pct)
			{
				if (magnitude != 0f)
				{
					direction *= this._body.velocity.magnitude * magnitude;
				}
				else
				{
					direction *= this._body.velocity.magnitude;
				}
			}
			Vector3 vector = direction;
			if (magnitude != 0f)
			{
				vector *= magnitude;
			}
			vector *= ((!ignorePushReceived) ? (this.Info.PushReceived + this.Combat.Attributes.PushReceivedPct) : 1f);
			this._body.AddForce(vector, 2);
		}

		public bool HasLinkWithTag(string tag)
		{
			return this._taggedLinks.ContainsKey(tag);
		}

		public bool HasLinkWith(CombatMovement other)
		{
			for (int i = 0; i < this._links.Count; i++)
			{
				if (this._links[i].Point1.Movement == other || this._links[i].Point2.Movement == other)
				{
					return true;
				}
			}
			return false;
		}

		public ICombatLink GetLinkWithTag(string tag)
		{
			ICombatLink result = null;
			this._taggedLinks.TryGetValue(tag, out result);
			return result;
		}

		public virtual void AddLink(ICombatLink newLink, bool force)
		{
			if (!force && !string.IsNullOrEmpty(newLink.Tag) && this.HasLinkWithTag(newLink.Tag))
			{
				newLink.Break();
				return;
			}
			if (!string.IsNullOrEmpty(newLink.Tag))
			{
				this._taggedLinks[newLink.Tag] = newLink;
			}
			newLink.OnLinkUpdated += this.IncrementUpdatedLinkCount;
			this._links.Add(newLink);
		}

		public virtual void RemoveLink(CombatLink link)
		{
			if (!string.IsNullOrEmpty(link.Tag) && this._taggedLinks.ContainsKey(link.Tag))
			{
				this._taggedLinks.Remove(link.Tag);
			}
			this._links.Remove(link);
		}

		public virtual void ForcePosition(Vector3 newPosition, bool includeLinks = true)
		{
			CombatMovement.Log.DebugFormat("ForcePosition Combat: {0} LastPosition: {1} NewPosition: {2} Body.Position: {3} IncludeLinks: {4}", new object[]
			{
				this.Combat,
				this._lastPosition,
				newPosition,
				this._body.position,
				includeLinks
			});
			if (includeLinks && this._links.Count > 0)
			{
				Vector3 vector = newPosition - this._body.position;
				for (int i = 0; i < this._links.Count; i++)
				{
					ICombatLink combatLink = this._links[i];
					if (!combatLink.IsBroken)
					{
						IPhysicalObject movement = combatLink.Point1.Movement;
						IPhysicalObject movement2 = combatLink.Point2.Movement;
						if (movement != this)
						{
							movement.ForcePosition(movement.LastPosition + vector, false);
						}
						if (movement2 != this)
						{
							movement2.ForcePosition(movement2.LastPosition + vector, false);
						}
					}
				}
			}
			base.transform.position = newPosition;
		}

		public virtual void ForcePositionAndRotation(Vector3 newPosition, Quaternion newRotation)
		{
			CombatMovement.Log.DebugFormat("ForcePositionAndRotation Combat: {0} Position: {1} NewPosition: {2} Rotation: {3} NewRotation: {4}", new object[]
			{
				this.Combat,
				base.transform.position,
				newPosition,
				base.transform.rotation,
				newRotation
			});
			this.BreakAllLinks();
			base.transform.SetPositionAndRotation(newPosition, newRotation);
		}

		public virtual void OnObjectSpawned(SpawnEvent msg)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.enabled = true;
			this.ResetImpulseAndVelocity();
		}

		public virtual void OnObjectUnspawned(UnspawnEvent msg)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.enabled = false;
			this.ResetImpulseAndVelocity();
		}

		public virtual void ResetImpulseAndVelocity()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._body.velocity = Vector3.zero;
				this._body.angularVelocity = Vector3.zero;
			}
			this._lastVelocity = Vector3.zero;
			this._lastAngularVelocity = Vector3.zero;
			this.SpeedZ = 0f;
		}

		public void Clear()
		{
			this.ResetImpulseAndVelocity();
			this.BreakAllLinks();
			FixedJoint[] components = base.GetComponents<FixedJoint>();
			for (int i = 0; i < components.Length; i++)
			{
				Object.Destroy(components[i]);
			}
			this.Push(Vector3.zero, false, 0f, false);
		}

		protected static float GetDrag(float speed, AnimationCurve dragCurve, float dragMod, float dragModPct)
		{
			speed = Mathf.Max((speed + dragMod) * (1f + dragModPct), 0f);
			Keyframe keyframe = dragCurve[dragCurve.length - 1];
			if (speed > keyframe.time)
			{
				float num = keyframe.value / (keyframe.time * keyframe.time);
				return num * speed * speed;
			}
			return dragCurve.Evaluate(speed);
		}

		public Vector3 GetClosestValidPosition(Vector3 position, bool useRespawnMesh = true)
		{
			float[] depthLayers = (!useRespawnMesh) ? this._meshValidatorDepth : this._respawnMeshValidatorDepth;
			return GameHubBehaviour.Hub.MatchMan.GetClosestValidPoint(position, depthLayers, this.Info.ValidatorTeleportOffset);
		}

		public void LookTowards(Vector3 forward)
		{
			base.transform.rotation = Quaternion.LookRotation(forward);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CombatMovement));

		[Inject]
		private ICollisionDispatcher _dispatcher;

		public CombatObject Combat;

		[SerializeField]
		private MovementInfo _info;

		protected readonly Dictionary<string, ICombatLink> _taggedLinks = new Dictionary<string, ICombatLink>();

		protected readonly List<ICombatLink> _links = new List<ICombatLink>();

		protected Vector3 _lastAngularVelocity;

		protected Vector3 _lastVelocity;

		protected float _lastSpeed;

		protected Vector3 _lastPosition;

		protected Rigidbody _body;

		protected Transform _trans;

		protected bool _lockMovement;

		public float SpeedZ;

		private int _numLinksUpdated;

		protected float[] _meshValidatorDepth;

		protected float[] _respawnMeshValidatorDepth;

		private float _nextCollisionEvent = -1f;

		private const float CollisionEventFrequency = 0.5f;

		public Action OnCollisionWithBombBlocker;
	}
}

using System;
using System.Collections.Generic;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using Hoplon.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Fog
{
	public class TRCInterpolator : GameHubBehaviour
	{
		protected Pocketverse.BitStream GetStream()
		{
			if (this._bitstream == null)
			{
				this._bitstream = new Pocketverse.BitStream(256);
			}
			this._bitstream.ResetBitsWritten();
			return this._bitstream;
		}

		public static void ResetUpdateTimers()
		{
			TRCInterpolator.currentTime = (TRCInterpolator.currentTimeModifier = (TRCInterpolator._latestUpdateDelay = (TRCInterpolator._latestUpdateTime = 0f)));
		}

		public static void RunUpdate()
		{
			if (TRCInterpolator._interpolators.Count == 0)
			{
				return;
			}
			float playbackUnityTime = GameHubBehaviour.Hub.GameTime.GetPlaybackUnityTime();
			float num = Mathf.Min(TRCInterpolator._latestUpdateDelay, playbackUnityTime);
			float a = playbackUnityTime - 0.08f;
			float latestUpdateDelay = TRCInterpolator._latestUpdateDelay;
			float num2 = Mathf.Min(a, latestUpdateDelay);
			float num3 = num2 - 0.399999976f;
			float num4 = num2 - TRCInterpolator.currentTime;
			if (TRCInterpolator.currentTime > num || TRCInterpolator.currentTime < num3)
			{
				TRCInterpolator.currentTime = num2;
				TRCInterpolator.currentTimeModifier = 0f;
			}
			else if (Mathf.Abs(num4) > 0.04f)
			{
				TRCInterpolator.currentTimeModifier = Mathf.Lerp(TRCInterpolator.currentTimeModifier, Mathf.Clamp(num4 * 2f, -0.5f, 2f), Time.smoothDeltaTime * 5f);
			}
			else
			{
				TRCInterpolator.currentTimeModifier = Mathf.Lerp(TRCInterpolator.currentTimeModifier, 0f, Time.smoothDeltaTime);
			}
			for (int i = 0; i < TRCInterpolator._interpolators.Count; i++)
			{
				TRCInterpolator._interpolators[i]._aboveExtrapolating = false;
				TRCInterpolator._interpolators[i].DoUpdate();
				GameHubBehaviour.Hub.PlayerExperienceBI.SetFreezeState(TRCInterpolator._interpolators[i]._aboveExtrapolating);
			}
			TRCInterpolator.currentTime += Time.smoothDeltaTime * (1f + TRCInterpolator.currentTimeModifier);
			TRCInterpolator._latestUpdateDelay = Mathf.Min(TRCInterpolator._latestUpdateDelay + Time.smoothDeltaTime, TRCInterpolator._latestUpdateTime);
		}

		private void Awake()
		{
			this._shaderPositionProperty = Shader.PropertyToID("_LocalPlayerPosition");
			if (!this.CombatObject)
			{
				this.CombatObject = base.GetComponent<CombatObject>();
			}
			if (!this.Spawn)
			{
				this.Spawn = base.GetComponent<SpawnController>();
			}
			if (!this.Transf)
			{
				this.Transf = base.transform;
			}
			if (!this.Body)
			{
				this.Body = base.GetComponent<Rigidbody>();
			}
			if (!this.CombatMovement)
			{
				this.CombatMovement = base.GetComponent<CombatMovement>();
			}
			if (!this.Car)
			{
				this.Car = base.GetComponent<CarMovement>();
			}
			if (!this.Input)
			{
				this.Input = base.GetComponent<CarInput>();
			}
			if (!this.Creep)
			{
				this.Creep = base.GetComponent<CreepController>();
			}
			this._stateQueue.Clear();
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
			}
			if (this.Spawn != null)
			{
				this.Spawn.OnSpawn += this.OnObjectSpawned;
			}
			TRCInterpolator._interpolators.Add(this);
		}

		private void OnDestroy()
		{
			TRCInterpolator._interpolators.Remove(this);
			if (this.Spawn != null)
			{
				this.Spawn.OnSpawn -= this.OnObjectSpawned;
			}
		}

		private void DoUpdate()
		{
			if (!base.isActiveAndEnabled || (this.CombatObject && !this.CombatObject.IsAlive() && this.CombatObject.HideOnUnspawn))
			{
				return;
			}
			this.Reposition();
			if (base.Id.IsOwner)
			{
				Shader.SetGlobalVector(this._shaderPositionProperty, base.transform.position + base.transform.forward * 30f);
			}
		}

		private void Reposition()
		{
			if (this._stateQueue.IsEmpty)
			{
				return;
			}
			if (this._stateQueue.Size == 1u)
			{
				InterpolatorState interpolatorState = this._stateQueue[0u];
				this.Position = interpolatorState.Pos;
				this.Rotation = interpolatorState.Rot;
				this.Scale = interpolatorState.Scale;
			}
			else
			{
				uint num = this._stateQueue.Size - 1u;
				InterpolatorState interpolatorState2 = this._stateQueue[num];
				if (TRCInterpolator.currentTime >= interpolatorState2.Time)
				{
					if (!this._isExtrapolating && !interpolatorState2.ShouldInterpolate)
					{
						this.Position = interpolatorState2.Pos;
						this.Rotation = interpolatorState2.Rot;
						this.Velocity = interpolatorState2.Speed;
						this.Scale = interpolatorState2.Scale;
					}
					this._isExtrapolating = true;
					this._isSynching = false;
					if (this._extrapolationTime < 0.25f)
					{
						float num2 = Time.smoothDeltaTime * (1f + TRCInterpolator.currentTimeModifier);
						this._extrapolationTime += num2;
						this.Position += interpolatorState2.Speed * num2;
						Quaternion lhs = new Quaternion(0f, 0.5f * interpolatorState2.AngSpeed, 0f, 0f);
						Quaternion quaternion = lhs * this.Rotation;
						this.Rotation.x = this.Rotation.x + quaternion.x * num2;
						this.Rotation.y = this.Rotation.y + quaternion.y * num2;
						this.Rotation.z = this.Rotation.z + quaternion.z * num2;
						this.Rotation.w = this.Rotation.w + quaternion.w * num2;
						this.Rotation.Normalize();
					}
					else if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery || GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.PreReplay)
					{
						this._aboveExtrapolating = true;
					}
				}
				else if (TRCInterpolator.currentTime <= this._stateQueue[0u].Time)
				{
					this._isSynching = false;
					this._isExtrapolating = false;
					this._extrapolationTime = 0f;
					this.Position = this._stateQueue[0u].Pos;
					this.Rotation = this._stateQueue[0u].Rot;
					this.Scale = this._stateQueue[0u].Scale;
				}
				else
				{
					InterpolatorState interpolatorState3 = interpolatorState2;
					InterpolatorState interpolatorState4 = this._stateQueue[num -= 1u];
					while (TRCInterpolator.currentTime < interpolatorState4.Time)
					{
						interpolatorState3 = interpolatorState4;
						interpolatorState4 = this._stateQueue[num -= 1u];
					}
					if (interpolatorState3.ShouldInterpolate)
					{
						float num3 = interpolatorState3.Time - interpolatorState4.Time;
						if (num3 > 0f || Time.timeScale <= 0f)
						{
							float t = (TRCInterpolator.currentTime - interpolatorState4.Time) / num3;
							this.Velocity = Vector3.Lerp(interpolatorState4.Speed, interpolatorState3.Speed, t);
							this.Position = Vector3.Lerp(interpolatorState4.Pos, interpolatorState3.Pos, t);
							this.Rotation = Quaternion.Slerp(interpolatorState4.Rot, interpolatorState3.Rot, t);
							this.Scale = Vector3.Lerp(interpolatorState4.Scale, interpolatorState3.Scale, t);
							if (this._isExtrapolating)
							{
								this._isSynching = true;
								this._isExtrapolating = false;
								this._synchStartTime = TRCInterpolator.currentTime;
								this._synchOffset = this.Transf.position - this.Position;
								this._synchRotation = this.Transf.rotation;
								this._synchExtrapolationTime = this._extrapolationTime;
							}
							if (this._isSynching)
							{
								float num4 = (this._synchExtrapolationTime <= 0f) ? 1f : ((TRCInterpolator.currentTime - this._synchStartTime) / this._synchExtrapolationTime);
								num4 = Mathf.Clamp01(num4);
								this._isSynching = (num4 < 1f);
								float num5 = Time.smoothDeltaTime * (1f + TRCInterpolator.currentTimeModifier);
								this._extrapolationTime = Mathf.Max(0f, this._extrapolationTime - num5);
								this.Position += this._synchOffset * (1f - num4);
								this.Rotation = Quaternion.Slerp(this._synchRotation, this.Rotation, num4);
							}
						}
					}
					else
					{
						this._isSynching = false;
						this._isExtrapolating = false;
						this._extrapolationTime = 0f;
						this.Velocity = Vector3.zero;
						this.Position = interpolatorState3.Pos;
						this.Rotation = interpolatorState3.Rot;
						this.Scale = interpolatorState3.Scale;
					}
				}
			}
			this.Transf.position = this.Position;
			this.Transf.rotation = this.Rotation;
			this.Transf.localScale = this.Scale;
		}

		public int GetContents(ref byte[] data)
		{
			Pocketverse.BitStream stream = this.GetStream();
			if (this.CombatObject && !this.CombatObject.IsAlive() && this.CombatObject.HideOnUnspawn)
			{
				stream.WriteBool(false);
				return stream.CopyToArray(data);
			}
			stream.WriteBool(true);
			stream.WriteCompressedFloat(GameHubBehaviour.Hub.GameTime.GetPlaybackUnityTime());
			Vector3 position = this.Transf.position;
			stream.WriteCompressedFixedPoint(position.x, 3);
			stream.WriteCompressedFixedPoint(position.y, 3);
			stream.WriteCompressedFixedPoint(position.z, 3);
			if (this.CombatMovement)
			{
				stream.WriteBool(true);
				stream.WriteVector3(this.CombatMovement.LastVelocity);
				stream.WriteCompressedFloat(this.CombatMovement.LastAngularVelocity);
				if (this.Car)
				{
					stream.WriteBool(true);
					if (this.WriteRotationZ)
					{
						stream.WriteBool(true);
						stream.WriteUltraCompressedQuaternionXYZ(this.Transf.rotation);
					}
					else
					{
						stream.WriteBool(false);
						stream.WriteShortCompressedQuaternionXY(this.Transf.rotation);
					}
					byte value = (byte)(this.Car.VAxis * 127f);
					byte value2 = (byte)(this.Input.TurretAngle * 0.7111111f);
					stream.WriteByte(value);
					stream.WriteByte(value2);
					stream.WriteSign((int)this.Input.TargetH);
					stream.WriteSign((int)this.Car.HAxis);
					stream.WriteSign((int)this.Input.TargetV);
					stream.WriteCompressedFloat(this.Car.SpeedZ);
					stream.WriteBool(this.Car.IsDrifting);
				}
				else
				{
					stream.WriteBool(false);
				}
			}
			else
			{
				stream.WriteBool(false);
			}
			if (this.Creep)
			{
				stream.WriteBool(true);
				stream.WriteByteVector3(this.Creep.Velocity);
			}
			else
			{
				stream.WriteBool(false);
			}
			if (!this.Car && !this.Creep)
			{
				stream.WriteBool(true);
				stream.WriteUltraCompressedQuaternionXY(this.Transf.rotation);
				if (this._writeScale)
				{
					stream.WriteBool(true);
					Vector3 localScale = this.Transf.localScale;
					stream.WriteCompressedFixedPoint(localScale.x, 3);
					stream.WriteCompressedFixedPoint(localScale.y, 3);
					stream.WriteCompressedFixedPoint(localScale.z, 3);
				}
				else
				{
					stream.WriteBool(false);
				}
			}
			else
			{
				stream.WriteBool(false);
			}
			return stream.CopyToArray(data);
		}

		public void ApplyContents(Pocketverse.BitStream stream, int offset)
		{
			if (!stream.ReadBool())
			{
				return;
			}
			float num = stream.ReadCompressedFloat() + (float)offset;
			if (num > TRCInterpolator._latestUpdateTime)
			{
				TRCInterpolator._latestUpdateTime = num;
				TRCInterpolator._latestUpdateDelay = num - 0.08f;
			}
			Vector3 pos;
			pos.x = stream.ReadCompressedFixedPoint(3);
			pos.y = stream.ReadCompressedFixedPoint(3);
			pos.z = stream.ReadCompressedFixedPoint(3);
			Quaternion rot = this.Transf.rotation;
			Vector3 vector = Vector3.zero;
			Vector3 localScale = this.Transf.localScale;
			float num2 = 0f;
			float targetV = 0f;
			if (stream.ReadBool())
			{
				vector = stream.ReadVector3();
				num2 = stream.ReadCompressedFloat();
				if (this.CombatMovement)
				{
					this.CombatMovement.LastVelocity = vector;
					this.CombatMovement.LastAngularVelocity = num2;
				}
				if (stream.ReadBool())
				{
					if (stream.ReadBool())
					{
						rot = stream.ReadUltraCompressedQuaternionXYZ();
					}
					else
					{
						rot = stream.ReadShortCompressedQuaternionXY();
					}
					this.LastVAxis = stream.ReadByte();
					this.LastTurretAngle = stream.ReadByte();
					float turretAngle = (float)this.LastTurretAngle * 1.40625f;
					float vaxis = (float)((sbyte)this.LastVAxis) / 127f;
					float targetH = (float)stream.ReadSign();
					float haxis = (float)stream.ReadSign();
					targetV = (float)stream.ReadSign();
					float speedZ = stream.ReadCompressedFloat();
					bool isDrifting = stream.ReadBool();
					if (this.Car)
					{
						this.Input.TargetH = targetH;
						this.Car.HAxis = haxis;
						this.Input.TargetV = targetV;
						this.Car.VAxis = vaxis;
						this.Input.TurretAngle = turretAngle;
						this.Car.SpeedZ = speedZ;
						this.Car.IsDrifting = isDrifting;
					}
				}
			}
			if (stream.ReadBool())
			{
				vector = stream.ReadByteVector3();
			}
			if (stream.ReadBool())
			{
				rot = stream.ReadUltraCompressedQuaternionXY();
				if (stream.ReadBool())
				{
					localScale.x = stream.ReadCompressedFixedPoint(3);
					localScale.y = stream.ReadCompressedFixedPoint(3);
					localScale.z = stream.ReadCompressedFixedPoint(3);
				}
			}
			this.AddState(pos, rot, vector, num2, num, targetV, true, localScale);
		}

		private void AddState(Vector3 pos, Quaternion rot, Vector3 speed, float angSpeed, float time, float targetV, bool shouldInterpolate, Vector3 scale)
		{
			if (!this._stateQueue.IsEmpty && time <= this._stateQueue[this._stateQueue.Size - 1u].Time)
			{
				return;
			}
			InterpolatorState item = new InterpolatorState
			{
				Pos = pos,
				Rot = rot,
				Speed = speed,
				AngSpeed = angSpeed,
				Time = time,
				TargetV = targetV,
				ShouldInterpolate = shouldInterpolate,
				Scale = scale
			};
			if (this._stateQueue.IsFull)
			{
				this._stateQueue.Pop();
			}
			this._stateQueue.Push(item);
		}

		private void OnObjectSpawned(SpawnEvent evt)
		{
			BombScoreBoard.State currentState = GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState;
			if (currentState == BombScoreBoard.State.BombDelivery || currentState == BombScoreBoard.State.PreReplay)
			{
				if (this._stateQueue.Size > 0u)
				{
					this.AddState(evt.Position, this.Rotation, Vector3.zero, 0f, this._stateQueue[this._stateQueue.Size - 1u].Time + 0.1f, 0f, false, base.transform.localScale);
				}
				this.Reposition();
			}
		}

		private void OnEnable()
		{
			this.Reposition();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TRCInterpolator));

		public const int MaxSamples = 1024;

		public const float Delay = 0.08f;

		public Vector3 Position;

		public Vector3 Velocity;

		public Quaternion Rotation;

		public Vector3 Scale;

		public Transform Transf;

		public Rigidbody Body;

		public CombatMovement CombatMovement;

		public CarMovement Car;

		public CarInput Input;

		public CreepController Creep;

		public CombatObject CombatObject;

		public SpawnController Spawn;

		private int _shaderPositionProperty;

		private readonly CircularQueue<InterpolatorState> _stateQueue = new CircularQueue<InterpolatorState>(1024u);

		private Pocketverse.BitStream _bitstream;

		private static readonly List<TRCInterpolator> _interpolators = new List<TRCInterpolator>();

		private static float currentTime;

		private static float currentTimeModifier = 0f;

		private static float _latestUpdateTime;

		private static float _latestUpdateDelay;

		private Vector3 _synchOffset;

		private Quaternion _synchRotation;

		private float _extrapolationTime;

		private float _synchStartTime;

		private float _synchExtrapolationTime;

		private bool _isSynching;

		private bool _isExtrapolating;

		private bool _aboveExtrapolating;

		private const float ExtrapolationLimit = 0.25f;

		public bool WriteRotationZ;

		[SerializeField]
		private bool _writeScale;

		public byte LastVAxis;

		public byte LastTurretAngle;

		public delegate void TDOnTransformUpdatedDelegate(Vector3 position, Quaternion rotation);
	}
}

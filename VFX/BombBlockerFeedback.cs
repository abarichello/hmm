using System;
using FMod;
using HeavyMetalMachines.Combat;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class BombBlockerFeedback : GameHubBehaviour
	{
		private void Start()
		{
			BombBlockerFeedback.Instance = this;
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
			}
		}

		public bool getClosestPoint(Vector3 position, float maxRange, out Vector3 closestPosition)
		{
			Vector3 vector = Vector3.zero;
			float num = 0f;
			float num2 = maxRange * maxRange;
			bool flag = false;
			position.y = 0f;
			if (position.z > 0f)
			{
				flag = true;
				position = -position;
			}
			bool flag2 = false;
			for (int i = 0; i < this.points.Length - 1; i++)
			{
				BombBlockerFeedback.BombBlockerPoint bombBlockerPoint = this.points[i];
				if (!bombBlockerPoint.isFinalPoint)
				{
					BombBlockerFeedback.BombBlockerPoint bombBlockerPoint2 = this.points[i + 1];
					Vector3 vector2 = position - bombBlockerPoint.point;
					Vector3 vector3 = bombBlockerPoint2.point - bombBlockerPoint.point;
					float num3 = Vector3.Dot(vector2, vector3);
					num3 /= vector3.sqrMagnitude;
					if (num3 > 1f)
					{
						num3 = 1f;
					}
					if (num3 < 0f)
					{
						num3 = 0f;
					}
					Vector3 vector4 = vector3 * num3;
					float num4 = Vector3.SqrMagnitude(vector2 - vector4);
					if (num4 < num2)
					{
						if (flag2)
						{
							if (num4 < num)
							{
								vector = vector4 + bombBlockerPoint.point;
								num = num4;
							}
						}
						else
						{
							vector = vector4 + bombBlockerPoint.point;
							num = num4;
						}
						flag2 = true;
					}
					else if (flag2)
					{
						break;
					}
				}
			}
			if (flag)
			{
				vector = -vector;
			}
			closestPosition = vector;
			return flag2;
		}

		public void OnEnable()
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDrop += this.OnBombDropped;
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.OnBombPicked;
			if (this.audioTransform == null)
			{
				this.audioTransform = new GameObject("BombBlockerAudio").transform;
				this.audioTransform.parent = base.transform;
			}
		}

		public void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDrop -= this.OnBombDropped;
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.OnBombPicked;
		}

		private void OnDestroy()
		{
			this.CleanAudio();
		}

		private void OnBombPicked(CombatObject carrier)
		{
			this.OnPlayerGetBomb((!carrier) ? null : carrier.transform);
		}

		private void OnBombDropped(BombInstance bombInstance, SpawnReason reason, int causer)
		{
			if (reason == SpawnReason.TriggerDrop)
			{
				this.OnHitBombDropper();
			}
			this.bombOwner = null;
		}

		protected void LateUpdate()
		{
			if (this.bombOwner == null)
			{
				return;
			}
			if (this.updateDelay > 0f)
			{
				this.updateDelay -= Time.deltaTime;
				this.interpolatedPosition = Vector3.Lerp(this.previousPoint, this.point, 1f - this.updateDelay / this.updateInterval);
				this.audioTransform.position = this.interpolatedPosition;
				return;
			}
			this.UpdateClosestBombBlocker();
		}

		private void UpdateClosestBombBlocker()
		{
			this.previousPoint = this.point;
			Vector3 vector;
			if (this.getClosestPoint(this.bombOwner.transform.position, 20f, out vector))
			{
				this.point = vector;
				if (!this.wasCloserToBombBlocker)
				{
					this.previousPoint = this.point;
				}
				this.wasCloserToBombBlocker = true;
				this.updateDelay = this.updateInterval;
			}
			else
			{
				this.wasCloserToBombBlocker = false;
				this.updateDelay = 0f;
			}
		}

		private void OnHitBombDropper()
		{
			if (this.bombOwner)
			{
				this.dropFeedback.transform.position = this.bombOwner.position;
				this.dropFeedback.Play();
			}
			this.bombOwner = null;
		}

		private void OnPlayerGetBomb(Transform target)
		{
			if (target == null)
			{
				this.CleanAudio();
				return;
			}
			if (this.bombBlockerAudioInstance == null)
			{
				this.bombBlockerAudioInstance = FMODAudioManager.PlayAt(this.bombBlockerAudio, this.audioTransform);
			}
			this.updateDelay = 0f;
			this.bombOwner = target;
		}

		private void CleanAudio()
		{
			if (this.bombBlockerAudioInstance != null)
			{
				this.bombBlockerAudioInstance.Stop();
				this.bombBlockerAudioInstance = null;
			}
		}

		private void OnDrawGizmos()
		{
			Matrix4x4 matrix = default(Matrix4x4);
			for (int i = 0; i < this.points.Length - 1; i++)
			{
				BombBlockerFeedback.BombBlockerPoint bombBlockerPoint = this.points[i];
				if (!bombBlockerPoint.isFinalPoint)
				{
					BombBlockerFeedback.BombBlockerPoint bombBlockerPoint2 = this.points[i + 1];
					Vector3 vector = bombBlockerPoint2.point - bombBlockerPoint.point;
					Vector3 vector2 = bombBlockerPoint.point + vector * 0.5f;
					vector2.y += 5f;
					matrix.SetTRS(vector2, Quaternion.LookRotation(vector, Vector3.up), Vector3.one);
					Gizmos.matrix = matrix;
					Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
					Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 10f, vector.magnitude));
					Vector3 vector3 = bombBlockerPoint2.point - bombBlockerPoint.point;
					Vector3 vector4 = bombBlockerPoint.point + vector * 0.5f;
					vector3.x = -vector3.x;
					vector3.z = -vector3.z;
					vector4.x = -vector4.x;
					vector4.z = -vector4.z;
					vector4.y += 5f;
					matrix.SetTRS(vector4, Quaternion.LookRotation(vector3, Vector3.up), Vector3.one);
					Gizmos.matrix = matrix;
					Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
					Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 10f, vector3.magnitude));
				}
			}
		}

		public bool editionMode;

		public BombBlockerFeedback.BombBlockerPoint[] points;

		private Transform bombOwner;

		public HoplonParticleSystem dropFeedback;

		private Transform audioTransform;

		public AudioEventAsset bombBlockerAudio;

		private FMODAudioManager.FMODAudio bombBlockerAudioInstance;

		public Vector3 interpolatedPosition;

		public static BombBlockerFeedback Instance;

		private bool wasCloserToBombBlocker;

		private Vector3 previousPoint;

		private Vector3 point;

		private float updateDelay;

		public float updateInterval = 0.1f;

		private float intensity;

		[Serializable]
		public class BombBlockerPoint
		{
			public Vector3 point;

			public bool isFinalPoint;
		}
	}
}

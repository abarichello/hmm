using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class SkidMarkEmitter : GameHubBehaviour
	{
		public GroundType GroundType
		{
			get
			{
				return this._groundType;
			}
			set
			{
				this._groundType = value;
			}
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			this.points = new SkidMarkEmitter.SkidMarkPoint[this.maxPoints];
			this._trans = base.transform;
		}

		private void Awake()
		{
			this.lastPosition = base.transform.position;
		}

		private void OnDisable()
		{
			this.wasEmiting = false;
		}

		public void Emit()
		{
			if (this.points == null)
			{
				return;
			}
			this.hasChanged = false;
			bool flag = true;
			float num = (base.transform.position - this.lastPosition).magnitude / (Time.time - this.lastTime);
			float num2 = Mathf.Clamp01(0.2f + num / 2.5f);
			this.lastPosition = base.transform.position;
			this.lastTime = Time.time;
			float num3 = this.width * 0.5f;
			Vector3 vector = this._trans.position + base.transform.right * (num3 * Mathf.Sign(base.transform.localPosition.x));
			this.lastPoint = (this.firstPoint + this.numOfPoints) % this.maxPoints;
			Vector3 vector2 = vector - Vector3.up * this.wheelRadius;
			if ((this.doEmission || this.wasEmiting != this.doEmission) && (this.wasEmiting != this.doEmission || (Vector3.SqrMagnitude(this.lastSpawnedPoint - vector2) > this.width * 2f && this.doEmission)))
			{
				if (this.numOfPoints >= this.maxPoints)
				{
					this.firstPoint++;
					this.firstPoint %= this.maxPoints;
				}
				else
				{
					this.numOfPoints++;
				}
				int num4 = this.lastPoint;
				this.lastPoint = (this.firstPoint + this.numOfPoints) % this.maxPoints;
				this.phaser++;
				SkidMarkEmitter.SkidMarkPoint skidMarkPoint = default(SkidMarkEmitter.SkidMarkPoint);
				skidMarkPoint.position = vector2;
				if (this.doEmission && !this.wasEmiting)
				{
					skidMarkPoint.cross = this._trans.right;
					num2 *= 0f;
				}
				else if (num4 == -1)
				{
					skidMarkPoint.textureU = 0f;
					skidMarkPoint.cross = this._trans.right;
					num2 *= (float)(this.doEmission ? 1 : 0);
				}
				else
				{
					Vector3 vector3 = this._trans.position - this.points[num4].position;
					float magnitude = vector3.magnitude;
					skidMarkPoint.textureU = this.points[num4].textureU + magnitude;
					skidMarkPoint.cross = Vector3.Cross(Vector3.up, vector3);
					num2 *= (float)(this.doEmission ? 1 : 0);
				}
				ReallyFastMath.FastNormalize(ref skidMarkPoint.cross);
				skidMarkPoint.cross.x = skidMarkPoint.cross.x * num3;
				skidMarkPoint.cross.y = skidMarkPoint.cross.y * num3;
				skidMarkPoint.cross.z = skidMarkPoint.cross.z * num3;
				skidMarkPoint.precomputedPointA = skidMarkPoint.position - skidMarkPoint.cross;
				skidMarkPoint.precomputedPointB = skidMarkPoint.position + skidMarkPoint.cross;
				skidMarkPoint.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				skidMarkPoint.alpha = num2;
				this.lastSpawnedPoint = skidMarkPoint.position;
				this.points[this.lastPoint] = skidMarkPoint;
				this.hasChanged = true;
				if (this.numOfPoints == 1)
				{
					this.Bounds = new Bounds(this.lastSpawnedPoint, Vector3.zero);
				}
				this.Bounds.Encapsulate(this.lastSpawnedPoint);
				for (int i = 1; i < this.maxPoints / 4; i++)
				{
					int num5 = (this.firstPoint + i) % this.maxPoints;
					SkidMarkEmitter.SkidMarkPoint[] array = this.points;
					int num6 = num5;
					array[num6].alpha = array[num6].alpha * 0.9f;
				}
				this.points[(this.firstPoint + 1) % this.maxPoints].alpha = 0f;
			}
			this.cleanBoundsUpdateDelay -= Time.deltaTime;
			if (this.cleanBoundsUpdateDelay < 0f)
			{
				this.cleanBoundsUpdateDelay = 0.5f;
				if (this.hasChanged)
				{
					if (this.points.Length > 0)
					{
						Vector3 position = this.points[0].position;
						Vector3 position2 = this.points[0].position;
						for (int j = 0; j < this.numOfPoints; j++)
						{
							SkidMarkEmitter.SkidMarkPoint skidMarkPoint2 = this.points[j];
							position.x = ((position.x >= skidMarkPoint2.position.x) ? (skidMarkPoint2.position.x - this.width) : position.x);
							position.y = ((position.y >= skidMarkPoint2.position.y) ? (skidMarkPoint2.position.y - this.width) : position.y);
							position.z = ((position.z >= skidMarkPoint2.position.z) ? (skidMarkPoint2.position.z - this.width) : position.z);
							position2.x = ((position2.x <= skidMarkPoint2.position.x) ? (skidMarkPoint2.position.x + this.width) : position2.x);
							position2.y = ((position2.y <= skidMarkPoint2.position.y) ? (skidMarkPoint2.position.y + this.width) : position2.y);
							position2.z = ((position2.z <= skidMarkPoint2.position.z) ? (skidMarkPoint2.position.z + this.width) : position2.z);
						}
						this.Bounds.max = position2;
						this.Bounds.min = position;
					}
					Plane[] array2 = GeometryUtility.CalculateFrustumPlanes(this._gameCameraEngine.UnityCamera);
					this.isVisible = GeometryUtility.TestPlanesAABB(array2, this.Bounds);
				}
			}
			this.wasEmiting = (this.doEmission && flag);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireCube(this.Bounds.center, this.Bounds.size);
		}

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private bool emitSmoke;

		public Vector3 smokeCenter;

		private GroundType _groundType;

		public int offsetDebug;

		public int numOfPoints = -1;

		public int firstPoint;

		public SkidMarkEmitter.SkidMarkPoint[] points;

		public float width = 0.5f;

		public float wheelRadius = 0.5f;

		private Vector3 lastPosition = Vector3.zero;

		private float lastTime;

		public bool doEmission;

		private bool wasEmiting;

		public int maxPoints = 64;

		private Transform _trans;

		[NonSerialized]
		public int lastPoint;

		public Bounds Bounds;

		private Vector3 lastSpawnedPoint;

		public bool isVisible;

		public bool hasChanged;

		private int phaser;

		public float cleanBoundsUpdateDelay = 0.5f;

		public struct SkidMarkPoint
		{
			public Vector3 position;

			public Vector3 cross;

			public Color32 color;

			public float alpha;

			public float textureU;

			public Vector3 precomputedPointA;

			public Vector3 precomputedPointB;
		}
	}
}

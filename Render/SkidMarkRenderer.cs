using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class SkidMarkRenderer : GameHubBehaviour
	{
		private void Awake()
		{
			Object.Destroy(this);
		}

		private void Start()
		{
			this._trans = base.transform;
			this.emitters = base.GetComponentsInChildren<SkidMarkEmitter>();
			this.emitterStride = this.maxPoints / this.emitters.Length;
			for (int i = 0; i < this.emitters.Length; i++)
			{
				this.emitters[i].maxPoints = this.emitterStride;
			}
			this.skidMarkMesh = new Mesh();
			this.skidMarkMesh.MarkDynamic();
			this.indices = new int[this.maxPoints * 6];
			this.vertices = new Vector3[this.maxPoints * 2];
			this.uvs = new Vector2[this.maxPoints * 2];
			this.colors = new Color32[this.maxPoints * 2];
			for (int j = 0; j < this.maxPoints; j++)
			{
				int num = j * 2;
				int num2 = j * 6;
				if (j < this.maxPoints - 1)
				{
					this.indices[num2] = num;
					this.indices[num2 + 1] = num + 1;
					this.indices[num2 + 2] = num + 2;
					this.indices[num2 + 3] = num + 1;
					this.indices[num2 + 4] = num + 3;
					this.indices[num2 + 5] = num + 2;
				}
				this.uvs[num] = new Vector2(0f, (float)j * this.texCoordScale);
				this.uvs[num + 1] = new Vector2(1f, (float)j * this.texCoordScale);
			}
			for (int k = 1; k < this.emitters.Length; k++)
			{
				int num3 = (k * this.emitterStride - 1) * 6;
				this.indices[num3] = 0;
				this.indices[num3 + 1] = 0;
				this.indices[num3 + 2] = 0;
				this.indices[num3 + 3] = 0;
				this.indices[num3 + 4] = 0;
				this.indices[num3 + 5] = 0;
			}
		}

		private void LateUpdate()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			int num = 0;
			if (this.emitters == null)
			{
				return;
			}
			if (this._gameCameraEngine == null || this._trans == null)
			{
				return;
			}
			float num2 = Vector3.SqrMagnitude(this._gameCameraEngine.CameraTransform.position - this._trans.position);
			if (num2 > this.maxDistanceToCam * this.maxDistanceToCam)
			{
				return;
			}
			for (int i = 0; i < this.emitters.Length; i++)
			{
				SkidMarkEmitter skidMarkEmitter = this.emitters[i];
				if (!(skidMarkEmitter == null))
				{
					skidMarkEmitter.Emit();
					if (skidMarkEmitter.isVisible)
					{
						if (!flag2)
						{
							this.bounds = skidMarkEmitter.Bounds;
							flag2 = true;
						}
						else
						{
							this.bounds.Encapsulate(skidMarkEmitter.Bounds);
						}
						flag = true;
						flag3 = (flag3 || skidMarkEmitter.hasChanged);
						Vector3 vector = skidMarkEmitter.transform.position - new Vector3(0f, skidMarkEmitter.wheelRadius, 0f);
						skidMarkEmitter.offsetDebug = num;
						if (skidMarkEmitter.numOfPoints >= 1)
						{
							int num3 = 0;
							Vector3 vector2 = skidMarkEmitter.points[0].position - skidMarkEmitter.points[1].position;
							Vector3 vector3 = Vector3.zero;
							for (int j = skidMarkEmitter.firstPoint + 1; j <= skidMarkEmitter.firstPoint + skidMarkEmitter.numOfPoints; j++)
							{
								int num4 = j % skidMarkEmitter.maxPoints;
								vector3 = skidMarkEmitter.points[num4].cross;
								int num5 = num3 + num;
								int num6 = num3 + 1 + num;
								this.vertices[num5] = skidMarkEmitter.points[num4].precomputedPointA;
								this.vertices[num6] = skidMarkEmitter.points[num4].precomputedPointB;
								this.uvs[num5].y = skidMarkEmitter.points[num4].textureU * this.texCoordScale;
								this.uvs[num6].y = skidMarkEmitter.points[num4].textureU * this.texCoordScale;
								Color color = skidMarkEmitter.points[num4].color;
								color.a *= skidMarkEmitter.points[num4].alpha;
								this.colors[num5] = (this.colors[num6] = color);
								num3 += 2;
							}
							vector2 = vector - skidMarkEmitter.points[skidMarkEmitter.lastPoint].position;
							Vector3 up = base.transform.up;
							ReallyFastMath.FastCross(ref vector2, ref up, ref vector3);
							ReallyFastMath.FastNormalize(ref vector3);
							vector3.x *= skidMarkEmitter.width;
							vector3.y *= skidMarkEmitter.width;
							vector3.z *= skidMarkEmitter.width;
							int num7 = skidMarkEmitter.maxPoints * 2;
							this.colors[num3 % num7 + num] = new Color32(0, 0, 0, 0);
							this.colors[(num3 + 1) % num7 + num] = new Color32(0, 0, 0, 0);
							this.colors[(num3 + 2) % num7 + num] = new Color32(0, 0, 0, 0);
							this.colors[(num3 + 3) % num7 + num] = new Color32(0, 0, 0, 0);
							if (skidMarkEmitter.doEmission)
							{
								this.vertices[num3 % num7 + num] = vector - vector3;
								this.vertices[(num3 + 1) % num7 + num] = vector + vector3;
							}
							num += this.emitterStride * 2;
						}
					}
				}
			}
			if (!flag || !flag3)
			{
				Graphics.DrawMesh(this.skidMarkMesh, Vector3.zero, Quaternion.identity, this.skidMarkMaterial, base.gameObject.layer);
				return;
			}
			this.skidMarkMesh.vertices = this.vertices;
			this.skidMarkMesh.uv = this.uvs;
			this.skidMarkMesh.colors32 = this.colors;
			this.skidMarkMesh.SetTriangles(this.indices, 0);
			this.skidMarkMesh.bounds = this.bounds;
			Graphics.DrawMesh(this.skidMarkMesh, Vector3.zero, Quaternion.identity, this.skidMarkMaterial, base.gameObject.layer);
		}

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private SkidMarkEmitter[] emitters;

		private Mesh skidMarkMesh;

		private Vector3[] vertices;

		private int[] indices;

		private Vector2[] uvs;

		private Color32[] colors;

		public Material skidMarkMaterial;

		public int maxPoints = 256;

		public float texCoordScale = 0.1f;

		public int emitterStride;

		public float maxDistanceToCam = 250f;

		private Transform _trans;

		private Bounds bounds = default(Bounds);
	}
}

using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Render
{
	public class BlackLotusScarf : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
			}
			else
			{
				this._velocityPropertyId = Shader.PropertyToID("_Velocity");
				this._specialPropertyId = Shader.PropertyToID("_Special");
			}
			this._combat = base.GetComponentInParent<CombatObject>();
			if (this._combat == null || this._combat.GadgetStates == null)
			{
				this._combatTransform = base.transform.parent;
			}
			else
			{
				this._combatTransform = this._combat.Transform;
				this.gadgetState = this._combat.GadgetStates.GetGadgetState(this.linkSpecialToGadget);
			}
			this.BuildScarf();
		}

		private void Start()
		{
			this._material = ((!(GameHubBehaviour.Hub == null) && this._combat.Team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? this.EnemyMaterial : this.TeamMaterial);
		}

		private void BuildScarf()
		{
			this.points = new BlackLotusScarf.scarfPoint[this.numPoints];
			this.stepSize = this.scarfLength / (float)this.numPoints;
			this.points[0].position = base.transform.position;
			for (int i = 1; i < this.numPoints; i++)
			{
				this.points[i].position = this.points[0].position - new Vector3(0f, this.stepSize * (float)i, 0f);
			}
			this.mesh = new Mesh();
			this.vertices = new Vector3[this.numPoints * 2];
			this.tangents = new Vector4[this.numPoints * 2];
			this.uvs = new Vector2[this.numPoints * 2];
			this.indices = new int[this.numPoints * 6];
			int num = 0;
			for (int j = 0; j < (this.numPoints - 1) * 6; j += 6)
			{
				this.indices[j] = num;
				this.indices[j + 1] = num + 1;
				this.indices[j + 2] = num + 2;
				this.indices[j + 3] = num + 2;
				this.indices[j + 4] = num + 1;
				this.indices[j + 5] = num + 3;
				num += 2;
			}
			float num2 = 0f;
			float num3 = 0.5f / (float)(this.numPoints - 1);
			for (int k = 0; k < this.numPoints * 2; k += 2)
			{
				this.tangents[k] = new Vector4(-1f, 0f, 0f, 1f);
				this.tangents[k + 1] = new Vector4(-1f, 0f, 0f, 1f);
				this.uvs[k] = new Vector2(-this.textureSpacing, num2);
				this.uvs[k + 1] = new Vector2(1f + this.textureSpacing, num2);
				num2 += num3 + num3;
			}
			this.mesh.vertices = this.vertices;
			this.mesh.uv = this.uvs;
			this.materialPropertyBlock = new MaterialPropertyBlock();
			this.materialPropertyBlock.SetFloat(this._velocityPropertyId, 0.5f);
		}

		private void LateUpdate()
		{
			this.velocity = Mathf.Min(Vector3.Distance(this.lastPosition, this._combatTransform.position) / Time.deltaTime, 1f);
			if (this.gadgetState != null)
			{
				this.specialAvailable = (this.gadgetState.GadgetState == GadgetState.Ready);
			}
			this.points[0].position = base.transform.position;
			Vector3 vector = this._combatTransform.TransformDirection(this.planeNormal.normalized);
			this.plane.SetNormalAndPosition(vector, this._combatTransform.position + this.planeOffset);
			float num = this.scarfWidth * 0.5f;
			for (int i = 0; i < this.numPoints; i++)
			{
				Vector3 vector3;
				if (i > 0)
				{
					BlackLotusScarf.scarfPoint[] array = this.points;
					int num2 = i;
					array[num2].velocity.y = array[num2].velocity.y + this.gravity;
					BlackLotusScarf.scarfPoint[] array2 = this.points;
					int num3 = i;
					array2[num3].position = array2[num3].position + this.points[i].velocity * Time.deltaTime;
					float num4 = 3f;
					if (!this.plane.GetSide(this.points[i].position))
					{
						float num5 = Mathf.Abs(Vector3.Project(this.points[i].position - this._combatTransform.position, this._combatTransform.forward).magnitude);
						float num6 = Mathf.Abs(Vector3.Project(this.points[i].position - this._combatTransform.position, this._combatTransform.right).magnitude);
						if (num5 < this.planeSize.y && num6 < this.planeSize.x && this.points[i].position.y > this._combatTransform.position.y)
						{
							BlackLotusScarf.scarfPoint[] array3 = this.points;
							int num7 = i;
							array3[num7].position = array3[num7].position - this.plane.GetDistanceToPoint(this.points[i].position) * vector;
							num4 = 3f;
						}
					}
					BlackLotusScarf.scarfPoint[] array4 = this.points;
					int num8 = i;
					array4[num8].velocity = array4[num8].velocity * Mathf.Max(1f - Time.deltaTime * num4, 0f);
					Vector3 vector2 = this.points[i].position - this.points[i - 1].position;
					float num9 = Mathf.Max(vector2.magnitude, 0.1f);
					Vector3 position = this.points[i].position;
					vector3 = vector2 / num9;
					if (num9 > this.stepSize)
					{
						this.points[i].position = this.points[i - 1].position + vector3 * this.stepSize;
					}
					BlackLotusScarf.scarfPoint[] array5 = this.points;
					int num10 = i;
					array5[num10].velocity = array5[num10].velocity + (this.points[i].position - position) / Time.deltaTime * 0.1f;
					float num11 = 1f - this.velocity;
					float num12 = this.windForce * (1f + Mathf.PerlinNoise(Time.time * this.shakingSpeed + (float)i, 0f)) * num11;
					BlackLotusScarf.scarfPoint[] array6 = this.points;
					int num13 = i;
					array6[num13].velocity = array6[num13].velocity + num12 * -this._combatTransform.forward / Time.deltaTime * 0.1f;
					float num14 = Time.time * this.ampTime + (float)i * this.amp;
					BlackLotusScarf.scarfPoint[] array7 = this.points;
					int num15 = i;
					array7[num15].velocity = array7[num15].velocity + this._combatTransform.right * (Mathf.PerlinNoise(num14, 0f) - 0.5f) * this.shaking * ((float)i / (float)this.numPoints) * 2f * num11;
				}
				else
				{
					vector3 = (this.points[1].position - this.points[0].position).normalized;
				}
				Vector3 vector4 = Vector3.Normalize(Vector3.Cross(vector3, Vector3.up)) * num;
				float num16 = 1f + (float)i / (float)this.numPoints;
				this.vertices[i * 2] = this.points[i].position - vector4 * 0.25f * num16;
				this.vertices[i * 2 + 1] = this.points[i].position + vector4 * 0.25f * num16;
			}
			this.mesh.vertices = this.vertices;
			this.mesh.SetIndices(this.indices, 0, 0);
			this.mesh.RecalculateNormals();
			this.mesh.RecalculateBounds();
			this.mesh.tangents = this.tangents;
			this.materialPropertyBlock.SetFloat(this._specialPropertyId, (float)((!this.specialAvailable) ? 0 : 1));
			this.materialPropertyBlock.SetFloat(this._velocityPropertyId, (this.velocity + 1f) * 0.5f);
			Graphics.DrawMesh(this.mesh, Matrix4x4.identity, this._material, base.gameObject.layer, null, 0, this.materialPropertyBlock);
			this.lastPosition = this._combatTransform.position;
		}

		private void OnDrawGizmos()
		{
			if (this.points == null || this.points.Length < this.numPoints)
			{
				return;
			}
			for (int i = this.numPoints - 1; i >= 0; i--)
			{
				Gizmos.color = Color.white;
				if (!this.plane.GetSide(this.points[i].position))
				{
					float num = Mathf.Abs(Vector3.Project(this.points[i].position - this._combatTransform.position, -this._combatTransform.right).magnitude);
					if (num > this.planeSize.x + 0.2f)
					{
						Gizmos.color = Color.blue;
					}
					else
					{
						Gizmos.color = Color.red;
					}
				}
				Gizmos.DrawSphere(this.points[i].position, 0.05f);
				if (i > 0)
				{
					Gizmos.color = Color.Lerp(Color.red, Color.green, (float)i / (float)this.numPoints);
					Gizmos.DrawLine(this.points[i].position, this.points[i - 1].position);
				}
			}
			Matrix4x4 matrix = Matrix4x4.TRS(this._combatTransform.position + this.planeOffset, this._combatTransform.rotation * Quaternion.LookRotation(this.planeNormal), Vector3.one);
			Gizmos.matrix = matrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(this.planeSize.x * 2f, this.planeSize.y * 2f, 0.1f));
		}

		private void OnValidate()
		{
			if (this.TeamMaterial == null && this.EnemyMaterial != null)
			{
				this.TeamMaterial = this.EnemyMaterial;
			}
			else if (this.TeamMaterial != null && this.EnemyMaterial == null)
			{
				this.EnemyMaterial = this.TeamMaterial;
			}
		}

		public float amp = 0.5f;

		public float ampTime = 2f;

		public float debug;

		public float gravity = -9.8f;

		private int[] indices;

		private Vector3 lastPosition;

		[FormerlySerializedAs("material")]
		public Material TeamMaterial;

		public Material EnemyMaterial;

		private Material _material;

		private Mesh mesh;

		public int numPoints;

		private Plane plane;

		public Vector3 planeNormal;

		public Vector3 planeOffset;

		public Vector2 planeSize;

		private Vector4[] tangents;

		private BlackLotusScarf.scarfPoint[] points;

		public float scarfLength;

		public float scarfWidth = 1f;

		public float shaking = 2f;

		public float shakingSpeed = 1f;

		private float stepSize;

		public float textureSpacing;

		private Vector2[] uvs;

		private Transform _combatTransform;

		private CombatObject _combat;

		private float velocity;

		private Vector3[] vertices;

		public float windForce = -1f;

		private MaterialPropertyBlock materialPropertyBlock;

		public bool specialAvailable;

		public GadgetSlot linkSpecialToGadget;

		private GadgetData.GadgetStateObject gadgetState;

		private int _velocityPropertyId = -1;

		private int _specialPropertyId = -1;

		private struct scarfPoint
		{
			public Vector3 position;

			public Vector3 velocity;
		}
	}
}

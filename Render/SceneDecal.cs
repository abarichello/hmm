using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	[ExecuteInEditMode]
	public class SceneDecal : GameHubBehaviour
	{
		public Material Material
		{
			get
			{
				return this.materialInstance ?? this.material;
			}
		}

		private void SetSpriteAnimation(int colCount, int rowCount, int fps)
		{
			int num = (int)((Time.time - this.startTime) * (float)fps);
			num %= colCount + rowCount;
			float x = 1f / (float)colCount;
			float y = 1f / (float)rowCount;
			Vector2 value = new Vector2(x, y);
			int num2 = num % colCount;
			int num3 = num / colCount;
			float x2 = (float)num2 * value.x;
			float y2 = 1f - value.y - (float)num3 * value.y;
			Vector2 value2 = new Vector2(x2, y2);
			this.materialInstance.SetTextureOffset("_MainTex", value2);
			this.materialInstance.SetTextureScale("_MainTex", value);
		}

		private void OnEnable()
		{
			if (this.material != null)
			{
				this.materialInstance = UnityEngine.Object.Instantiate<Material>(this.material);
			}
			this.UpdateDecal();
		}

		private void OnDisable()
		{
			if (this.materialInstance != null)
			{
				UnityEngine.Object.Destroy(this.materialInstance);
			}
		}

		private void Start()
		{
			this.startTime = Time.time;
			if (this.spriteFramesX * this.spriteFramesY > 1)
			{
				this.SetSpriteAnimation(this.spriteFramesX, this.spriteFramesY, this.fps);
			}
			this.UpdateDecal();
		}

		private void LateUpdate()
		{
			if (this.materialInstance == null)
			{
				return;
			}
			if (this.mesh == null && this.materialInstance != null)
			{
				this.UpdateDecal();
				return;
			}
			Graphics.DrawMesh(this.mesh, Matrix4x4.identity, this.materialInstance, base.gameObject.layer);
			if (DateTime.Now - this.LastMeshUpdate > TimeSpan.FromMilliseconds(100.0) && (this._lastPosition != base.transform.position || this._lastRotation != base.transform.rotation || !Mathf.Approximately(this._lastSize, this.size)))
			{
				this.LastMeshUpdate = DateTime.Now;
				this._lastPosition = base.transform.position;
				this._lastRotation = base.transform.rotation;
				this._lastSize = this.size;
				this.UpdateDecal();
			}
			if (this.spriteFramesX * this.spriteFramesY > 1)
			{
				this.SetSpriteAnimation(this.spriteFramesX, this.spriteFramesY, this.fps);
			}
		}

		public void UpdateDecal()
		{
			this.LastMeshUpdate = DateTime.Now;
			if (this.justWithColliders)
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position, this.size, this.layerMask);
				this.Reset();
				if (array != null)
				{
					foreach (Collider collider in array)
					{
						if (!(collider.GetComponent<Renderer>() == null))
						{
							this.ProcessDecalForObject(collider.gameObject, base.transform.position, -base.transform.up, new Vector2(this.size * this.horizontalRatio, this.size), this.size);
						}
					}
				}
			}
			else
			{
				this.Reset();
				MeshRenderer[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
				Bounds bounds = new Bounds(base.transform.position, new Vector3(this.size * this.horizontalRatio, this.size, this.size));
				if (array2 != null)
				{
					foreach (MeshRenderer meshRenderer in array2)
					{
						if (!(meshRenderer == null))
						{
							if ((1 << meshRenderer.gameObject.layer & this.layerMask.value) != 0)
							{
								if (bounds.Intersects(meshRenderer.bounds))
								{
									this.ProcessDecalForObject(meshRenderer.gameObject, base.transform.position, -base.transform.up, new Vector2(this.size * this.horizontalRatio, this.size), this.size);
								}
							}
						}
					}
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = this.RayColor;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawRay(Vector3.zero, -Vector3.up * this.size);
			Gizmos.color = this.CubeColor;
			Vector3 vector = new Vector3(this.size * this.horizontalRatio, this.size, this.size);
			Gizmos.DrawCube(Vector3.zero, vector);
		}

		private void Reset()
		{
			this.triangleCount = 0;
			this.vertexCount = 0;
			this.vertex = null;
			this.normal = null;
			this.tangent = null;
			this.texCoord = null;
			this.color = null;
			this.triangle = null;
			if (this.mesh != null)
			{
				this.mesh.Clear(true);
			}
		}

		private void ProcessDecalForObject(GameObject obj, Vector3 decalPoint, Vector3 decalNormal, Vector2 decalSize, float decalDepth)
		{
			if (obj == null)
			{
				return;
			}
			Vector3[] array = new Vector3[9];
			Vector3[] array2 = new Vector3[9];
			Vector4[] array3 = new Vector4[9];
			if (this.vertex == null)
			{
				this.vertex = new Vector3[SceneDecal.maxDecalVertices];
			}
			if (this.normal == null)
			{
				this.normal = new Vector3[SceneDecal.maxDecalVertices];
			}
			if (this.tangent == null)
			{
				this.tangent = new Vector4[SceneDecal.maxDecalVertices];
			}
			if (this.texCoord == null)
			{
				this.texCoord = new Vector2[SceneDecal.maxDecalVertices];
			}
			if (this.color == null)
			{
				this.color = new Color[SceneDecal.maxDecalVertices];
			}
			if (this.triangle == null)
			{
				this.triangle = new int[SceneDecal.maxDecalVertices * 3];
			}
			decalSize /= obj.transform.localScale.x;
			this.decalSize = decalSize;
			this.decalNormal = decalNormal;
			MeshFilter component = obj.GetComponent<MeshFilter>();
			if (!component)
			{
				return;
			}
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh == null)
			{
				return;
			}
			this.meshVertices = sharedMesh.vertices;
			this.meshNormals = sharedMesh.normals;
			int[] triangles = sharedMesh.triangles;
			decalNormal = obj.transform.InverseTransformDirection(decalNormal);
			decalNormal = obj.transform.InverseTransformDirection(base.transform.up);
			if (decalNormal == Vector3.up)
			{
				this.T = Vector3.Cross(new Vector3(1E-05f, 1f, 1E-05f), decalNormal).normalized;
			}
			else
			{
				this.T = Vector3.Cross(Vector3.up, decalNormal).normalized;
			}
			this.T = obj.transform.InverseTransformDirection(base.transform.right);
			this.B = Vector3.Cross(decalNormal, this.T).normalized;
			this.B = obj.transform.InverseTransformDirection(base.transform.forward);
			this.P = obj.transform.InverseTransformPoint(decalPoint);
			this.left = new Plane(this.T, decalSize.x * 0.5f * this.T);
			this.right = new Plane(-this.T, decalSize.x * 0.5f * -this.T);
			this.bottom = new Plane(decalNormal, decalDepth * decalNormal);
			this.top = new Plane(-decalNormal, decalDepth * -decalNormal);
			this.front = new Plane(-this.B, decalSize.y * 0.5f * -this.B);
			this.back = new Plane(this.B, decalSize.y * 0.5f * this.B);
			float num = this.size * this.size;
			Vector3[] array4 = new Vector3[3];
			Vector3[] array5 = new Vector3[3];
			for (int i = 0; i < triangles.Length; i += 3)
			{
				array4[0] = this.meshVertices[triangles[i]];
				array4[1] = this.meshVertices[triangles[i + 1]];
				array4[2] = this.meshVertices[triangles[i + 2]];
				array5[0] = this.meshNormals[triangles[i]];
				array5[1] = this.meshNormals[triangles[i + 1]];
				array5[2] = this.meshNormals[triangles[i + 2]];
				Vector3 lhs = array4[1] - array4[0];
				Vector3 rhs = array4[2] - array4[1];
				float num2 = Mathf.Max(Vector3.SqrMagnitude(array4[1] - array4[0]), Vector3.SqrMagnitude(array4[2] - array4[1]));
				float num3 = num + num2;
				if (Vector3.SqrMagnitude(array4[0] - this.P) <= num3 || Vector3.SqrMagnitude(array4[1] - this.P) <= num3 || Vector3.SqrMagnitude(array4[2] - this.P) <= num3)
				{
					Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
					if (Vector3.Dot(decalNormal, normalized) >= this.maxDecalAngle)
					{
						int num4 = this.ClipTriangle(array4, ref array, ref array5, ref array2, ref this.tangent, ref array3);
						if (num4 != 0 && !this.AddTriangle(obj.transform, num4, ref array, ref array2, ref array3))
						{
							break;
						}
					}
				}
			}
			if (this.mesh == null)
			{
				this.mesh = new Mesh();
				this.mesh.name = "Decal mesh";
			}
			this.mesh.vertices = this.vertex;
			this.mesh.uv = this.texCoord;
			this.mesh.normals = this.normal;
			this.mesh.colors = this.color;
			this.mesh.tangents = this.tangent;
			int[] array6 = new int[this.triangleCount];
			for (int j = 0; j < this.triangleCount; j++)
			{
				array6[j] = this.triangle[j];
			}
			this.mesh.SetTriangles(array6, 0);
			this.mesh.RecalculateBounds();
		}

		private bool AddTriangle(Transform objTransform, int count, ref Vector3[] newVertices, ref Vector3[] newNormals, ref Vector4[] newTangent)
		{
			int num = this.vertexCount;
			if (num + count > SceneDecal.maxDecalVertices)
			{
				return false;
			}
			for (int i = 2; i < count + 1; i++)
			{
				this.triangle[this.triangleCount] = num;
				this.triangle[this.triangleCount + 2] = num + (i - 1);
				this.triangle[this.triangleCount + 1] = num + (i - 2);
				this.triangleCount += 3;
			}
			for (int j = 0; j < count; j++)
			{
				this.vertex[num] = objTransform.TransformPoint(newVertices[j]);
				this.texCoord[num] = new Vector2(Vector3.Dot(this.T, newVertices[j] - this.P) / this.decalSize.x + 0.5f, Vector3.Dot(this.B, newVertices[j] - this.P) / this.decalSize.y + 0.5f);
				this.normal[num] = objTransform.TransformDirection(newNormals[j]);
				this.tangent[num] = objTransform.TransformDirection(newTangent[j]);
				this.color[num] = new Color(1f, 1f, 1f, 1f);
				num++;
			}
			this.vertexCount = num;
			return true;
		}

		private int ClipTriangleAgainstPlane(Plane plane, int vertexCount, Vector3[] vertices, ref Vector3[] verticesOutput, ref Vector3[] normals, ref Vector3[] normalsOutput, ref Vector4[] tangents, ref Vector4[] tangentsOutput)
		{
			bool[] array = new bool[vertexCount];
			int num = 0;
			for (int i = 0; i < vertexCount; i++)
			{
				bool side = plane.GetSide(vertices[i] - this.P);
				array[i] = side;
				num += ((!side) ? 0 : 1);
			}
			if (num == vertexCount)
			{
				return 0;
			}
			int num2 = 0;
			for (int j = 0; j < vertexCount; j++)
			{
				int num3 = (j == 0) ? (vertexCount - 1) : (j - 1);
				if (array[j])
				{
					if (!array[num3])
					{
						Vector3 vector = vertices[num3];
						Vector3 a = vertices[j];
						Vector3 a2 = normals[num3];
						Vector3 a3 = normals[j];
						Vector3 a4 = tangents[num3];
						Vector3 a5 = tangents[j];
						Vector3 vector2 = a - vector;
						float num4;
						plane.Raycast(new Ray(vector - this.P, vector2.normalized), out num4);
						num4 /= vector2.magnitude;
						verticesOutput[num2] = vector * (1f - num4) + a * num4;
						normalsOutput[num2] = a2 * (1f - num4) + a3 * num4;
						tangentsOutput[num2] = a4 * (1f - num4) + a5 * num4;
						verticesOutput[num2] += normalsOutput[num2] * this.decalOffset;
						num2++;
					}
				}
				else
				{
					if (array[num3])
					{
						Vector3 vector3 = vertices[j];
						Vector3 a6 = vertices[num3];
						Vector3 a7 = normals[j];
						Vector3 a8 = normals[num3];
						Vector3 a9 = tangents[num3];
						Vector3 a10 = tangents[j];
						Vector3 vector4 = a6 - vector3;
						float num5;
						plane.Raycast(new Ray(vector3 - this.P, vector4.normalized), out num5);
						num5 /= vector4.magnitude;
						verticesOutput[num2] = vector3 * (1f - num5) + a6 * num5;
						normalsOutput[num2] = a7 * (1f - num5) + a8 * num5;
						tangentsOutput[num2] = a9 * (1f - num5) + a10 * num5;
						verticesOutput[num2] += normalsOutput[num2] * this.decalOffset;
						num2++;
					}
					verticesOutput[num2] = vertices[j];
					normalsOutput[num2] = normals[j];
					tangentsOutput[num2] = tangents[j];
					verticesOutput[num2] += normalsOutput[num2] * this.decalOffset;
					num2++;
				}
			}
			return num2;
		}

		private int ClipTriangle(Vector3[] triangle, ref Vector3[] newVertices, ref Vector3[] normal, ref Vector3[] newNormals, ref Vector4[] tangent, ref Vector4[] newTangent)
		{
			int num = this.ClipTriangleAgainstPlane(this.left, 3, triangle, ref this.tempVertices, ref normal, ref this.tempNormals, ref tangent, ref this.tempTangents);
			if (num != 0)
			{
				num = this.ClipTriangleAgainstPlane(this.right, num, this.tempVertices, ref newVertices, ref this.tempNormals, ref newNormals, ref this.tempTangents, ref newTangent);
				if (num != 0)
				{
					num = this.ClipTriangleAgainstPlane(this.bottom, num, newVertices, ref this.tempVertices, ref newNormals, ref this.tempNormals, ref newTangent, ref this.tempTangents);
					if (num != 0)
					{
						num = this.ClipTriangleAgainstPlane(this.top, num, this.tempVertices, ref newVertices, ref this.tempNormals, ref newNormals, ref this.tempTangents, ref newTangent);
						if (num != 0)
						{
							num = this.ClipTriangleAgainstPlane(this.back, num, newVertices, ref this.tempVertices, ref newNormals, ref this.tempNormals, ref newTangent, ref this.tempTangents);
							if (num != 0)
							{
								num = this.ClipTriangleAgainstPlane(this.front, num, this.tempVertices, ref newVertices, ref this.tempNormals, ref newNormals, ref this.tempTangents, ref newTangent);
							}
						}
					}
				}
			}
			return num;
		}

		public float size = 3f;

		public float horizontalRatio = 1f;

		public LayerMask layerMask;

		private Mesh mesh;

		public Material material;

		[NonSerialized]
		public Material materialInstance;

		[HideInInspector]
		public DateTime LastMeshUpdate;

		[NonSerialized]
		private Vector3 _lastPosition;

		[NonSerialized]
		private Quaternion _lastRotation;

		[NonSerialized]
		private float _lastSize;

		public int spriteFramesX = 1;

		public int spriteFramesY = 1;

		public int fps = 5;

		private float startTime;

		public bool justWithColliders = true;

		public float midDecalProcessTime;

		public Color CubeColor = new Color(1f, 1f, 1f, 0.15f);

		public Color RayColor = new Color(1f, 1f, 1f, 0.5f);

		public static readonly int maxDecalVertices = 256;

		public Plane left;

		public Plane right;

		public Plane top;

		public Plane bottom;

		public Plane front;

		public Plane back;

		private int vertexCount;

		private int triangleCount;

		[NonSerialized]
		public Vector3[] meshVertices;

		[NonSerialized]
		public Vector3[] meshNormals;

		[NonSerialized]
		public Vector3[] vertex;

		[NonSerialized]
		public Vector3[] normal;

		[NonSerialized]
		public Vector4[] tangent;

		[NonSerialized]
		public Vector2[] texCoord;

		[NonSerialized]
		public Color[] color;

		[NonSerialized]
		public int[] triangle;

		public Vector3 T;

		public Vector3 B;

		public Vector3 P;

		public Vector3 decalNormal;

		private Vector2 decalSize;

		public float decalOffset;

		[Range(-1f, 1f)]
		public float maxDecalAngle;

		private Vector3[] tempVertices = new Vector3[9];

		private Vector3[] tempNormals = new Vector3[9];

		private Vector4[] tempTangents = new Vector4[9];
	}
}

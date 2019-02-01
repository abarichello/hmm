using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	internal class TeamMarkerVFX : BaseVFX
	{
		private void Awake()
		{
			this.propertyBlock = new MaterialPropertyBlock();
			if (TeamMarkerVFX.planeMesh == null)
			{
				TeamMarkerVFX.planeMesh = new Mesh();
				Vector3[] vertices = new Vector3[]
				{
					new Vector3(-1f, 0f, -1f),
					new Vector3(-1f, 0f, 1f),
					new Vector3(1f, 0f, 1f),
					new Vector3(1f, 0f, -1f)
				};
				int[] indices = new int[]
				{
					0,
					1,
					2,
					0,
					2,
					3
				};
				Vector2[] uv = new Vector2[]
				{
					new Vector2(0f, 0f),
					new Vector2(1f, 0f),
					new Vector2(1f, 1f),
					new Vector2(0f, 1f)
				};
				TeamMarkerVFX.planeMesh.vertices = vertices;
				TeamMarkerVFX.planeMesh.uv = uv;
				TeamMarkerVFX.planeMesh.SetIndices(indices, MeshTopology.Triangles, 0);
			}
			Shader shader = (Shader)Resources.Load("Particle Add Overall", typeof(Shader));
			this.planeMaterial = new Material(shader);
			this.planeMaterial.mainTexture = this.texture;
		}

		private void LateUpdate()
		{
			if (this.visible)
			{
				this.alpha += Time.deltaTime * 5f;
			}
			else
			{
				this.alpha -= Time.deltaTime * 5f;
			}
			if (this.alpha <= 0f)
			{
				this.alpha = 0f;
				return;
			}
			if (this.alpha > 1f)
			{
				this.alpha = 1f;
			}
			Vector3 position = base.transform.position;
			position.y = 0f;
			this.matrix.SetTRS(position, Quaternion.Euler(0f, Time.time * 45f, 0f), new Vector3(this.radius, 1f, this.radius));
			this.propertyBlock.SetColor("_TintColor", this.color * this.alpha);
			Graphics.DrawMesh(TeamMarkerVFX.planeMesh, this.matrix, this.planeMaterial, base.gameObject.layer, null, 0, this.propertyBlock);
		}

		protected override void OnActivate()
		{
			if (this._targetFXInfo.Owner != null)
			{
				switch (this.teamCondition)
				{
				case TeamMarkerVFX.TeamType.Ally:
					if (this._targetFXInfo.Owner)
					{
						TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
						if (team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
						{
							this.alpha = 0f;
							return;
						}
					}
					break;
				case TeamMarkerVFX.TeamType.Enemy:
					if (this._targetFXInfo.Owner)
					{
						TeamKind team2 = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
						if (team2 == GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
						{
							this.alpha = 0f;
							return;
						}
					}
					break;
				case TeamMarkerVFX.TeamType.Blue:
					if (this._targetFXInfo.Owner)
					{
						TeamKind team3 = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
						if (team3 == TeamKind.Blue)
						{
							this.alpha = 0f;
							return;
						}
					}
					break;
				case TeamMarkerVFX.TeamType.Red:
					if (this._targetFXInfo.Owner)
					{
						TeamKind team4 = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
						if (team4 == TeamKind.Red)
						{
							this.alpha = 0f;
							return;
						}
					}
					break;
				}
				this.color = GUIColorsInfo.GetColorByPlayerCarId(this._targetFXInfo.Owner.ObjId.GetInstanceId(), true);
			}
			else
			{
				this.color = Color.white;
			}
			this.propertyBlock.Clear();
			this.propertyBlock.SetColor("_TintColor", this.color);
			this.visible = true;
		}

		protected override void WillDeactivate()
		{
			this.visible = false;
		}

		protected override void OnDeactivate()
		{
			this.visible = false;
		}

		private static Mesh planeMesh;

		private Material planeMaterial;

		public Texture texture;

		private Matrix4x4 matrix = default(Matrix4x4);

		public float radius = 1f;

		private MaterialPropertyBlock propertyBlock;

		public TeamMarkerVFX.TeamType teamCondition = TeamMarkerVFX.TeamType.Ally;

		private float alpha;

		private bool visible;

		private Color color;

		public enum TeamType
		{
			None,
			Ally,
			Enemy,
			Blue,
			Red
		}
	}
}

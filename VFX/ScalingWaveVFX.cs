using System;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ScalingWaveVFX : BaseVFX
	{
		protected static void UpdateMesh(float arcAngle)
		{
			ScalingWaveVFX.WaveMesh = (ScalingWaveVFX.WaveMesh ?? new Mesh());
			ScalingWaveVFX.WaveMesh.name = "ScalingWaveVFX Mesh";
			int num = (int)Mathf.Floor(arcAngle / ScalingWaveVFX.MeshAngleStep);
			int num2 = num << 1;
			Vector3[] array = new Vector3[num2];
			Color[] array2 = new Color[num2];
			Vector2[] array3 = new Vector2[num2];
			int[] array4 = new int[num2 * 3];
			float num3 = arcAngle / (float)num;
			float num4 = (float)num2;
			float num5 = 0f;
			float num6 = -arcAngle * 0.5f;
			int i = 0;
			while (i < num2)
			{
				float num7 = (float)(i & 1);
				Vector3 vector;
				vector..ctor(Mathf.Sin(0.017453292f * num6), 0f, Mathf.Cos(0.017453292f * num6));
				array[i] = vector * (ScalingWaveVFX.MeshThickness * num7) + Vector3.back;
				array2[i] = ((!(i < 2 | i > num2 - 2)) ? Color.white : Color.black);
				array3[i].x = num5 / (num4 - 1f);
				array3[i].y = num7;
				num6 += num3 * num7;
				i++;
				num5 += 1f;
			}
			for (int j = 0; j < num2 - 3; j += 2)
			{
				int num8 = 6 * (j >> 1);
				array4[num8] = j;
				array4[num8 + 1] = j + 1;
				array4[num8 + 2] = j + 2;
				array4[num8 + 3] = j + 1;
				array4[num8 + 4] = j + 3;
				array4[num8 + 5] = j + 2;
			}
			ScalingWaveVFX.WaveMesh.Clear();
			ScalingWaveVFX.WaveMesh.vertices = array;
			ScalingWaveVFX.WaveMesh.colors = array2;
			ScalingWaveVFX.WaveMesh.uv = array3;
			ScalingWaveVFX.WaveMesh.triangles = array4;
		}

		protected void Awake()
		{
			this.isRunning = false;
			this._offsetYPropertyId = Shader.PropertyToID("_OffsetY");
			this._colorPropertyId = Shader.PropertyToID("_Color");
			this.propertyBlock = new MaterialPropertyBlock();
			this.propertyBlock.SetFloat(this._offsetYPropertyId, 0f);
			this.propertyBlock.SetColor(this._colorPropertyId, Color.white);
		}

		protected override void OnActivate()
		{
			this.isRunning = true;
			this.info = (this._targetFXInfo.Gadget as MultipleEffectsAngleCannon);
			float num = this.info.GetRange() + this.info.Radius + this.extraScale;
			float num2 = Mathf.Tan(this.info.Angle / 2f) * num / this.info.Radius;
			this.lifetime = this.info.GetRange() / this.info.CannonInfo.MoveSpeed - 1f / this.fadeSpeed;
			this.startPosition = this._targetFXInfo.Origin;
			if (ScalingWaveVFX.WaveMesh == null)
			{
				ScalingWaveVFX.UpdateMesh(this.info.Angle / num2);
			}
			this.propertyBlock.SetFloat(this._offsetYPropertyId, 0f);
			this.propertyBlock.SetColor(this._colorPropertyId, Color.white);
			base.transform.localPosition = new Vector3(0f, 0f, this.info.Radius + this.extraScale);
			this.CanCollectToCache = false;
			this.WaveMeshFilter.mesh = ScalingWaveVFX.WaveMesh;
			this.offset = 0f;
			this.Update();
		}

		private void Update()
		{
			float num = Vector3.Distance(this.startPosition, base.transform.position);
			base.transform.localScale = new Vector3(num + this.info.Radius + this.extraScale, 1f, num + this.info.Radius + this.extraScale);
			if (!this.isRunning)
			{
				if (this.offset < 1f)
				{
					this.offset += Time.deltaTime * this.fadeSpeed;
				}
				if (this.offset >= 1f)
				{
					this.offset = 1f;
					this.CanCollectToCache = true;
				}
			}
			this.propertyBlock.SetFloat(this._offsetYPropertyId, this.offset);
			this.WaveMeshRenderer.SetPropertyBlock(this.propertyBlock);
			this.lifetime -= Time.deltaTime;
			if (this.lifetime <= 0f)
			{
				this.isRunning = false;
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this.isRunning = false;
			this.offset = 1f;
		}

		private static Mesh WaveMesh;

		private static float MeshAngleStep = 1f;

		private static float MeshThickness = 1f;

		private Vector3 startPosition;

		private MultipleEffectsAngleCannon info;

		public float fadeSpeed = 1f;

		public MeshFilter WaveMeshFilter;

		public MeshRenderer WaveMeshRenderer;

		public float offset;

		public float extraScale;

		private float lifetime;

		private bool isRunning;

		private MaterialPropertyBlock propertyBlock;

		private int _offsetYPropertyId = -1;

		private int _colorPropertyId = -1;
	}
}

using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	public class Billboard : MonoBehaviour
	{
		private void OnRenderObject()
		{
			if (this.filter == null)
			{
				this.filter = base.GetComponent<MeshFilter>();
				if (this.filter == null)
				{
					return;
				}
			}
			if (this.filter.sharedMesh == null)
			{
				return;
			}
			if (!this.material)
			{
				return;
			}
			if (!this.material.SetPass(0))
			{
				return;
			}
			Vector3 normalized = (Camera.current.transform.position - base.transform.position).normalized;
			Billboard.Alignment alignment = this.alignment;
			if (alignment != Billboard.Alignment.Billboard)
			{
				if (alignment == Billboard.Alignment.ForwardBillboard)
				{
					this.matrix.SetTRS(base.transform.position, Quaternion.LookRotation(Vector3.forward, normalized), base.transform.lossyScale);
				}
			}
			else
			{
				this.matrix.SetTRS(base.transform.position, Quaternion.LookRotation(Camera.current.transform.up, -normalized), base.transform.lossyScale);
			}
			Graphics.DrawMeshNow(this.filter.sharedMesh, this.matrix, 0);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawIcon(base.transform.position, "billboard");
		}

		private MeshFilter filter;

		public Material material;

		private Matrix4x4 matrix = default(Matrix4x4);

		public Billboard.Alignment alignment;

		public enum Alignment
		{
			Billboard,
			ForwardBillboard,
			UpwardBillboard
		}
	}
}

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Pipeline
{
	public class ArtReference : MonoBehaviour
	{
		public Material Material
		{
			get
			{
				return this._material;
			}
		}

		public GameObject ModelFbx
		{
			get
			{
				return this._modelFbx;
			}
		}

		public Bounds Bounds
		{
			get
			{
				return this.ModelBounds;
			}
		}

		protected void ComputeModelSize()
		{
			this.ModelBounds = default(Bounds);
			if (this.ChildModel != null)
			{
				Vector3 position = this.ChildModel.transform.position;
				Quaternion rotation = this.ChildModel.transform.rotation;
				this.ChildModel.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				MeshRenderer[] componentsInChildren = this.ChildModel.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					this.ModelBounds.Encapsulate(componentsInChildren[i].bounds);
				}
				if (this.BoundSkinnedMeshes)
				{
					SkinnedMeshRenderer[] componentsInChildren2 = this.ChildModel.GetComponentsInChildren<SkinnedMeshRenderer>();
					for (int j = 0; j < componentsInChildren2.Length; j++)
					{
						this.ModelBounds.Encapsulate(componentsInChildren2[j].bounds);
					}
				}
				this.ChildModel.transform.SetPositionAndRotation(position, rotation);
			}
		}

		private void OnValidate()
		{
			this.ComputeModelSize();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.gray;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(this.ModelBounds.center, this.ModelBounds.size);
		}

		[FormerlySerializedAs("material")]
		[SerializeField]
		private Material _material;

		[FormerlySerializedAs("modelFBX")]
		[SerializeField]
		private GameObject _modelFbx;

		[FormerlySerializedAs("childModel")]
		[SerializeField]
		public GameObject ChildModel;

		[HideInInspector]
		[SerializeField]
		protected Bounds ModelBounds;

		[SerializeField]
		[Tooltip("Should the model bounds consider skinned meshes?")]
		protected bool BoundSkinnedMeshes;
	}
}

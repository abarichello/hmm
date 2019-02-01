using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMSprite : MonoBehaviour
	{
		public int index
		{
			get
			{
				return this._index;
			}
			private set
			{
				this._index = value;
			}
		}

		public HMMPanel parentPanel { get; private set; }

		public Vector3 position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = value;
				if (this.parentPanel == null)
				{
					return;
				}
				this.UpdateSprite();
			}
		}

		public bool visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				bool visible = this._visible;
				this._visible = value;
				if (this._visible != visible)
				{
					this.UpdateSprite();
				}
			}
		}

		public Color color
		{
			get
			{
				return this.spriteMesh.color;
			}
			set
			{
				this.spriteMesh.SetColor(value);
			}
		}

		public void Init(int index, HMMPanel parentPanel)
		{
			this.parentPanel = parentPanel;
			this.index = index;
		}

		public void UpdateSprite()
		{
			this.parentPanel.UpdateSprite(this);
		}

		public HMMPanelMeshController.SpriteMesh spriteMesh;

		public string spriteName;

		public string Spritestyle;

		public bool customSize;

		public int width;

		public int height;

		[SerializeField]
		private Vector3 _position;

		[SerializeField]
		private bool _visible;

		[SerializeField]
		private int _index;
	}
}

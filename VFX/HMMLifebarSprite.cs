using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMLifebarSprite : MonoBehaviour
	{
		public int verticesAmount
		{
			get
			{
				if (this.parentPanel.GetSpriteStyleForName(this.spriteName) == HMMLifebarPanel.SpriteStyle.Bordered)
				{
					return 8;
				}
				return 4;
			}
		}

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

		public HMMLifebarPanel parentPanel { get; private set; }

		public HMMLifebarSpriteContainer parentContainer { get; private set; }

		public void SetParentContainer()
		{
			HMMLifebarSpriteContainer component = base.transform.parent.GetComponent<HMMLifebarSpriteContainer>();
			if (component != null)
			{
				this.parentContainer = component;
			}
		}

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

		public void Init(int index, HMMLifebarPanel parentPanel)
		{
			this.SetParentContainer();
			this.parentPanel = parentPanel;
			this.index = index;
		}

		public void UpdateSprite()
		{
			this.parentPanel.UpdateSprite(this);
		}

		public HMMLifebarPanelMeshController.SpriteMesh spriteMesh;

		public string spriteName;

		public string Spritestyle;

		public int depth;

		public bool customSize;

		public int width;

		public int height;

		public int leftBorder;

		public int rightBorder;

		[SerializeField]
		private Vector3 _position;

		[SerializeField]
		private bool _visible;

		[SerializeField]
		private int _index;
	}
}

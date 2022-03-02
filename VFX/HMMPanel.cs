using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClientAPI.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(HMMPanelMeshController))]
	[ExecuteInEditMode]
	public class HMMPanel : MonoBehaviour
	{
		public bool hasSpriteContainers
		{
			get
			{
				return this.spriteContainers != null && this.spriteContainers.Length > 0;
			}
		}

		public int atlasWidth
		{
			get
			{
				return this._atlasWidth;
			}
		}

		public int atlasHeight
		{
			get
			{
				return this._atlasHeight;
			}
		}

		public bool changed
		{
			get
			{
				return this._changed;
			}
			private set
			{
				this._changed = value;
			}
		}

		public HMMSprite[] sprites
		{
			get
			{
				return this._sprites;
			}
		}

		public HMMSpriteContainer[] spriteContainers
		{
			get
			{
				return this._spriteContainers;
			}
		}

		public Vector4 bounds
		{
			get
			{
				return this._meshController.meshBounds;
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
				if (visible != this._visible)
				{
					if (this.hidePanelByVertices)
					{
						this.SetSpritesVisibility(this._visible);
					}
					else
					{
						this._meshController.GetComponent<Renderer>().enabled = this._visible;
						this._meshController.enabled = this._visible;
					}
				}
			}
		}

		private void Awake()
		{
			this.Init();
		}

		private IEnumerator Start()
		{
			yield return UnityUtils.WaitForEndOfFrame;
			if (!this._meshesCreated)
			{
				this.CreateMeshes();
			}
			yield break;
		}

		public void Init()
		{
			this.ValidateDependencies();
			this.Initialized = true;
			if (Application.isPlaying)
			{
				return;
			}
			this.ParseDataFile();
			this.UpdateSprites();
		}

		public void ParseDataFile()
		{
			this._spriteDataList.Clear();
			Dictionary<string, object> dictionary = Json.ToObject<Dictionary<string, object>>(this.dataFile.text);
			Dictionary<string, object> dictionary2 = dictionary["frames"] as Dictionary<string, object>;
			Dictionary<string, object> dictionary3 = dictionary["meta"] as Dictionary<string, object>;
			Dictionary<string, object> dictionary4 = dictionary3["size"] as Dictionary<string, object>;
			this._atlasWidth = Convert.ToInt32(dictionary4["w"]);
			this._atlasHeight = Convert.ToInt32(dictionary4["h"]);
			foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
			{
				Dictionary<string, object> dictionary5 = keyValuePair.Value as Dictionary<string, object>;
				Dictionary<string, object> dictionary6 = dictionary5["frame"] as Dictionary<string, object>;
				HMMPanel.SpriteData item = new HMMPanel.SpriteData
				{
					name = keyValuePair.Key,
					width = Convert.ToInt32(dictionary6["w"]),
					height = Convert.ToInt32(dictionary6["h"]),
					x = Convert.ToInt32(dictionary6["x"]),
					y = Convert.ToInt32(dictionary6["y"]),
					rotated = Convert.ToBoolean(dictionary5["rotated"])
				};
				this._spriteDataList.Add(item);
			}
		}

		public void CreateMeshes()
		{
			this._meshesCreated = true;
			this.ValidateDependencies();
			this._meshController.CreateMeshes();
		}

		public HMMPanel.SpriteData GetSpriteForName(string name)
		{
			this.ValidateSpriteDataMap();
			if (this._spriteDataMap.ContainsKey(name))
			{
				return this._spriteDataMap[name];
			}
			HMMPanel.SpriteData spriteData;
			return (!this._spriteDataMap.TryGetValue(name, out spriteData)) ? null : spriteData;
		}

		public HMMPanel.SpriteStyle GetSpriteStyleForName(string name)
		{
			return (HMMPanel.SpriteStyle)Enum.Parse(typeof(HMMPanel.SpriteStyle), name);
		}

		private void LateUpdate()
		{
			if (this.changed)
			{
				this.UpdateVertices();
				this.changed = false;
			}
		}

		public void UpdateSprite(HMMSprite pHmmSprite)
		{
			this._meshController.UpdateSprite(pHmmSprite);
			this.changed = true;
		}

		public void UpdateSprites()
		{
			for (int i = 0; i < this.sprites.Length; i++)
			{
				HMMSprite pHmmSprite = this.sprites[i];
				this.UpdateSprite(pHmmSprite);
			}
		}

		public void UpdateVertices()
		{
			this._meshController.UpdateVertices();
		}

		public void RecalculateBounds()
		{
			this._meshController.RecalculateBounds();
		}

		public string[] GetSpriteNames()
		{
			if (this._spriteDataMap == null || this._spriteDataMap.Count == 0)
			{
				this.ValidateSpriteDataMap();
			}
			return this._spriteDataMap.Keys.ToArray<string>();
		}

		public string[] GetSpriteStyleNames()
		{
			return Enum.GetNames(typeof(HMMPanel.SpriteStyle));
		}

		private void SetSpritesVisibility(bool pVisible)
		{
			for (int i = 0; i < this._sprites.Length; i++)
			{
				HMMSprite hmmsprite = this._sprites[i];
				hmmsprite.visible = pVisible;
			}
		}

		private void ValidateDependencies()
		{
			this.ValidateSpriteDataMap();
			this.ValidateChildrenSpriteContainers();
			this.ValidateChildrenSprites();
			this.ValidateMeshController();
		}

		private void ValidateSpriteDataMap()
		{
			if (this._spriteDataMap != null)
			{
				return;
			}
			this._spriteDataMap = new Dictionary<string, HMMPanel.SpriteData>();
			for (int i = 0; i < this._spriteDataList.Count; i++)
			{
				HMMPanel.SpriteData spriteData = this._spriteDataList[i];
				this._spriteDataMap[spriteData.name] = spriteData;
			}
		}

		private void ValidateMeshController()
		{
			if (this._meshController == null)
			{
				this._meshController = base.GetComponent<HMMPanelMeshController>();
			}
		}

		private void ValidateChildrenSprites()
		{
			this._sprites = base.GetComponentsInChildren<HMMSprite>();
			for (int i = 0; i < this._sprites.Length; i++)
			{
				HMMSprite hmmsprite = this._sprites[i];
				hmmsprite.Init(i, this);
			}
		}

		private void ValidateChildrenSpriteContainers()
		{
			this._spriteContainers = base.GetComponentsInChildren<HMMSpriteContainer>();
			for (int i = 0; i < this._spriteContainers.Length; i++)
			{
				HMMSpriteContainer hmmspriteContainer = this._spriteContainers[i];
				hmmspriteContainer.Init(i, this);
			}
		}

		public TextAsset dataFile;

		public bool hidePanelByVertices;

		[SerializeField]
		private int _atlasWidth;

		[SerializeField]
		private int _atlasHeight;

		public Dictionary<string, HMMPanel.SpriteData> _spriteDataMap;

		private bool _changed;

		private HMMPanelMeshController _meshController;

		[SerializeField]
		private List<HMMPanel.SpriteData> _spriteDataList = new List<HMMPanel.SpriteData>();

		[SerializeField]
		private HMMSprite[] _sprites;

		[SerializeField]
		private HMMSpriteContainer[] _spriteContainers;

		[SerializeField]
		private bool _visible;

		[HideInInspector]
		[SerializeField]
		private bool _meshesCreated;

		public bool Initialized;

		[Serializable]
		public class SpriteData
		{
			public string name;

			public int width;

			public int height;

			public int x;

			public int y;

			public bool rotated;
		}

		[Serializable]
		public enum SpriteStyle
		{
			Normal,
			Tiled,
			Filled
		}
	}
}

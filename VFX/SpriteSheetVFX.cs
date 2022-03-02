using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(Renderer))]
	public class SpriteSheetVFX : BaseVFX
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				Object.Destroy(this);
				return;
			}
			this._mainTexId = Shader.PropertyToID("_MainTex");
		}

		private void Update()
		{
			int num = this._columns * this._lines;
			if (Time.timeSinceLevelLoad - this._lastFrame > this._frameTime)
			{
				this._index = ((!this._loop) ? Math.Min(this._index + 1, num) : ((this._index + 1) % num));
				this._lastFrame = Time.timeSinceLevelLoad;
			}
			int num2 = this._index % this._columns;
			int num3 = (this._index + this._columns) / this._columns;
			this._renderer.material.SetTextureOffset(this._mainTexId, new Vector2((float)num2 * this._size.x, 1f - (float)num3 * this._size.y));
		}

		public override int Priority
		{
			get
			{
				return 1000;
			}
		}

		protected override void OnActivate()
		{
			this._index = 1;
			this._lastFrame = Time.timeSinceLevelLoad;
			this._renderer.material.SetTextureScale(this._mainTexId, this._size);
			this._renderer.material.SetTextureOffset(this._mainTexId, new Vector2(this._size.x, 1f - this._size.y));
			base.enabled = true;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			base.enabled = false;
		}

		private void OnValidate()
		{
			this._size = new Vector2(1f / (float)this._columns, 1f / (float)this._lines);
			this._frameTime = this._duration / (float)(this._columns * this._lines);
			this._renderer = base.GetComponent<Renderer>();
		}

		[SerializeField]
		private bool _loop;

		[SerializeField]
		[Tooltip("Duration in seconds")]
		private float _duration = 10f;

		[SerializeField]
		[Header("Dimensions")]
		private int _columns = 1;

		[SerializeField]
		private int _lines = 1;

		[SerializeField]
		[HideInInspector]
		private Renderer _renderer;

		[SerializeField]
		[HideInInspector]
		private Vector2 _size;

		[SerializeField]
		[HideInInspector]
		private float _frameTime;

		[NonSerialized]
		private int _index;

		[NonSerialized]
		private float _lastFrame;

		[NonSerialized]
		private int _mainTexId;
	}
}

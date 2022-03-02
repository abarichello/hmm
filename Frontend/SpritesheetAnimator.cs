using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SpritesheetAnimator
	{
		public SpritesheetAnimator(ITextureMappingUpdater provider)
		{
			this._provider = provider;
		}

		public bool IsPlaying
		{
			get
			{
				return this._playing;
			}
		}

		public void Initialize(int columns, int lines, float duration)
		{
			this._columns = columns;
			this._lines = lines;
			this._total = this._columns * this._lines;
			this._uvRect = new Rect(0f, 0f, 1f / (float)this._columns, 1f / (float)this._lines);
			this._frameTime = duration / (float)(this._columns * this._lines);
			this.UpdateUvRect();
		}

		public void Update()
		{
			if (!this._playing)
			{
				return;
			}
			if (Time.timeSinceLevelLoad - this._lastFrame > this._frameTime)
			{
				this._index = (this._index + 1) % this._total;
				this._lastFrame = Time.timeSinceLevelLoad;
			}
			this.UpdateUvRect();
		}

		public void UpdateUvRect()
		{
			int num = this._index % this._columns;
			int num2 = (this._index + this._columns) / this._columns;
			this._uvRect.x = (float)num * this._uvRect.width;
			this._uvRect.y = 1f - (float)num2 * this._uvRect.height;
			this._provider.UpdateUv(this._uvRect);
		}

		public void Stop()
		{
			this._playing = false;
			this._index = 0;
			this.UpdateUvRect();
		}

		public void StartAnimation()
		{
			this._playing = true;
			this._index = 0;
			this._lastFrame = Time.timeSinceLevelLoad;
			this._provider.UpdateUv(this._uvRect);
		}

		private bool _playing;

		private float _frameTime;

		private ITextureMappingUpdater _provider;

		private int _columns = 1;

		private int _lines = 1;

		private int _total = 1;

		private int _index;

		private float _lastFrame;

		private Rect _uvRect;
	}
}

using System;
using Hoplon.Metrics.Api;
using Pocketverse;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;

namespace HeavyMetalMachines.Memory
{
	public class GCController : MonoBehaviour
	{
		public void Start()
		{
			this._customGCControl = this._configLoader.GetBoolValue(ConfigAccess.CustomGCControl);
			this._maxAllocatedSize = this._configLoader.GetLongValue(ConfigAccess.MaxGCAllocatedSize);
			GarbageCollector.OnCollect += this.GarbageCollectorOnOnCollect;
			this._performanceReader = Platform.Current.CreatePerformanceReader();
		}

		public void OnDestroy()
		{
			GarbageCollector.OnCollect -= this.GarbageCollectorOnOnCollect;
		}

		private void GarbageCollectorOnOnCollect()
		{
			this._lastCollection = Time.realtimeSinceStartup;
		}

		public void Update()
		{
			if (!this._customGCControl)
			{
				return;
			}
			if (Time.realtimeSinceStartup - this._lastCollection <= (float)this._gcTestInterval)
			{
				return;
			}
			long monoUsedSizeLong = Profiler.GetMonoUsedSizeLong();
			if (monoUsedSizeLong <= this._maxAllocatedSize)
			{
				return;
			}
			long num = monoUsedSizeLong / 1024L / 1024L;
			long num2 = this._maxAllocatedSize / 1024L / 1024L;
			GarbageCollector.Collect(string.Format("Memory has reached the threshold of {0}mb. Used={1}mb", num2, num));
			this._lastCollection = Time.realtimeSinceStartup;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GCController));

		private Camera Camera;

		private float _lastCollection;

		private int _gcTestInterval = 30;

		private long _maxAllocatedSize = 1073741824L;

		private bool _customGCControl;

		private bool _plotDebug;

		private IPerformanceReader _performanceReader;

		[Inject]
		private IConfigLoader _configLoader;
	}
}

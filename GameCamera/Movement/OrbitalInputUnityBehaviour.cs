using System;
using Pocketverse.MuralContext;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public class OrbitalInputUnityBehaviour : BaseMonoBehaviour, ICleanupListener
	{
		[Inject]
		private void Init(OrbitalInput input)
		{
			this._input = input;
			base.enabled = true;
		}

		private void OnEnable()
		{
			if (this._input == null)
			{
				base.enabled = false;
			}
		}

		public void LateUpdate()
		{
			this._input.LateUpdate();
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._input != null)
			{
				this._input.Reset();
			}
		}

		private OrbitalInput _input;
	}
}

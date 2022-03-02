using System;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.GameCamera.Behaviour;
using HeavyMetalMachines.GameCamera.Movement;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using HeavyMetalMachines.PostProcessing;
using NativePlugins;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Injection
{
	public class CameraInstaller : ClientMonoInstaller<CameraInstaller>
	{
		protected override void Bind()
		{
			if (this._config.GetBoolValue(ConfigAccess.HORTA) && !this._config.GetBoolValue(ConfigAccess.DisableHORTACam))
			{
				this.BindNewHortaCamera();
			}
			else if (!this._config.GetBoolValue(ConfigAccess.DisableCamV2))
			{
				this.BindNewGameCamera();
			}
			else
			{
				this.BindCarCameraAdapters();
			}
			base.Container.QueueForInject(UnityInterface.Instance);
		}

		private void BindNewHortaCamera()
		{
			this.BindBasicElements();
			this.BindParameters();
			this.BindMovements();
			base.Container.Bind<IBombScoreCameraBehaviour>().To<BombScoreCameraBehaviour>().AsSingle();
			base.Container.Bind<ISpectatorCameraBehaviour>().To<SpectatorCameraBehaviour>().AsSingle();
			base.Container.Bind<IPlayerDeadBehaviour>().To<NoPlayerDeadBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<BombScoreCameraBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<SpectatorCameraBehaviour>().AsSingle();
			GameObject gameObject = this.InstantiateAndBindCamera();
			gameObject.name = "HORTACamera";
		}

		private void BindNewGameCamera()
		{
			this.BindBasicElements();
			this.BindParameters();
			this.BindMovements();
			base.Container.Bind<IBombScoreCameraBehaviour>().To<BombScoreCameraBehaviour>().AsSingle();
			base.Container.Bind<ISpectatorCameraBehaviour>().To<SpectatorCameraBehaviour>().AsSingle();
			base.Container.Bind<IPlayerDeadBehaviour>().To<PlayerDeadBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<BombScoreCameraBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<SpectatorCameraBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<PlayerDeadBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<BombDisputeBehaviour>().AsSingle();
			base.Container.Bind<IGameCameraBehaviour>().To<PlayerCameraBehaviour>().AsSingle();
			GameObject gameObject = this.InstantiateAndBindCamera();
			gameObject.name = "PlayerCamera";
		}

		private GameObject InstantiateAndBindCamera()
		{
			GameObject gameObject = base.Container.InstantiatePrefab(this.HORTACam);
			gameObject.transform.parent = null;
			HORTACamera component = gameObject.GetComponent<HORTACamera>();
			HORTACameraEngine component2 = gameObject.GetComponent<HORTACameraEngine>();
			MainPostProcessing component3 = gameObject.GetComponent<MainPostProcessing>();
			base.Container.Bind<HORTACamera>().FromInstance(component);
			base.Container.Bind<IGameCameraEngine>().FromInstance(component2);
			base.Container.Bind<IListenerPositionUpdater>().To<ListenerPositionUpdaterByGameCamera>().AsTransient();
			base.Container.Bind<IGamePostProcessing>().FromInstance(component3);
			base.Container.Bind<IGameCamera>().To<HORTAGameCamera>().AsSingle();
			return gameObject;
		}

		private void BindBasicElements()
		{
			base.Container.Bind<IGameCameraInversion>().FromInstance(this.CameraInversion);
			base.Container.Bind<IGameCameraPlayerSetup>().To<GameCameraPlayerSetup>().AsSingle();
		}

		private void BindParameters()
		{
			base.Container.Bind<SkyViewParameters>().FromInstance(this.SkyViewParams);
			base.Container.Bind<OrbitalParameters>().FromInstance(this.OrbitalParams);
			base.Container.Bind<FixedCameraParameters>().FromInstance(this.FixedCameraParams);
			base.Container.Bind<SpectatorCameraBehaviourParameters>().FromInstance(this.SpectatorParams);
			base.Container.QueueForInject(this.OrbitalParams);
			base.Container.QueueForInject(this.SkyViewParams);
		}

		private void BindMovements()
		{
			base.Container.Bind<SkyViewMovement>().AsSingle();
			base.Container.Bind<OrbitalMovement>().AsSingle();
			base.Container.Bind<FixedCameraMovement>().AsSingle();
			base.Container.Bind<IOrbitalInput>().To<OrbitalInput>().AsSingle();
		}

		private void BindCarCameraAdapters()
		{
			base.Container.Bind<IGameCameraInversion>().To<CarCameraAdapter>().AsSingle();
			base.Container.Bind<IGameCameraEngine>().To<CarCameraAdapter>().AsSingle();
			base.Container.Bind<IListenerPositionUpdater>().To<ListenerPositionUpdaterByGameCamera>().AsTransient();
			base.Container.Bind<IGameCamera>().To<CarCameraAdapter>().AsSingle();
			base.Container.Bind<IGameCameraPlayerSetup>().To<CarCameraAdapter>().AsSingle();
			base.Container.Bind<IOrbitalInput>().To<NoOrbitalInput>().AsSingle();
			base.Container.Bind<IBombScoreCameraBehaviour>().To<BombScoreCameraBehaviourAdapter>().AsSingle();
			base.Container.Bind<ISpectatorCameraBehaviour>().To<SpectatorCameraBehaviourAdapter>().AsSingle();
			base.Container.Bind<IPlayerDeadBehaviour>().To<NoPlayerDeadBehaviour>().AsSingle();
			GameObject gameObject = base.Container.InstantiatePrefab(this.PlayerCam);
			gameObject.transform.parent = null;
			gameObject.name = "PlayerCamera";
			base.Container.Bind<IGamePostProcessing>().FromInstance(gameObject.GetComponent<MainPostProcessing>());
		}

		[Inject]
		private IConfigLoader _config;

		public GameCameraInversion CameraInversion;

		public SkyViewParameters SkyViewParams;

		public OrbitalParameters OrbitalParams;

		public FixedCameraParameters FixedCameraParams;

		public SpectatorCameraBehaviourParameters SpectatorParams;

		public GameObject HORTACam;

		public GameObject PlayerCam;
	}
}

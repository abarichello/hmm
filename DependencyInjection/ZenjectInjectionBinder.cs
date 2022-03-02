using System;
using Hoplon.Assertions;
using Hoplon.DependencyInjection;
using Zenject;

namespace HeavyMetalMachines.DependencyInjection
{
	public class ZenjectInjectionBinder : IInjectionBinder
	{
		public ZenjectInjectionBinder(DiContainer container)
		{
			Assert.IsNotNull<DiContainer>(container, "Cannot create ZenjectInjectionBinder with null container.");
			this._container = container;
		}

		public void BindTransient<TImplementation>()
		{
			this._container.Bind<TImplementation>().AsTransient();
		}

		public void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface
		{
			this._container.Bind<TInterface>().To<TImplementation>().AsTransient();
		}

		public void ConditionalBindTransient<TInterface, TImplementation>(object conditional) where TImplementation : TInterface
		{
			this._container.Bind<TInterface>().WithId(conditional).To<TImplementation>().AsTransient();
		}

		public void BindSingle<TImplementation>()
		{
			this._container.Bind<TImplementation>().AsSingle();
		}

		public void BindSingle<TInterface, TImplementation>() where TImplementation : TInterface
		{
			this._container.Bind<TInterface>().To<TImplementation>().AsSingle();
		}

		public void BindSingletonInstance<TInstance>(TInstance instance)
		{
			this._container.BindInstance<TInstance>(instance);
		}

		public void BindSingleWithInterfaces<TImplementation>()
		{
			this._container.BindInterfacesAndSelfTo<TImplementation>().AsSingle();
		}

		public void BindFromMethod<T>(Func<T> method)
		{
			this._container.Bind<T>().FromMethod((InjectContext _) => method());
		}

		private readonly DiContainer _container;
	}
}

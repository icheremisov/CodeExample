using System;
using Zenject;

namespace Client.Core.Common.DI
{
    public class ZenjectContainerBuilder : IContainerBuilder
    {
        public ZenjectContainer Container { get; }
        private DiContainer RawContainer => Container.RawContainer;

        public ZenjectContainerBuilder(ZenjectContainer diContainer) => Container = diContainer;

        public void RegisterSingle(Type type, object instance) =>
            RawContainer.Bind(type).FromInstance(instance);


        public void RegisterAllInterfacesSingle(object instance) =>
            RawContainer.BindInterfacesTo(instance.GetType())
                .FromInstance(instance)
                .AsSingle();

        public void RegisterSingle<TService>() => RawContainer.Bind<TService>().AsSingle();

        public void RegisterSingle<TImplementation, TService>()
            where TImplementation : TService where TService : notnull =>
            RawContainer.Bind<TService>().To<TImplementation>().AsSingle();

        public void RegisterInstance<TService>(TService instance) where TService : class => RawContainer.BindInstance(instance);

        public void RegisterAllInterfacesSingle<TService>() => RawContainer.BindInterfacesTo<TService>().AsSingle();

        public void RegisterAllInterfacesSingle(Type type) => RawContainer.BindInterfacesTo(type).AsSingle();

        public void RegisterAllInterfacesCached<TService>() => RawContainer.BindInterfacesTo<TService>().AsCached();

        public IContainer Build() => Container;
    }
}
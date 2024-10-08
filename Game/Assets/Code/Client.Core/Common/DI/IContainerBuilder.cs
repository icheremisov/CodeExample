using System;

namespace Client.Core.Common.DI {

	public interface IContainerBuilder
	{
		void RegisterSingle(Type type, object instance);
		void RegisterAllInterfacesSingle(object instance);
		
		void RegisterSingle<TService>();
		void RegisterSingle<TImplementation, TService>() where TImplementation : TService where TService : notnull;
		void RegisterInstance<TService>(TService instance) where TService : class;
		
		void RegisterAllInterfacesSingle<TService>();
		void RegisterAllInterfacesSingle(Type type);

		void RegisterAllInterfacesCached<TService>();
		
		IContainer Build();
	}
}
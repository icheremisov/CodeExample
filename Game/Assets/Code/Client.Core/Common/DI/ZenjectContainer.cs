using System;
using System.Collections.Generic;
using Zenject;

namespace Client.Core.Common.DI {

	public class ZenjectContainer : IContainer {
		
		public DiContainer RawContainer { get; }

		public ZenjectContainer() => RawContainer = new DiContainer();

		public ZenjectContainer(DiContainer container) => RawContainer = container;

		public TService Resolve<TService>() where TService : class => RawContainer.Resolve<TService>();

		public IEnumerable<TService> ResolveAll<TService>() => RawContainer.ResolveAll<TService>();

		public TService Instantiate<TService>(params object[] args) => RawContainer.Instantiate<TService>(args);

		public object Instantiate(Type type, params object[] args) => RawContainer.Instantiate(type, args);
	}

}
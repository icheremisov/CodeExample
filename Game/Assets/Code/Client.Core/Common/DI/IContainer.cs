using System;
using System.Collections.Generic;

namespace Client.Core.Common.DI
{
    public interface IContainer
    {
        TService Resolve<TService>() where TService : class;

        IEnumerable<TService> ResolveAll<TService>();

        TService Instantiate<TService>(params object[] args);

        object Instantiate(Type type, params object[] args);
    }
}
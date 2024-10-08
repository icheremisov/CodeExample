using System;
using XLib.Core.Utils;
using Zenject;

namespace XLib.Unity.Installers {

	public abstract partial class ProjectContextInstaller<TDerived> : BaseInstaller<TDerived>
		where TDerived : ProjectContextInstaller<TDerived> {

		public override void InstallBindings() {

			if (GetType() != TypeOf<TDerived>.Raw) throw new Exception($"{GetType().FullName}: invalid generic base param: must be <{GetType().Name}> but actual is <{TypeOf<TDerived>.Name}>!");

			if (!GetComponent<ProjectContext>()) throw new Exception($"{this.GetType().Name} can be only used on {nameof(ProjectContext)} container!");
			base.InstallBindings();
		}
		
	}

}

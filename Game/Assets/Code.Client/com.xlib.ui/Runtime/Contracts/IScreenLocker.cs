using System;
using UnityEngine;
using XLib.UI.Types;

namespace XLib.UI.Contracts {

	public interface IScreenLocker {

		bool IsLocked { get; }
		event Action LockAreaClick;

		event Action LockChanged;

		void LockScreen(ScreenLockTag tag);
		void UnlockScreen(ScreenLockTag tag);

		void AddUnlocker(ScreenLockTag tag);
		void RemoveUnlocker(ScreenLockTag tag);

		IDisposable LockScope(ScreenLockTag tag);
		
		/// <summary>
		///     lock screen and focus on target
		/// </summary>
		void LockScreen(ScreenLockTag tag, Component target);

		void UnlockAll();

	}

}
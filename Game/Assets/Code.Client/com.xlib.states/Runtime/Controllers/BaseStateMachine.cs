using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Core.Utils;
using XLib.States.Contracts;
using XLib.States.Exceptions;
using XLib.States.Internal;

namespace XLib.States.Controllers {

	/// <summary>
	///     base class for state machine.
	/// </summary>
	/// <typeparam name="TState">type or interface for game state type</typeparam>
	public abstract class BaseStateMachine<TState> : IStateMachine<TState> {
		public delegate void StateChangedEvent(TState from, TState to, object payload);

		private readonly IStateFactory<TState> _stateFactory;

		protected BaseStateMachine(IStateFactory<TState> stateFactory) {
			_stateFactory = stateFactory;
		}

		public event StateChangedEvent<TState> OnBeforeStateChanged;
		public event StateChangedEvent<TState> OnAfterStateChanged;

		public bool ChangingState { get; private set; }
		public Type CurrentStateType { get; private set; }
		public TState CurrentState { get; private set; }

		public bool IsActive<T>() where T : class, TState => CurrentStateType == TypeOf<T>.Raw;

		/// <summary>
		///     enter specified state
		/// </summary>
		public async UniTask EnterAsync(Type stateType, CancellationToken ct = default) {
			RichLog.Log($"[{TypeOf<TState>.Name:green}] Change state {CurrentStateType?.Name ?? "n/a"} -> {stateType.Name}");

			await UniTask.DelayFrame(1, cancellationToken: ct);
			await EnterInternalAsync(stateType, null, CallEnterAsync, ct);
		}

		public async UniTask EnterAsync<T>(CancellationToken ct) where T : class, TState, IEnterState {
			RichLog.Log($"[{TypeOf<TState>.Name:green}] Change state {CurrentStateType?.Name ?? "n/a"} -> {TypeOf<T>.Name}");

			await UniTask.DelayFrame(1, cancellationToken: ct);
			await EnterInternalAsync(TypeOf<T>.Raw, null, CallEnterAsync, ct);
		}

		public async UniTask EnterAsync<T, TPayload>(TPayload payload, CancellationToken ct) where T : class, TState, IPayloadedState<TPayload> {
			RichLog.Log($"[{TypeOf<TState>.Name:green}] Change state {CurrentStateType?.Name ?? "n/a"} -> {TypeOf<T>.Name}");

			await UniTask.DelayFrame(1, cancellationToken: ct);

			UniTask ExecuteAsync(TState state, CancellationToken ctt) => CallEnterAsync(state, TypeOf<TPayload>.Raw, payload, ctt);

			await EnterInternalAsync(TypeOf<T>.Raw, payload, ExecuteAsync, ct);
		}

		private async UniTask EnterInternalAsync(Type stateType, object payload, Func<TState, CancellationToken, UniTask> callEnter, CancellationToken ct) {
			if (this.IsActive(stateType)) return;

			var prevChangingState = ChangingState;

			var oldState = CurrentState;
			var newState = GetStateInstance(stateType);

			try {
				ChangingState = true;

				OnBeforeStateChanged?.Invoke(oldState, newState, payload);

				if (oldState is IExitState exitState) await exitState.OnExitAsync(ct);

				CurrentState = newState;
				CurrentStateType = newState.GetType();

				await callEnter(CurrentState, ct);
			}
			finally {
				ChangingState = prevChangingState;
			}

			OnAfterStateChanged?.Invoke(oldState, newState, payload);
		}

		private static async UniTask CallEnterAsync(TState newState, CancellationToken ct) {
			if (newState is IEnterState enterState) await enterState.OnEnterAsync(ct);
		}

		private static async UniTask CallEnterAsync(TState newState, Type payloadType, object payload, CancellationToken ct) {
			var stateGenericType = StateHelpers.PayloadedStateType.MakeGenericType(payloadType);

			if (!stateGenericType.IsInstanceOfType(newState))
				throw new StateException($"State {newState.GetType().FullName} mut be instance of IPayloadedState<{payloadType.Name}>!");

			var mEnterMethod = stateGenericType.GetMethod("OnEnterAsync");
			if (mEnterMethod == null) throw new StateException($"State {newState.GetType().FullName}: cannot find OnEnter method!");

			var task = (UniTask)mEnterMethod.Invoke(newState, new[] { payload, ct });
			await task;
		}

		private TState GetStateInstance(Type stateType) {
			if (stateType == null) return default;

			var result = _stateFactory.Create(stateType);

			return result != null ? result : throw new StateException($"Cannot find game state {stateType.FullName}!");
		}
	}

}
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.States.Contracts {

	public delegate void StateChangedEvent<in TState>(TState from, TState to, object payload);

	public interface IStateMachine<TState> {

		/// <summary>
		///     true if state is changing right now
		/// </summary>
		bool ChangingState { get; }

		/// <summary>
		///     get current active state type
		/// </summary>
		Type CurrentStateType { get; }

		/// <summary>
		///     get current state instance
		/// </summary>
		TState CurrentState { get; }

		/// <summary>
		///     event called before state changed
		/// </summary>
		event StateChangedEvent<TState> OnBeforeStateChanged;

		/// <summary>
		///     event called right after state changed
		/// </summary>
		event StateChangedEvent<TState> OnAfterStateChanged;

		/// <summary>
		///     check if specific state is active
		/// </summary>
		/// <typeparam name="T">state type to check</typeparam>
		/// <returns>true if state is active</returns>
		bool IsActive<T>() where T : class, TState;

		/// <summary>
		///     enter specified state
		/// </summary>
		UniTask EnterAsync(Type stateType, CancellationToken ct = default);

		/// <summary>
		///     enter specified state
		/// </summary>
		UniTask EnterAsync<T>(CancellationToken ct = default) where T : class, TState, IEnterState;

		/// <summary>
		///     enter specified state and pass argument
		/// </summary>
		UniTask EnterAsync<T, TPayload>(TPayload payload, CancellationToken ct = default) where T : class, TState, IPayloadedState<TPayload>;

	}

	public static class StateMachineExtensions {

		/// <summary>
		///     check if specific state is active
		/// </summary>
		/// <returns>true if state is active</returns>
		public static bool IsActive<TState>(this IStateMachine<TState> stateMachine, Type stateType) {
			var currentState = stateMachine.CurrentStateType;
			return currentState != null && stateType.IsAssignableFrom(currentState);
		}

	}

}
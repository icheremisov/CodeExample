using System;
using System.Collections;
using UnityEngine;

// ReSharper disable once CheckNamespace
public static class CoroutineExtensions {

	/// <summary>
	///     Start a coroutine that might throw an exception. Call the callback with the exception if it
	///     does or null if it finishes without throwing an exception.
	/// </summary>
	/// <param name="monoBehaviour">MonoBehaviour to start the coroutine on</param>
	/// <param name="enumerator">Iterator function to run as the coroutine</param>
	/// <param name="onError">
	///     Callback to call when the coroutine has thrown an exception.
	///     The thrown exception or null is passed as the parameter.
	/// </param>
	/// <returns>The started coroutine</returns>
	public static Coroutine StartThrowingCoroutine(
		this MonoBehaviour monoBehaviour,
		IEnumerator enumerator,
		Action<Exception> onError = null,
		Action onComplete = null
	) =>
		monoBehaviour.StartCoroutine(RunThrowingIterator(enumerator, onError, onComplete));

	/// <summary>
	///     Run an iterator function that might throw an exception. Call the callback with the exception
	///     if it does or null if it finishes without throwing an exception.
	/// </summary>
	/// <param name="enumerator">Iterator function to run</param>
	/// <param name="error">
	///     Callback to call when the iterator has thrown an exception or finished.
	///     The thrown exception or null is passed as the parameter.
	/// </param>
	/// <returns>An enumerator that runs the given enumerator</returns>
	public static IEnumerator RunThrowingIterator(
		IEnumerator enumerator,
		Action<Exception> error,
		Action onComplete
	) {
		while (true) {
			object current;
			try {
				if (enumerator.MoveNext() == false) break;

				current = enumerator.Current;
			}
			catch (Exception ex) {
				if (error != null)
					error(ex);
				else
					Debug.LogException(ex);
				yield break;
			}

			yield return current;
		}

		onComplete?.Invoke();
	}

}
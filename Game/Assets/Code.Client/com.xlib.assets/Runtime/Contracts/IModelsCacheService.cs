using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XLib.Assets.Cache;

namespace XLib.Assets.Contracts {

	/// <summary>
	///     preload configs into memory for faster instant access
	/// </summary>
	public interface IModelsCacheService {

		/// <summary>
		///     initialize cache once
		/// </summary>
		UniTask ApplyAsync(ModelsCacheOptions options);

		IEnumerable<TModel> GetList<TModel>(Func<TModel, bool> selector = null) where TModel : class;
		TModel GetSingle<TModel>() where TModel : class;
		TModel GetByKey<TModel>(object key) where TModel : class;

	}

}
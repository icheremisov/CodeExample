using System;
using System.Collections.Generic;
using Client.Levels.Contracts;
using Entitas;
using UnityEngine;

namespace Client.Levels.Internal {

	public class LevelStateController : ILevelStateController, IDisposable {
		private readonly LevelContext _levelContext;
		private readonly Dictionary<ClientLevelState, ILevelState> _levelStates = new();

		private ILevelState _currentBattleState;

		public LevelStateController(LevelContext levelContext, IEnumerable<ILevelState> levelStates) {
			_levelContext = levelContext;
			foreach (var battleState in levelStates) _levelStates.Add(battleState.State, battleState);
			
			var battleStateGroup = _levelContext.GetGroup(LevelMatcher.LevelState);
			battleStateGroup.OnEntityAdded += OnBattleStateAdded;
			battleStateGroup.OnEntityUpdated += OnBattleStateUpdated;
		}

		public void Dispose() {
			var battleStateGroup = _levelContext.GetGroup(LevelMatcher.LevelState);
			battleStateGroup.OnEntityAdded -= OnBattleStateAdded;
			battleStateGroup.OnEntityUpdated -= OnBattleStateUpdated;
		}
		
		private void OnBattleStateAdded(IGroup<LevelEntity> group, LevelEntity entity, int index, IComponent component) {
			UpdateState(~entity.levelState);
		}
		
		private void OnBattleStateUpdated(IGroup<LevelEntity> group, LevelEntity entity, int index, IComponent previouscomponent, IComponent newcomponent) {
			UpdateState(~entity.levelState);
		}

		private void UpdateState(ClientLevelState newState) {
			if (_currentBattleState?.State == newState) return;
			
			_currentBattleState?.OnExit();

			if (!_levelStates.TryGetValue(newState, out _currentBattleState)) {
				Debug.LogError($"Cannot find battle state for type: {newState}");
				return;
			}
			
			_currentBattleState?.OnEnter();
		}
	}

}
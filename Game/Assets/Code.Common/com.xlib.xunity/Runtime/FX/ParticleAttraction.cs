using Coffee.UIExtensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace XLib.Unity.FX {

	public class ParticleAttraction : MonoBehaviour {
		private static readonly ParticleSystem.Particle[] Particles = new ParticleSystem.Particle[1000];

		[SerializeField, RequiredIn(PrefabKind.InstanceInScene)] private Transform _target;
		
		private enum LifetimeMode {
			Global,
			PerParticle,
		}

		private enum DistanceMode {
			Constant,
			RelativeToTarget,
		}

		[Header("Power")]
		[SerializeField, Range(0.1f, 20)] private float _attractionPower = 1;
		[SerializeField, Range(0.0f, 16), ShowIf(nameof(_timeMode), LifetimeMode.PerParticle)] private float _particleOrderPower = 1;
		
		[Header("Time")]
		[SerializeField] private LifetimeMode _timeMode = LifetimeMode.Global;
		[SerializeField] private float _attractionDelaySec = 0.5f;
		
		[Header("Distance")]
		[SerializeField] private DistanceMode _distanceMode = DistanceMode.RelativeToTarget;
		[SerializeField, ShowIf(nameof(_distanceMode), DistanceMode.Constant)] private float _maxAttractionDistance = 2000;
		[SerializeField, Range(0.1f, 10), ShowIf(nameof(_distanceMode), DistanceMode.RelativeToTarget)] private float _distanceScale = 1;
		[FormerlySerializedAs("_curve"),SerializeField] private AnimationCurve _powerOverDistance;

		[Space]
		[SerializeField, Required] private ParticleSystem[] _systems;

		private float _delayTimer = -1;
		private float _fxTimer;
		private bool _played;

		private ParticleSystem[] Systems => _systems ??= GetComponentsInChildren<ParticleSystem>(true);

		private void LateUpdate() {
			if (!_played || !_target) return;

			if (_delayTimer > 0) {
				_delayTimer -= Time.deltaTime;
				if (_delayTimer > 0) return;

				_delayTimer = -1;
				_fxTimer = 0;
			}

			var targetLocalPos = transform.InverseTransformPoint(_target.transform.TransformPoint(Vector3.zero));
			var targetGlobalPos = _target.position;
			var particleOrderPower = _particleOrderPower * 0.01f;
			var attractionPower = 1.0f / (_attractionPower * 0.1f);

			foreach (var system in Systems) {
				var count = system.GetParticles(Particles);
				var power = Mathf.Pow(_fxTimer, attractionPower);

				var maxDistance = _distanceMode == DistanceMode.Constant ? _maxAttractionDistance : (targetGlobalPos - system.transform.position).magnitude * _distanceScale;
				maxDistance = Mathf.Max(0.1f, maxDistance);

				var targetPos = targetLocalPos;
				var uiParticle = system.GetComponent<UIParticle>();
				if (uiParticle && uiParticle.scale > 0) targetPos /= uiParticle.scale;

				for (var i = 0; i < count; i++) {
					var particle = Particles[i];
					
					var dir = targetPos - particle.position;
					var distance = dir.magnitude;
					if (distance < 0.15f) {
						particle.remainingLifetime = -1;
						Particles[i] = particle;
						continue;
					}

					if (_timeMode == LifetimeMode.PerParticle) power = Mathf.Pow(Mathf.Max(0, particle.startLifetime - particle.remainingLifetime + i * particleOrderPower), attractionPower);
					dir.Normalize();

					var force = _powerOverDistance.Evaluate((maxDistance - distance) / maxDistance) * power;
					

					particle.position = Vector3.Lerp(particle.position, targetPos, Mathf.Clamp01(Time.deltaTime * force));
					Particles[i] = particle;
				}

				system.SetParticles(Particles, count);
			}

			_fxTimer += Time.unscaledDeltaTime;
		}

		private void OnDisable() {
			Systems.ForEach(x => x.Stop());
			_delayTimer = -1;
			_played = false;
		}

		[Button]
		public void Play() {
			if (!Application.isPlaying) return;
			if (!_target) return;

			Systems.ForEach(x => {
				x.Stop();
				x.Clear();
			});

			Systems.ForEach(x => x.Play());
			_delayTimer = _attractionDelaySec;
			_fxTimer = 0;
			_played = true;
		}

		public void Play(Vector3 targetPos) {
			if (!Application.isPlaying) return;
			if (!_target) return;

			_target.position = targetPos;
			Play();
		}
	}

}
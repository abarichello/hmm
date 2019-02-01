using System;
using System.Collections;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class PickupAnimation : GameHubBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
			this.initialPosition = base.transform.position;
			this.initialScale = base.transform.localScale;
		}

		private Vector3 Evaluate(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			t = Mathf.Pow(t, 0.5f);
			float d = 1f - t;
			return a * d + b * d * t + c * (t * t);
		}

		private void OnEnable()
		{
			if (!this.playing)
			{
				base.enabled = false;
			}
		}

		private void Update()
		{
			if (!this.playing)
			{
				return;
			}
			float num = Mathf.Pow(this.t / this.animationTime, 3f);
			Vector3 position = this.Evaluate(this.initialPosition, this.target.position + new Vector3(0f, this.height, 0f), this.target.position, num);
			position.y += Mathf.Sin(num * 3.14159274f);
			base.transform.position = position;
			base.transform.localScale = this.initialScale * Mathf.Lerp(1f, this.finalScale, this.t);
			this.t += Time.deltaTime;
			if (this.t > this.animationTime)
			{
				base.enabled = false;
				this.playing = false;
				base.transform.position = this.initialPosition;
				base.transform.localScale = this.initialScale;
				foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>())
				{
					renderer.enabled = false;
				}
				foreach (Collider collider in base.GetComponentsInChildren<Collider>())
				{
					collider.enabled = false;
				}
				foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
				{
					particleSystem.Stop();
				}
			}
		}

		public void PlayAnimation(Transform target)
		{
			if (target == null)
			{
				return;
			}
			this.t = 0f;
			this.target = target;
			base.transform.localScale = this.initialScale;
			this.playing = true;
			base.enabled = true;
		}

		public void PlayAnimation(Transform target, bool destroy)
		{
			this.PlayAnimation(target);
			if (destroy)
			{
				new PickupAnimation.Timer(this, this.animationTime, new PickupAnimation.Timer.OnTimeReached(this.OnDestroyTime));
			}
		}

		private void OnDestroyTime()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public float animationTime = 1f;

		public Transform target;

		public float height = 1f;

		private Vector3 initialPosition;

		private float t;

		private bool playing;

		public float finalScale = 1f;

		private Vector3 initialScale;

		[Serializable]
		public class Timer
		{
			public Timer(MonoBehaviour parent, float timeSeconds, PickupAnimation.Timer.OnTimeReached callback)
			{
				this.Parent = parent;
				this.Set(timeSeconds, callback);
			}

			public Timer(MonoBehaviour parent, float timeSeconds)
			{
				this.Parent = parent;
				this.Set(timeSeconds);
			}

			public Timer(MonoBehaviour parent)
			{
				this.Parent = parent;
			}

			public float TimeRemaining
			{
				get
				{
					if (this.isDecrescent)
					{
						return (float)(this.targetIterations - this.iterations) * Time.fixedDeltaTime;
					}
					return (float)this.iterations * Time.fixedDeltaTime;
				}
			}

			public void Pause()
			{
				this.localIsPaused = true;
			}

			public void Resume()
			{
				this.localIsPaused = false;
			}

			public void PauseOrResume()
			{
				this.localIsPaused = !this.localIsPaused;
			}

			public void Set(float timeSeconds, PickupAnimation.Timer.OnTimeReached callback)
			{
				this.isDecrescent = true;
				this.targetIterations = (int)Mathf.Ceil(timeSeconds / Time.fixedDeltaTime);
				this.iterations = 0;
				this.callback = callback;
				if (this.timerCoroutine == null)
				{
					this.timerCoroutine = this.Parent.StartCoroutine(this.CheckTimeRemaing());
				}
			}

			public void Set(float timeSeconds)
			{
				this.isDecrescent = true;
				this.targetIterations = (int)Mathf.Ceil(timeSeconds / Time.fixedDeltaTime);
				this.iterations = 0;
				this.callback = null;
				if (this.timerCoroutine == null)
				{
					this.timerCoroutine = this.Parent.StartCoroutine(this.CheckTimeRemaing());
				}
			}

			public void SetCrescent()
			{
				this.isDecrescent = false;
				this.targetIterations = 0;
				this.iterations = 0;
				this.callback = null;
				if (this.timerCoroutine == null)
				{
					this.timerCoroutine = this.Parent.StartCoroutine(this.CheckTimeRemaing());
				}
			}

			public void Cancel()
			{
				this.callback = null;
			}

			private IEnumerator CheckTimeRemaing()
			{
				while (!this.isDecrescent || this.TimeRemaining > 0f)
				{
					yield return UnityUtils.WaitForEndOfFrame;
					this.iterations++;
				}
				this.timerCoroutine = null;
				if (this.callback != null)
				{
					this.callback();
				}
				yield break;
			}

			public MonoBehaviour Parent;

			public static bool globalIsPaused;

			private bool isDecrescent;

			public bool localIsPaused;

			private int iterations;

			private int targetIterations;

			private Coroutine timerCoroutine;

			private PickupAnimation.Timer.OnTimeReached callback;

			public delegate void OnTimeReached();

			public delegate void OnTimeReachedInt();
		}
	}
}

using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class MustangHorsesVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			this.CanCollectToCache = false;
			if (this.instances == null && this.horses != null)
			{
				this.instances = new MustangHorsesVFX.HorseInstance[this.horses.Length];
				for (int i = 0; i < this.instances.Length; i++)
				{
					this.instances[i] = new MustangHorsesVFX.HorseInstance(this.horses[i], this, i);
				}
			}
			if (this.instances != null)
			{
				for (int j = 0; j < this.instances.Length; j++)
				{
					this.instances[j].OnActivate(this);
				}
				this._active = true;
			}
		}

		private void Update()
		{
			if (!this._active || this.instances == null)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < this.instances.Length; i++)
			{
				this.instances[i].Update(this);
				flag |= this.instances[i].IsAlive();
			}
			if (!flag)
			{
				this.CanCollectToCache = true;
				this._active = false;
			}
		}

		protected override void WillDeactivate()
		{
			this.OnDeactivate();
		}

		protected override void OnDeactivate()
		{
			if (this.instances != null)
			{
				for (int i = 0; i < this.instances.Length; i++)
				{
					this.instances[i].OnDeactivate();
				}
			}
		}

		public GameObject[] horses;

		public float horseHorizontalSpacing = 2f;

		public float horseVerticalSpacing = 2f;

		private MustangHorsesVFX.HorseInstance[] instances;

		private bool _active;

		public float maxAnimationVariation = 0.2f;

		public float maxSpawnTimeVariation = 0.5f;

		private class HorseInstance
		{
			public HorseInstance(GameObject go, MustangHorsesVFX parent, int index)
			{
				this.transform = go.transform;
				this.renderer = go.GetComponentInChildren<SkinnedMeshRenderer>();
				this.animation = go.GetComponentInChildren<Animation>();
				float num = Mathf.Floor((float)index) - Mathf.Floor((float)parent.horses.Length / 2f);
				float num2 = -Mathf.Abs(num);
				this.transform.localPosition = new Vector3(num * parent.horseHorizontalSpacing, 0f, num2 * parent.horseVerticalSpacing);
			}

			public void OnActivate(MustangHorsesVFX parent)
			{
				this.delay = Random.Range(0f, parent.maxSpawnTimeVariation);
				this.animation.Stop();
				Color color = this.renderer.material.color;
				color.a = 0f;
				this.renderer.material.color = color;
				this.transform.gameObject.SetActive(true);
				this.active = true;
				this.timeToDie = 0.3f;
			}

			public void Update(MustangHorsesVFX parent)
			{
				if (!this.IsAlive())
				{
					return;
				}
				if (this.animation.isPlaying)
				{
					AnimationState animationState = this.animation["run"];
					animationState.speed = this.speed;
					float a = Mathf.Lerp(0.5f, 1f, Mathf.Sin(animationState.normalizedTime * 3.1415927f));
					Color color = this.renderer.material.color;
					color.a = a;
					this.renderer.material.color = color;
				}
				else
				{
					this.delay -= Time.deltaTime;
					if (this.delay <= 0f)
					{
						this.animation.Play();
						this.speed = Random.Range(1f - parent.maxAnimationVariation, 1f + parent.maxAnimationVariation);
					}
					Color color2 = this.renderer.material.color;
					color2.a = 0f;
					this.renderer.material.color = color2;
				}
				if (!this.active && this.timeToDie > 0f)
				{
					this.timeToDie -= Time.deltaTime;
					Color color3 = this.renderer.material.color;
					color3.a = this.timeToDie;
					this.renderer.material.color = color3;
					if (this.timeToDie <= 0f)
					{
						this.OnDeath();
					}
				}
			}

			public bool IsAlive()
			{
				return this.timeToDie > 0f;
			}

			private void OnDeath()
			{
				this.transform.gameObject.SetActive(false);
				this.animation.Stop();
			}

			public void OnDeactivate()
			{
				this.active = false;
			}

			private SkinnedMeshRenderer renderer;

			private Transform transform;

			private Animation animation;

			private float speed = 1f;

			private float delay;

			private bool active;

			private float timeToDie;
		}
	}
}

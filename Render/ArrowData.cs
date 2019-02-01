using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class ArrowData : MonoBehaviour
	{
		public void OnEnable()
		{
			if (this.ParticleSystem)
			{
				this.ParticleSystem.Play(true);
			}
			if (this.billboard)
			{
				this.billboard.enabled = true;
			}
			this.isDead = false;
			this.ActiveObjects.SetActive(true);
			this.Explosion.GetComponent<ParticleSystem>().Stop(true);
			this.Explosion.GetComponent<ParticleSystem>().Clear();
			this.Explosion.SetActive(false);
			this.Miss.GetComponent<ParticleSystem>().Stop(true);
			this.Miss.SetActive(false);
			this.Miss.GetComponent<ParticleSystem>().Clear();
		}

		public void Die(bool withTarget)
		{
			this.isDead = true;
			if (this.ParticleSystem)
			{
				this.ParticleSystem.Stop(true);
				this.ParticleSystem.Clear(true);
			}
			if (this.billboard)
			{
				this.billboard.enabled = false;
			}
			if (withTarget)
			{
				this.Explosion.SetActive(true);
				this.Explosion.GetComponent<ParticleSystem>().Play(true);
			}
			else
			{
				this.Miss.SetActive(true);
				this.Miss.GetComponent<ParticleSystem>().Play(true);
			}
		}

		public ArrowCurveInfo curveInfo;

		public float startTime;

		public float homingTime;

		public ParticleSystem ParticleSystem;

		public GameObject Explosion;

		public GameObject Miss;

		public GameObject ActiveObjects;

		public bool isDead;

		public float LifeTime = 1f;

		public Billboard billboard;
	}
}

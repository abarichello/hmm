using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	internal class ScrapAnimator : GameHubBehaviour
	{
		private void Awake()
		{
			this.particles = new ParticleSystem.Particle[50];
		}

		private Vector3 Evaluate(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			t = Mathf.Pow(t, 0.5f);
			float d = 1f - t;
			return a * d + b * d * t + c * (t * t);
		}

		private void LateUpdate()
		{
			int size = this.particlesSystem.GetParticles(this.particles);
			for (int i = this.scraps.Count - 1; i >= 0; i--)
			{
				ScrapAnimator.ScrapData scrapData = this.scraps[i];
				float num = Mathf.Pow(scrapData.lifeTime / this.animationTime, 3f);
				Vector3 vector = this.Evaluate(scrapData.startPosition, scrapData.targetPosition + new Vector3(0f, this.height, 0f), scrapData.targetPosition, num);
				vector.y += Mathf.Sin(num * 3.14159274f);
				scrapData.startRotation *= Quaternion.Euler(Time.deltaTime * this.rotationSpeed * new Vector3(1f, 1f, 0f));
				Vector3 s = scrapData.startScale * Mathf.Lerp(1f, this.finalScale, scrapData.lifeTime);
				scrapData.lifeTime += Time.deltaTime;
				this.particles[i].position = vector;
				this.particles[i].remainingLifetime = 1f - scrapData.lifeTime / this.animationTime;
				Graphics.DrawMesh(this.ScrapMesh, Matrix4x4.TRS(vector, scrapData.startRotation, s), (scrapData.scrapType != ScrapAnimator.ScrapType.Scenery) ? this.dropScrap : this.sceneryScrap, scrapData.gameObjectlayer);
				if (scrapData.lifeTime > this.animationTime)
				{
					this.scraps.RemoveAt(i);
				}
			}
			this.particlesSystem.SetParticles(this.particles, size);
		}

		public static void AddScrap(Vector3 position, Quaternion rotation, Vector3 lossyScale, Vector3 targetPosition, Quaternion targetRotation, ScrapAnimator.ScrapType scrapType, int gameObjectlayer)
		{
		}

		public float animationTime = 1f;

		public float height = 1f;

		public float finalScale = 1f;

		public Mesh ScrapMesh;

		public Material sceneryScrap;

		public Material dropScrap;

		private List<ScrapAnimator.ScrapData> scraps = new List<ScrapAnimator.ScrapData>();

		public float rotationSpeed = 200f;

		public ParticleSystem particlesSystem;

		private ParticleSystem.Particle[] particles;

		private const int maxParticles = 50;

		public enum ScrapType
		{
			Drop,
			Scenery
		}

		public class ScrapData
		{
			public Vector3 startPosition = Vector3.zero;

			public Quaternion startRotation;

			public Vector3 targetPosition = Vector3.zero;

			public Quaternion targetRotation = Quaternion.identity;

			public int gameObjectlayer;

			public Vector3 startScale = Vector3.zero;

			public Vector3 rotationAxis = Vector3.zero;

			public ScrapAnimator.ScrapType scrapType;

			public float lifeTime;
		}
	}
}

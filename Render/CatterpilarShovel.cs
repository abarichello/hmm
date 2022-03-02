using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CatterpilarShovel : MonoBehaviour
	{
		private void Awake()
		{
			this.generator = base.GetComponent<CarGenerator>();
			this.Configure(this.generator);
		}

		private void Configure(CarGenerator generator)
		{
			this.shovelGO = (generator.bodyGO.transform.Find("shovel") ?? generator.bodyGO.transform.Find("shovel"));
			this.throwerDummy = generator.GetDummyByName("Thrower");
			this.throwerDummy.transform.parent = this.shovelGO.transform;
			this.angle = 0f;
			this.initialized = true;
		}

		public void LateUpdate()
		{
			if (!this.initialized)
			{
				return;
			}
			if (this.FXWorking)
			{
				return;
			}
			if (Physics.Raycast(this.throwerDummy.transform.position + Vector3.up * 10f, -Vector3.up, ref this.hitInfo, 20f, this.collisionMask))
			{
				this.hitPos = this.hitInfo.point;
				this.throwerPos = this.throwerDummy.transform.position;
				this.hit = true;
				this.distancyfToground = this.throwerPos.y - this.hitPos.y;
				if (this.distancyfToground < this.minDistancyToTheGround)
				{
					this.angle = -Vector3.Angle(this.hitPos, this.hitPos + Vector3.up * this.minDistancyToTheGround) * this.multiplicator;
				}
				else if (this.distancyfToground > this.maxDistancyToTheGround)
				{
					this.angle += Time.deltaTime * 20f;
					if (this.angle > 0f)
					{
						this.angle = 0f;
					}
				}
			}
			this.shovelGO.localRotation = Quaternion.Euler(this.angle, 0f, 0f);
		}

		private void OnDrawGizmos()
		{
			if (!this.initialized)
			{
				return;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawLine(this.throwerPos + Vector3.up * 10f, this.throwerPos - Vector3.up * 20f);
			Gizmos.DrawSphere(this.throwerPos, 0.5f);
			if (this.distancyfToground < this.minDistancyToTheGround)
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = Color.blue;
			}
			if (this.hit)
			{
				Gizmos.DrawCube(this.hitPos, new Vector3(0.5f, 0.5f, 0.5f));
			}
		}

		public LayerMask collisionMask;

		private Transform shovelGO;

		private GameObject throwerDummy;

		public float angle;

		private bool initialized;

		private CarGenerator generator;

		public bool FXWorking;

		private RaycastHit hitInfo;

		private bool hit;

		private float distancyfToground;

		public float minDistancyToTheGround = 0.25f;

		public float maxDistancyToTheGround = 0.5f;

		public float multiplicator = 20f;

		private Vector3 hitPos;

		private Vector3 throwerPos;
	}
}

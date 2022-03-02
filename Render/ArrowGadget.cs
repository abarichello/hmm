using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class ArrowGadget : VisualGadget
	{
		private void Start()
		{
			GameObject gameObject = new GameObject("MyCache");
			this.myCacheTransform = gameObject.transform;
			this.myCacheTransform.localPosition = Vector3.zero;
			this.myCacheTransform.localRotation = Quaternion.identity;
			this.myCacheTransform.parent = base.transform;
			gameObject.SetActive(false);
			for (int i = 0; i < this.PrecacheCount; i++)
			{
				ArrowData arrowData = Object.Instantiate<ArrowData>(this.ArrowPrefab);
				this.myCache.Push(arrowData);
				arrowData.transform.parent = this.myCacheTransform;
			}
		}

		private void Update()
		{
			if (this.debugFire)
			{
				this.debugFire = false;
				this.Fire(0.2f);
			}
			LinkedListNode<ArrowData> linkedListNode = this.currentMissiles.First;
			while (linkedListNode != null)
			{
				ArrowData value = linkedListNode.Value;
				if (Time.time - value.startTime >= value.LifeTime)
				{
					linkedListNode = linkedListNode.Next;
					this.currentMissiles.RemoveFirst();
					this.myCache.Push(value);
					value.transform.parent = this.myCacheTransform;
				}
				else if (value.isDead)
				{
					linkedListNode = linkedListNode.Next;
				}
				else
				{
					float num = (Time.time - value.startTime) / value.homingTime;
					num = Mathf.Clamp(num, 0f, 1f);
					if (num == 1f)
					{
						if (!value.isDead)
						{
							if (GameHubBehaviour.Hub)
							{
							}
							value.Die(value.curveInfo.Target != null);
						}
						linkedListNode = linkedListNode.Next;
					}
					else
					{
						value.transform.position = this.Evaluate(num, value.curveInfo, Vector3.zero);
						if (this.derivative != Vector3.up)
						{
							value.transform.rotation = Quaternion.LookRotation(this.derivative, Vector3.up);
						}
						linkedListNode = linkedListNode.Next;
					}
				}
			}
		}

		public override void Fire(float homingTime)
		{
			this.currentSlot = (this.currentSlot + 1) % this.Slots.Length;
			Transform transform = this.Slots[this.currentSlot];
			ArrowData arrowData;
			if (this.myCache.Count > 0)
			{
				arrowData = this.myCache.Pop();
				arrowData.transform.position = transform.position;
				arrowData.transform.rotation = Quaternion.LookRotation(transform.forward);
				arrowData.transform.parent = null;
			}
			else
			{
				arrowData = Object.Instantiate<ArrowData>(this.ArrowPrefab, transform.position, Quaternion.LookRotation(transform.forward));
			}
			arrowData.homingTime = homingTime;
			arrowData.startTime = Time.time;
			Vector3 vector = ((!this.Target) ? this.RelativePosition : this.Target.TransformPoint(this.RelativePosition)) - transform.position;
			float magnitude = vector.magnitude;
			Vector3 vector2 = vector / magnitude;
			arrowData.curveInfo = new ArrowCurveInfo();
			arrowData.curveInfo.Source = transform;
			arrowData.curveInfo.InitialPosition = transform.position;
			arrowData.curveInfo.SourceTangent = vector2 + new Vector3(0f, this.arcHeight, 0f);
			arrowData.curveInfo.Target = this.Target;
			arrowData.curveInfo.TargetTangent = -vector2 + new Vector3(0f, this.arcHeight, 0f);
			arrowData.curveInfo.RelativePosition = this.RelativePosition + Random.insideUnitSphere * this.maxTargetVariationOffset;
			arrowData.curveInfo.lastEvaluation = arrowData.curveInfo.InitialPosition;
			this.currentMissiles.AddLast(arrowData);
		}

		public Vector3 Evaluate(float time, ArrowCurveInfo curveInfo, Vector3 offset)
		{
			Vector3 vector = (!this.AttachedToSource || !curveInfo.Source) ? curveInfo.InitialPosition : curveInfo.Source.position;
			Vector3 vector2 = (!curveInfo.Target) ? curveInfo.RelativePosition : curveInfo.Target.TransformPoint(curveInfo.RelativePosition);
			if (time == 0f)
			{
				curveInfo.lastEvaluation = vector;
				this.derivative = Vector3.Normalize(vector2 - vector);
				return vector;
			}
			Vector3 vector3 = Vector3.Lerp(vector, vector2, time);
			Vector3 vector4 = Vector3.Lerp(vector3, vector3 + Vector3.up * this.arcHeight, Mathf.Sin(time * 3.1415927f));
			this.derivative = Vector3.Normalize(vector4 - curveInfo.lastEvaluation);
			curveInfo.lastEvaluation = vector4;
			return vector4;
		}

		private void OnDrawGizmos()
		{
			foreach (ArrowData arrowData in this.currentMissiles)
			{
				Vector3 vector = this.Evaluate(0f, arrowData.curveInfo, Vector3.zero);
				for (int i = 1; i < 50; i++)
				{
					float time = (float)i / 50f;
					Vector3 vector2 = this.Evaluate(time, arrowData.curveInfo, Vector3.zero);
					Gizmos.DrawLine(vector, vector2);
					vector = vector2;
				}
			}
		}

		private Vector3 derivative;

		public ArrowData ArrowPrefab;

		public int PrecacheCount;

		public bool AttachedToSource;

		private Stack<ArrowData> myCache = new Stack<ArrowData>(2);

		private LinkedList<ArrowData> currentMissiles = new LinkedList<ArrowData>();

		private int currentSlot;

		public bool debugFire;

		private Transform myCacheTransform;

		public float arcHeight;

		public float maxTargetVariationOffset = 1f;

		[AudioDrawer]
		public string ShotAudio;

		[AudioDrawer]
		public string HitAudio;

		[AudioDrawer]
		public string MissAudio;

		public AudioEventAsset tmpShotAudio;

		public AudioEventAsset tmpHitAudio;

		public AudioEventAsset tmpMissAudio;
	}
}

using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class SpringJumper : SceneryVisualGadget
	{
		private void Awake()
		{
			this._animation = base.GetComponent<Animation>();
		}

		private void Update()
		{
			if (this.currentTime > 0f && GameHubBehaviour.Hub.Net.IsClient())
			{
				if (this._animation)
				{
					this._animation.clip.SampleAnimation(base.gameObject, 1f - this.currentTime / this.curveInfo.Duration);
				}
				this.currentTime -= Time.deltaTime;
				if (this.currentTime < 0f)
				{
					this.currentTime = 0f;
				}
				return;
			}
			if (this.currentJumpers.Count == 0)
			{
				return;
			}
			for (int i = 0; i < this.currentJumpers.Count; i++)
			{
				SpringJumper.JumpingData value = this.currentJumpers[i];
				float num = value.CurrentTime / this.curveInfo.Duration;
				value.targetCar.transform.position = this.Evaluate(num, this.curveInfo, value.Offset);
				if (this.curveInfo.useDirection && this.derivative != Vector3.zero)
				{
					Quaternion quaternion = Quaternion.LookRotation(this.derivative, Vector3.up);
					if (value.CurrentTime < 0.1f)
					{
						value.targetCar.rotation = Quaternion.Lerp(value.StartRotation, quaternion, Mathf.Clamp01(value.CurrentTime / 0.2f));
					}
					else if (this.curveInfo.Duration - value.CurrentTime < 0.1f)
					{
						value.targetCar.rotation = Quaternion.Lerp(quaternion, this.curveInfo.Target.rotation, Mathf.Clamp01(this.curveInfo.Duration - value.CurrentTime) / 0.2f);
					}
					else
					{
						value.targetCar.rotation = quaternion;
					}
				}
				else
				{
					value.targetCar.rotation = Quaternion.Lerp(value.StartRotation, this.curveInfo.Target.rotation, num);
				}
				value.CurrentTime += Time.deltaTime;
				this.currentJumpers[i] = value;
				if (value.CurrentTime > this.curveInfo.Duration)
				{
					this.currentJumpers.RemoveAt(i);
				}
			}
		}

		public Vector3 Evaluate(float time, CurveInfo curveInfo, Vector3 offset)
		{
			time = curveInfo.timeCurve.Evaluate(time);
			int num = Mathf.FloorToInt(Mathf.Clamp01(time) * (float)(curveInfo.Steps.Length + 1));
			float time2 = time;
			if (time < 1f)
			{
				time = time * (float)(curveInfo.Steps.Length + 1) - (float)num;
			}
			if (num > curveInfo.Steps.Length)
			{
				num = curveInfo.Steps.Length;
			}
			Vector3 vector;
			Vector3 vector2;
			if (num == curveInfo.Steps.Length)
			{
				vector = curveInfo.Target.position;
				vector2 = vector + curveInfo.TargetTangent;
				if (curveInfo.lockedCurve)
				{
					vector = curveInfo.Source.InverseTransformPoint(vector) + curveInfo.Source.position;
					vector2 = curveInfo.Source.InverseTransformPoint(vector2) + curveInfo.Source.position;
				}
			}
			else
			{
				vector = curveInfo.Source.position + curveInfo.Steps[num].Center;
				vector2 = vector - curveInfo.Steps[num].Tangent;
			}
			Vector3 vector3;
			Vector3 vector4;
			if (num == 0)
			{
				vector3 = curveInfo.Source.position + offset;
				vector4 = vector3 + curveInfo.SourceTangent;
			}
			else
			{
				vector3 = curveInfo.Source.position + curveInfo.Steps[num - 1].Center;
				vector4 = vector3 + curveInfo.Steps[num - 1].Tangent;
			}
			float num2 = 1f - time;
			float num3 = num2 * num2;
			float d = num2 * num2 * num2;
			Vector3 vector5 = d * vector3 + 3f * time * num3 * vector4 + 3f * (time * time) * num2 * vector2 + time * time * time * vector;
			if (curveInfo.useDirection)
			{
				this.derivative = 3f * num3 * (vector4 - vector3) + 6f * num2 * time * (vector2 - vector4) + 3f * (time * time) * (vector - vector2);
				Vector3 a = curveInfo.Target.position - curveInfo.Source.position;
				a.y = 0f;
				a.Normalize();
				this.derivative.Normalize();
				this.derivative = Vector3.Lerp(a, this.derivative, curveInfo.followDirectionCurve.Evaluate(time2));
			}
			else
			{
				this.derivative = curveInfo.Target.position - curveInfo.Source.position;
				this.derivative.y = 0f;
				this.derivative.Normalize();
			}
			if (curveInfo.lockedCurve)
			{
				this.derivative = curveInfo.Source.TransformDirection(this.derivative);
			}
			return (!curveInfo.lockedCurve) ? vector5 : curveInfo.Source.TransformPoint(vector5 - curveInfo.Source.position);
		}

		public override void PlayVfxClient(Transform target)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.currentTime = this.curveInfo.Duration;
				return;
			}
			this.currentJumpers.Add(new SpringJumper.JumpingData(target.rotation, target, target.position - this.curveInfo.Source.position));
		}

		public void DrawCurve(CurveInfo curveInfo)
		{
			if (curveInfo.Source == null || curveInfo.Target == null)
			{
				return;
			}
			Vector3 vector = this.Evaluate(0f, curveInfo, Vector3.zero);
			float a = 0f;
			float num = curveInfo.timeCurve.Evaluate(Time.realtimeSinceStartup % curveInfo.Duration / curveInfo.Duration);
			for (int i = 1; i <= 30; i++)
			{
				float time = (float)i / 30f;
				Vector3 vector2 = this.Evaluate(time, curveInfo, Vector3.zero);
				float magnitude = (vector2 - vector).magnitude;
				a = Mathf.Max(a, magnitude);
				Gizmos.color = Color.Lerp(Color.green, Color.red, magnitude / this.currentCurveMaxVelocity);
				Gizmos.DrawLine(vector, vector2);
				Gizmos.color = Color.Lerp(Color.black, Color.white, curveInfo.followDirectionCurve.Evaluate(time));
				Gizmos.DrawSphere(vector2, 0.1f);
				vector = vector2;
			}
			Gizmos.matrix = Matrix4x4.TRS(this.Evaluate(num, curveInfo, Vector3.zero), Quaternion.LookRotation(this.derivative, Vector3.up), new Vector3(3f, 3f, 3f));
			Gizmos.color = Color.Lerp(Color.green, Color.red, this.derivative.magnitude / this.currentCurveMaxVelocity * (1f - (num - curveInfo.timeCurve.Evaluate(num))));
			Gizmos.DrawCube(new Vector3(0f, 0.5f, 0f), Vector3.one);
			this.currentCurveMaxVelocity = a;
		}

		public void OnDrawGizmos()
		{
			if (this.curveInfo.Source == null || this.curveInfo.Target == null)
			{
				return;
			}
			float num = Time.realtimeSinceStartup % this.curveInfo.Duration / this.curveInfo.Duration;
			if (this.lastTime > num)
			{
				this.baseY = this.curveInfo.Source.position.y;
			}
			else if (this.baseY < this.curveInfo.Source.position.y)
			{
				this.baseY = this.curveInfo.Source.position.y;
			}
			this.DrawCurve(this.curveInfo);
			this.lastTime = num;
			if (this._animation)
			{
				this._animation.clip.SampleAnimation(base.gameObject, Mathf.Clamp01(num * 2f) * base.GetComponent<Animation>().clip.length);
			}
		}

		private Animation _animation;

		public float currentTime;

		private float baseY;

		private float lastTime;

		private List<SpringJumper.JumpingData> currentJumpers = new List<SpringJumper.JumpingData>(3);

		[NonSerialized]
		public Vector3 derivative;

		private float currentCurveMaxVelocity;

		private struct JumpingData
		{
			public JumpingData(Quaternion startRotation, Transform targetCar, Vector3 offset)
			{
				this.Offset = offset;
				this.targetCar = targetCar;
				this.CurrentTime = 0f;
				this.StartRotation = startRotation;
			}

			public Transform targetCar;

			public float CurrentTime;

			public Vector3 Offset;

			public Quaternion StartRotation;
		}
	}
}

using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class InterfacePing : MonoBehaviour
	{
		private void OnEnable()
		{
			this.SourcePing.transform.localScale = Vector3.zero;
			this.DestPing.transform.localScale = Vector3.zero;
			this.HorizontalBar.transform.localScale = new Vector3(0f, this.HorizontalBar.transform.localScale.y, this.HorizontalBar.transform.localScale.z);
			this._destPingDuration = this.DestPing.clip.length;
		}

		public void ExecutePing(Vector3 from, Vector3 to)
		{
			this._from = from;
			this._to = to;
			this._from.z = 0f;
			this._to.z = 0f;
			this._accTime = 0f;
			this._playedLastAnimation = false;
			this.SourcePing.transform.position = from;
			this.DestPing.transform.position = to;
			this.HorizontalBar.transform.localScale = new Vector3(0f, this.HorizontalBar.transform.localScale.y, this.HorizontalBar.transform.localScale.z);
			this.ChangeState(InterfacePing.EInterfacePingState.PingStart);
		}

		private void ChangeState(InterfacePing.EInterfacePingState interfacePingState)
		{
			if (interfacePingState != InterfacePing.EInterfacePingState.PingStart)
			{
				if (interfacePingState != InterfacePing.EInterfacePingState.BarTravel)
				{
					if (interfacePingState == InterfacePing.EInterfacePingState.PingEnd)
					{
						this._nextStateTime = this.TotalDuration;
						this._pingState = InterfacePing.EInterfacePingState.PingEnd;
					}
				}
				else
				{
					this.HorizontalBar.transform.position = this._from;
					Vector3 vector = this._to - this._from;
					float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
					Debug.Log(string.Format("Travel:{0} From:{1} To:{2} Diff:{3} Rot:{4}", new object[]
					{
						this._accTime,
						this._from,
						this._to,
						vector,
						num
					}));
					this.HorizontalBar.transform.rotation = Quaternion.Euler(0f, 0f, num);
					this.UpdateMaxSize(vector);
					this._nextStateTime = this.TotalDuration - this.BarDelay;
					this._pingState = InterfacePing.EInterfacePingState.BarTravel;
				}
			}
			else
			{
				this.HorizontalBar.alpha = 1f;
				this.PlayAnimation(this.SourcePing);
				this._nextStateTime = this.BarDelay;
				this._pingState = InterfacePing.EInterfacePingState.PingStart;
			}
		}

		public void Update()
		{
			if (this._pingState == InterfacePing.EInterfacePingState.None)
			{
				return;
			}
			this._accTime += Time.deltaTime;
			this.UpdateState();
			if (this._accTime > this._nextStateTime)
			{
				if (this._pingState == InterfacePing.EInterfacePingState.PingEnd)
				{
					this.HorizontalBar.alpha = 0f;
					this._pingState = InterfacePing.EInterfacePingState.None;
				}
				else
				{
					this.ChangeState(this._pingState + 1);
				}
			}
		}

		private void UpdateState()
		{
			InterfacePing.EInterfacePingState pingState = this._pingState;
			if (pingState != InterfacePing.EInterfacePingState.PingStart)
			{
				if (pingState != InterfacePing.EInterfacePingState.BarTravel)
				{
					if (pingState == InterfacePing.EInterfacePingState.PingEnd)
					{
						this.CheckLastPingAnimation();
					}
				}
				else
				{
					float num = this.TotalDuration - 2f * this.BarDelay;
					float num2 = Mathf.Clamp01((this._accTime - this.BarDelay) / num);
					float num3 = this._barIncreaseDecreaseCurve.Evaluate(num2);
					float x = num3 * this._maxSize;
					this.HorizontalBar.transform.localScale = new Vector3(x, this.HorizontalBar.transform.localScale.y, this.HorizontalBar.transform.localScale.z);
					this.HorizontalBar.transform.position = Vector3.Lerp(this._from, this._to, num2);
					this.CheckLastPingAnimation();
				}
			}
		}

		private void CheckLastPingAnimation()
		{
			if (!this._playedLastAnimation && this._accTime > this.TotalDuration - this._destPingDuration)
			{
				this._playedLastAnimation = true;
				this.PlayAnimation(this.DestPing);
			}
		}

		private void UpdateMaxSize(Vector3 diff)
		{
			float num = this.RootLocalScale();
			this._maxSize = diff.magnitude / num / 4f;
		}

		public float RootLocalScale()
		{
			return this.HorizontalBar.root.transform.localScale.x;
		}

		private void PlayAnimation(Animation pAnim)
		{
			pAnim.Rewind();
			pAnim.Play();
		}

		[SerializeField]
		private Animation SourcePing;

		[SerializeField]
		private Animation DestPing;

		[SerializeField]
		private UI2DSprite HorizontalBar;

		[SerializeField]
		private float TotalDuration = 1f;

		[Header("Bar will appear after delay and will disapear before total duration")]
		[SerializeField]
		private float BarDelay = 0.3f;

		[SerializeField]
		private AnimationCurve _barIncreaseDecreaseCurve;

		private InterfacePing.EInterfacePingState _pingState;

		private Vector3 _from;

		private Vector3 _to;

		private float _accTime;

		private float _nextStateTime;

		private float _maxSize;

		private bool _playedLastAnimation;

		private float _destPingDuration;

		private enum EInterfacePingState
		{
			None,
			PingStart,
			BarTravel,
			PingEnd
		}
	}
}

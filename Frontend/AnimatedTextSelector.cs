using System;
using System.Collections;
using FMod;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public abstract class AnimatedTextSelector : GameHubBehaviour
	{
		private void Awake()
		{
			this.Initialize();
			this._anim.SetBool("visible", false);
			this._idleHash = Animator.StringToHash("Base.Idle");
			this.UpdateLabels();
			this.unityGambs = false;
		}

		protected abstract void Initialize();

		protected virtual void OnCurrentIndexChanged()
		{
			if (this.transitionSFX != null)
			{
				FMODAudioManager.PlayOneShotAt(this.transitionSFX, Vector3.zero, 0);
			}
		}

		private int GetValidIndex(int index)
		{
			if (index < 0)
			{
				return this.selectionStrings.Length - 1;
			}
			if (index >= this.selectionStrings.Length)
			{
				return 0;
			}
			return index;
		}

		protected void UpdateLabels()
		{
			this.leftLabel.text = Language.Get(this.selectionStrings[this.GetValidIndex(this.currentIndex - 1)], this.translationSheet);
			this.centralLabel.text = Language.Get(this.selectionStrings[this.currentIndex], this.translationSheet);
			this.rightLabel.text = Language.Get(this.selectionStrings[this.GetValidIndex(this.currentIndex + 1)], this.translationSheet);
		}

		public void GoLeft()
		{
			this.TryStartAnimation(-1);
		}

		public void GoRight()
		{
			this.TryStartAnimation(1);
		}

		public void OnIdleEnter()
		{
			this.UpdateLabels();
			this.unityGambs = false;
		}

		public void ChangeVisibility(bool visibility)
		{
			this._anim.SetBool("visible", visibility);
		}

		private void TryStartAnimation(int direction)
		{
			if (this.unityGambs || this._anim.IsInTransition(0) || this._anim.GetCurrentAnimatorStateInfo(0).nameHash != this._idleHash)
			{
				return;
			}
			if (this._currentCoroutine != null)
			{
				base.StopCoroutine(this._currentCoroutine);
			}
			this._currentCoroutine = base.StartCoroutine(this.IEnumeratorGoAnimator(direction));
		}

		private IEnumerator IEnumeratorGoAnimator(int direction)
		{
			this.unityGambs = true;
			this.currentIndex = this.GetValidIndex(this.currentIndex + direction);
			this.OnCurrentIndexChanged();
			this._anim.SetInteger("direction", direction);
			yield return this.wof;
			while (this._anim.IsInTransition(0))
			{
				yield return this.wof;
			}
			this._anim.SetInteger("direction", 0);
			yield break;
		}

		private const string ParamVisible = "visible";

		private const string ParamDirection = "direction";

		private const string StateIdle = "Base.Idle";

		[SerializeField]
		private Animator _anim;

		[SerializeField]
		private UILabel leftLabel;

		[SerializeField]
		private UILabel centralLabel;

		[SerializeField]
		private UILabel rightLabel;

		[SerializeField]
		private TranslationSheets translationSheet;

		[SerializeField]
		private FMODAsset transitionSFX;

		protected string[] selectionStrings;

		protected int currentIndex;

		private int _idleHash = -1;

		private Coroutine _currentCoroutine;

		private bool unityGambs;

		private WaitForEndOfFrame wof = new WaitForEndOfFrame();
	}
}

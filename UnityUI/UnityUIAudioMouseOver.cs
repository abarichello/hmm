using System;
using HeavyMetalMachines.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUIAudioMouseOver : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
	{
		protected void Awake()
		{
			this._selectable = base.GetComponent<Selectable>();
			this._selectableId = ((!(this._selectable == null)) ? this._selectable.GetInstanceID() : -1);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this._selectableId == -1 || this._selectable.IsInteractable())
			{
				this._sfxController.Play(this.asset, base.transform, 1f, UnityUIAudioMouseOver._parameter, 0, this._forceResetTimeline);
			}
		}

		[SerializeField]
		private AudioEventAsset asset;

		[SerializeField]
		private SFXController _sfxController;

		[SerializeField]
		private bool _forceResetTimeline = true;

		private static readonly byte[] _parameter = new byte[0];

		private Selectable _selectable;

		private int _selectableId;
	}
}

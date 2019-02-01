using System;
using System.Collections.Generic;
using HeavyMetalMachines.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUIAudioMouseClick : MonoBehaviour, IPointerUpHandler, IEventSystemHandler
	{
		protected void Awake()
		{
			this._selectable = base.GetComponent<Selectable>();
			this._selectableId = ((!(this._selectable == null)) ? this._selectable.GetInstanceID() : -1);
			this._instanceId = base.gameObject.GetInstanceID();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (this._selectableId != -1 && !this._selectable.IsInteractable())
			{
				return;
			}
			List<GameObject> hovered = eventData.hovered;
			for (int i = 0; i < hovered.Count; i++)
			{
				if (hovered[i].GetInstanceID() == this._instanceId)
				{
					this._sfxController.Play(this.asset, base.transform, 1f, this.parameterBytes, 0, this._forceResetTimeline);
					break;
				}
			}
		}

		[SerializeField]
		private FMODAsset asset;

		[SerializeField]
		private SFXController _sfxController;

		[SerializeField]
		private bool _forceResetTimeline = true;

		private byte[] parameterBytes = new byte[0];

		private Selectable _selectable;

		private int _selectableId;

		private int _instanceId;
	}
}

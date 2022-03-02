using System;
using System.Collections;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public abstract class BaseGUIItem<T, U> : MonoBehaviour where T : BaseGUIItem<T, U>
	{
		public U ReferenceObject
		{
			get
			{
				return this._referenceObject;
			}
		}

		public T CreateNewGuiItem(U referenceObject, bool setActive = true, Transform overrideParent = null)
		{
			Transform parentTransform = overrideParent ?? base.transform.parent;
			T result = this._diContainer.InstantiatePrefabForComponent<T>(this, parentTransform);
			result.gameObject.name = base.gameObject.name;
			result.SetProperties(referenceObject);
			if (setActive)
			{
				result.gameObject.SetActive(true);
			}
			return result;
		}

		public void SetProperties(U referenceObject)
		{
			this._referenceObject = referenceObject;
			this.SetPropertiesTasks(referenceObject);
		}

		protected abstract void SetPropertiesTasks(U referenceObject);

		public void CenterObjectOnParentScrollView()
		{
			Vector3 pos = -this._scrollViewPanel.cachedTransform.InverseTransformPoint(base.transform.position) - new Vector3(this._thisWidget.localSize.x, this._thisWidget.localSize.y) * 0.5f - new Vector3(this._scrollViewPanel.finalClipRegion.z, this._scrollViewPanel.finalClipRegion.w) * 0.5f;
			if (!this._parentScrollView.canMoveHorizontally)
			{
				pos.x = this._scrollViewPanel.cachedTransform.localPosition.x;
			}
			if (!this._parentScrollView.canMoveVertically)
			{
				pos.y = this._scrollViewPanel.cachedTransform.localPosition.y;
			}
			SpringPanel.Begin(this._scrollViewPanel.cachedGameObject, pos, 0f);
		}

		public IEnumerator WaitFrameAndCenterOnThisObject()
		{
			yield return null;
			yield return null;
			this.CenterObjectOnParentScrollView();
			yield break;
		}

		public void SetParentScrollView(UIScrollView parentScrollView)
		{
			if (this._parentScrollView == parentScrollView)
			{
				return;
			}
			this._parentScrollView = parentScrollView;
			for (int i = 0; i < this._uiDragScrollViews.Length; i++)
			{
				this._uiDragScrollViews[i].scrollView = this._parentScrollView;
			}
		}

		[Header("Base UI Item Properties")]
		[SerializeField]
		private UIScrollView _parentScrollView;

		[SerializeField]
		private UIDragScrollView[] _uiDragScrollViews;

		[SerializeField]
		private UIPanel _scrollViewPanel;

		[SerializeField]
		private UIWidget _thisWidget;

		[Space(5f)]
		private U _referenceObject;

		[InjectOnClient]
		private DiContainer _diContainer;
	}
}

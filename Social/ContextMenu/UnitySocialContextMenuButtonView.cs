﻿using System;
using HeavyMetalMachines.Extensions;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.ContextMenu.Presenting;
using Hoplon.Math;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Social.ContextMenu
{
	public class UnitySocialContextMenuButtonView : MonoBehaviour, ISocialContextMenuButtonView
	{
		private Canvas GetCanvas()
		{
			if (this._canvas == null)
			{
				this._canvas = base.GetComponentInParent<Canvas>();
			}
			return this._canvas;
		}

		public Vector2 GetContextMenuPosition()
		{
			Vector2 vector = default(Vector2);
			vector.x = base.transform.TransformPoint(Vector3.zero).x + this._contextMenuOffset.x;
			vector.y = base.transform.TransformPoint(Vector3.down).y + this._contextMenuOffset.y;
			Vector2 v = vector;
			Vector3 vector2 = this.GetCanvas().worldCamera.WorldToViewportPoint(v.ToUnityVector2());
			vector = default(Vector2);
			vector.x = (float)Screen.width * vector2.x;
			vector.y = (float)Screen.height * vector2.y;
			return vector;
		}

		public IButton Button
		{
			get
			{
				return this._button;
			}
		}

		public IObservable<Unit> OnViewDisabled
		{
			get
			{
				return this._viewDisabledSubject;
			}
		}

		public bool OpenInFixedPosition
		{
			get
			{
				return this._openContextMenuInFixedPosition;
			}
		}

		private string ViewId
		{
			get
			{
				return (!string.IsNullOrEmpty(this._viewId)) ? this._viewId : base.gameObject.GetInstanceID().ToString();
			}
		}

		public void Awake()
		{
			this._viewProvider.Bind<ISocialContextMenuButtonView>(this, this.ViewId);
			this._viewId = this.ViewId;
			base.gameObject.SetActive(true);
		}

		public void OnDisable()
		{
			this._viewDisabledSubject.OnNext(Unit.Default);
		}

		public void OnDestroy()
		{
			this._viewProvider.Unbind<ISocialContextMenuButtonView>(this.ViewId);
		}

		public void SetViewId(string viewId)
		{
			this._viewId = viewId;
			this._viewProvider.Bind<ISocialContextMenuButtonView>(this, this._viewId);
		}

		private readonly Subject<Unit> _viewDisabledSubject = new Subject<Unit>();

		[SerializeField]
		private UnityButton _button;

		[SerializeField]
		private string _viewId;

		[SerializeField]
		private bool _openContextMenuInFixedPosition;

		[SerializeField]
		private Vector2 _contextMenuOffset;

		[Inject]
		private IViewProvider _viewProvider;

		private Canvas _canvas;
	}
}

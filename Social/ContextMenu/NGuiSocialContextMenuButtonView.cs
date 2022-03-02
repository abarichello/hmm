using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Social.ContextMenu.Presenting;
using Hoplon.Math;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Social.ContextMenu
{
	public class NGuiSocialContextMenuButtonView : MonoBehaviour, ISocialContextMenuButtonView
	{
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

		public void OnDisable()
		{
			this._viewDisabledSubject.OnNext(Unit.Default);
		}

		public Vector2 GetContextMenuPosition()
		{
			Camera mainCamera = UICamera.mainCamera;
			Vector2 vector = default(Vector2);
			vector.x = base.transform.TransformPoint(Vector3.left).x + this._contextMenuOffset.x;
			vector.y = base.transform.TransformPoint(Vector3.down).y + this._contextMenuOffset.y;
			Vector2 vector2 = vector;
			Vector3 vector3 = mainCamera.WorldToViewportPoint(vector2);
			Vector2 result = default(Vector2);
			result.x = (float)Screen.width * vector3.x;
			result.y = (float)Screen.height * vector3.y;
			return result;
		}

		private readonly Subject<Unit> _viewDisabledSubject = new Subject<Unit>();

		[SerializeField]
		private NGuiButton _button;

		[SerializeField]
		private Vector2 _contextMenuOffset;

		[SerializeField]
		private bool _openContextMenuInFixedPosition;
	}
}

using System;
using Hoplon.Math;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavyMetalMachines.Frontend
{
	public class HmmUiClickHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		public IObservable<Vector2> OnLeftClick
		{
			get
			{
				return this._leftClick;
			}
		}

		public IObservable<Vector2> OnMiddleClick
		{
			get
			{
				return this._middleClick;
			}
		}

		public IObservable<Vector2> OnRightClick
		{
			get
			{
				return this._rightClick;
			}
		}

		public void Awake()
		{
			this._leftClick = new Subject<Vector2>();
			this._middleClick = new Subject<Vector2>();
			this._rightClick = new Subject<Vector2>();
		}

		public void OnDestroy()
		{
			this._rightClick.Dispose();
			this._rightClick = null;
			this._middleClick.Dispose();
			this._middleClick = null;
			this._leftClick.Dispose();
			this._leftClick = null;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == null)
			{
				Subject<Vector2> leftClick = this._leftClick;
				Vector2 vector = default(Vector2);
				vector.x = eventData.position.x;
				vector.y = eventData.position.y;
				leftClick.OnNext(vector);
			}
			else if (eventData.button == 2)
			{
				Subject<Vector2> middleClick = this._middleClick;
				Vector2 vector2 = default(Vector2);
				vector2.x = eventData.position.x;
				vector2.y = eventData.position.y;
				middleClick.OnNext(vector2);
			}
			else if (eventData.button == 1)
			{
				Subject<Vector2> rightClick = this._rightClick;
				Vector2 vector3 = default(Vector2);
				vector3.x = eventData.position.x;
				vector3.y = eventData.position.y;
				rightClick.OnNext(vector3);
			}
		}

		private Subject<Vector2> _rightClick;

		private Subject<Vector2> _leftClick;

		private Subject<Vector2> _middleClick;
	}
}

using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuNewsThumbInfo : MonoBehaviour
	{
		public void Setup(int index, Action<int> onClickAction)
		{
			this._index = index;
			this._onClickAction = onClickAction;
			this.Button.onClick.Clear();
			this.Button.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnClickButtonEventDelegate)));
		}

		private void OnClickButtonEventDelegate()
		{
			this._onClickAction(this._index);
		}

		[SerializeField]
		public UITexture ImageTexture;

		[SerializeField]
		public GameObject LoadingGameObject;

		[SerializeField]
		public GameObject ErrorGameObject;

		[SerializeField]
		protected UIButton Button;

		[SerializeField]
		public GameObject SelectedGameObject;

		private Action<int> _onClickAction;

		private int _index;
	}
}

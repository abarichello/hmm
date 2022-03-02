using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialBottomPanelDescriptionCellView : MonoBehaviour, ITutorialBottomPanelDescriptionCellView
	{
		public IActivatable MainActivatable
		{
			get
			{
				return this._mainGameObjectActivatable;
			}
		}

		public ILabel DescriptionLabel
		{
			get
			{
				return this._descriptionLabel;
			}
		}

		public IActivatable InputIconActivatable
		{
			get
			{
				return this._inputIconGameObjectActivatable;
			}
		}

		public ISpriteImage InputIconImage
		{
			get
			{
				return this._inputIconImage;
			}
		}

		public IActivatable KeyboardActivatable
		{
			get
			{
				return this._keyboardGameObjectActivatable;
			}
		}

		public ILabel KeyboardLabel
		{
			get
			{
				return this._keyboardLabel;
			}
		}

		[SerializeField]
		private GameObjectActivatable _mainGameObjectActivatable;

		[SerializeField]
		private UnityLabel _descriptionLabel;

		[SerializeField]
		private GameObjectActivatable _inputIconGameObjectActivatable;

		[SerializeField]
		private UnityImage _inputIconImage;

		[SerializeField]
		private GameObjectActivatable _keyboardGameObjectActivatable;

		[SerializeField]
		private UnityLabel _keyboardLabel;
	}
}

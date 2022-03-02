using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnitySkinSelectorListItemView : MonoBehaviour, ISkinSelectorListItemView
	{
		public ILabel SkinNameLabel
		{
			get
			{
				return this._skinNameLabel;
			}
		}

		public IDynamicImage IconImage
		{
			get
			{
				return this._iconImage;
			}
		}

		public IToggle Toggle
		{
			get
			{
				return this._toggle;
			}
		}

		public IActivatable DefaultBorderActivatable
		{
			get
			{
				return this._defaultBorderActivatable;
			}
		}

		public IActivatable BronzeBorderActivatable
		{
			get
			{
				return this._bronzeBorderActivatable;
			}
		}

		public IActivatable SilverBorderActivatable
		{
			get
			{
				return this._silverBorderActivatable;
			}
		}

		public IActivatable GoldBorderActivatable
		{
			get
			{
				return this._goldBorderActivatable;
			}
		}

		public IActivatable HeavyMetalBorderActivatable
		{
			get
			{
				return this._heavyMetalBorderActivatable;
			}
		}

		public IActivatable ConfirmSelectionActivatable
		{
			get
			{
				return this._confirmSelectionActivatable;
			}
		}

		public Toggle SelectionToggle
		{
			get
			{
				return this._toggle.Toggle;
			}
		}

		[SerializeField]
		private UnityLabel _skinNameLabel;

		[SerializeField]
		private UnityDynamicImage _iconImage;

		[SerializeField]
		private UnityToggle _toggle;

		[SerializeField]
		private GameObjectActivatable _defaultBorderActivatable;

		[SerializeField]
		private GameObjectActivatable _bronzeBorderActivatable;

		[SerializeField]
		private GameObjectActivatable _silverBorderActivatable;

		[SerializeField]
		private GameObjectActivatable _goldBorderActivatable;

		[SerializeField]
		private GameObjectActivatable _heavyMetalBorderActivatable;

		[SerializeField]
		private GameObjectActivatable _confirmSelectionActivatable;
	}
}

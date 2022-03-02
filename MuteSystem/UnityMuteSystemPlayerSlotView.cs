using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;

namespace HeavyMetalMachines.MuteSystem
{
	public class UnityMuteSystemPlayerSlotView : MonoBehaviour, IMuteSystemPlayerSlotView
	{
		public ILabel PlayerNameLabel
		{
			get
			{
				return this._playerNameLabel;
			}
		}

		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public IDynamicImage CharacterIconDynamicImage
		{
			get
			{
				return this._characterIconDynamicImage;
			}
		}

		public IActivatable PsnIdIconActivatable
		{
			get
			{
				return this._psnIdIconActivatable;
			}
		}

		public ILabel PsnIdLabel
		{
			get
			{
				return this._psnIdLabel;
			}
		}

		public IButton MuteVoiceButton
		{
			get
			{
				return this._muteVoiceButton;
			}
		}

		public IHoverable MuteVoiceHoverable
		{
			get
			{
				return this._muteVoiceHoverable;
			}
		}

		public ISelectable MuteVoiceSelectable
		{
			get
			{
				return this._muteVoiceSelectable;
			}
		}

		public IButton UnMuteVoiceButton
		{
			get
			{
				return this._unMuteVoiceButton;
			}
		}

		public IHoverable UnMuteVoiceHoverable
		{
			get
			{
				return this._unMuteVoiceHoverable;
			}
		}

		public ISelectable UnMuteVoiceSelectable
		{
			get
			{
				return this._unMuteVoiceSelectable;
			}
		}

		public IButton MuteOtherActionsButton
		{
			get
			{
				return this._muteOtherActionsButton;
			}
		}

		public IHoverable MuteOtherActionsHoverable
		{
			get
			{
				return this._muteOtherActionsHoverable;
			}
		}

		public ISelectable MuteOtherActionsSelectable
		{
			get
			{
				return this._muteOtherActionsSelectable;
			}
		}

		public IButton UnMuteOtherActionsButton
		{
			get
			{
				return this._unMuteOtherActionsButton;
			}
		}

		public IHoverable UnMuteOtherActionsHoverable
		{
			get
			{
				return this._unMuteOtherActionsHoverable;
			}
		}

		public ISelectable UnMuteOtherActionsSelectable
		{
			get
			{
				return this._unMuteOtherActionsSelectable;
			}
		}

		public IButton BlockButton
		{
			get
			{
				return this._blockButton;
			}
		}

		public IHoverable BlockHoverable
		{
			get
			{
				return this._blockHoverable;
			}
		}

		public ISelectable BlockSelectable
		{
			get
			{
				return this._blockSelectable;
			}
		}

		public IButton UnBlockButton
		{
			get
			{
				return this._unBlockButton;
			}
		}

		public IHoverable UnBlockHoverable
		{
			get
			{
				return this._unBlockHoverable;
			}
		}

		public ISelectable UnBlockSelectable
		{
			get
			{
				return this._unBlockSelectable;
			}
		}

		public IActivatable BlockInfoActivatable
		{
			get
			{
				return this._blockInfoActivatable;
			}
		}

		public IButton ReportButton
		{
			get
			{
				return this._reportButton;
			}
		}

		public IHoverable ReportHoverable
		{
			get
			{
				return this._reportHoverable;
			}
		}

		public ISelectable ReportSelectable
		{
			get
			{
				return this._reportSelectable;
			}
		}

		private void Awake()
		{
			this._buttonTransforms = new Dictionary<IButton, Transform>
			{
				{
					this._muteVoiceButton,
					this._muteVoiceTransform
				},
				{
					this._unMuteVoiceButton,
					this._unMuteVoiceTransform
				},
				{
					this._muteOtherActionsButton,
					this._muteOtherActionsTransform
				},
				{
					this._unMuteOtherActionsButton,
					this._unMuteOtherActionsTransform
				},
				{
					this._blockButton,
					this._blockTransform
				},
				{
					this._unBlockButton,
					this._unBlockTransform
				},
				{
					this._reportButton,
					this._reportTransform
				}
			};
		}

		public void SetCharacterNameLabelAsAlly()
		{
			this.CharacterNameLabel.Color = this._allyPlayerNameColor.ToHmmColor();
		}

		public void SetCharacterNameLabelAsEnemy()
		{
			this.CharacterNameLabel.Color = this._enemyPlayerNameColor.ToHmmColor();
		}

		public void SetCharacterNameLabelAsNarrator()
		{
			this.CharacterNameLabel.Color = this._narratorPlayerNameColor.ToHmmColor();
		}

		public void TryToSelect(IButton button, IUiNavigationAxisSelectorTransformHandler axisSelectorTransformHandler)
		{
			axisSelectorTransformHandler.TryForceSelection(this._buttonTransforms[button]);
		}

		public string GetEmptyCharIconName()
		{
			return this._emptyCharIconName;
		}

		[SerializeField]
		private string _emptyCharIconName = "Generic_icon_char_64";

		[SerializeField]
		private Color _allyPlayerNameColor;

		[SerializeField]
		private Color _enemyPlayerNameColor;

		[SerializeField]
		private Color _narratorPlayerNameColor;

		[SerializeField]
		private UnityDynamicImage _characterIconDynamicImage;

		[SerializeField]
		private UnityLabel _playerNameLabel;

		[SerializeField]
		private UnityLabel _characterNameLabel;

		[SerializeField]
		private GameObjectActivatable _psnIdIconActivatable;

		[SerializeField]
		private UnityLabel _psnIdLabel;

		[SerializeField]
		private UnityButton _muteVoiceButton;

		[SerializeField]
		private UnityHoverable _muteVoiceHoverable;

		[SerializeField]
		private UnitySelectable _muteVoiceSelectable;

		[SerializeField]
		private Transform _muteVoiceTransform;

		[SerializeField]
		private UnityButton _unMuteVoiceButton;

		[SerializeField]
		private UnityHoverable _unMuteVoiceHoverable;

		[SerializeField]
		private UnitySelectable _unMuteVoiceSelectable;

		[SerializeField]
		private Transform _unMuteVoiceTransform;

		[SerializeField]
		private UnityButton _muteOtherActionsButton;

		[SerializeField]
		private UnityHoverable _muteOtherActionsHoverable;

		[SerializeField]
		private UnitySelectable _muteOtherActionsSelectable;

		[SerializeField]
		private Transform _muteOtherActionsTransform;

		[SerializeField]
		private UnityButton _unMuteOtherActionsButton;

		[SerializeField]
		private UnityHoverable _unMuteOtherActionsHoverable;

		[SerializeField]
		private UnitySelectable _unMuteOtherActionsSelectable;

		[SerializeField]
		private Transform _unMuteOtherActionsTransform;

		[SerializeField]
		private UnityButton _blockButton;

		[SerializeField]
		private UnityHoverable _blockHoverable;

		[SerializeField]
		private UnitySelectable _blockSelectable;

		[SerializeField]
		private Transform _blockTransform;

		[SerializeField]
		private UnityButton _unBlockButton;

		[SerializeField]
		private UnityHoverable _unBlockHoverable;

		[SerializeField]
		private UnitySelectable _unBlockSelectable;

		[SerializeField]
		private Transform _unBlockTransform;

		[SerializeField]
		private GameObjectActivatable _blockInfoActivatable;

		[SerializeField]
		private UnityButton _reportButton;

		[SerializeField]
		private UnityHoverable _reportHoverable;

		[SerializeField]
		private UnitySelectable _reportSelectable;

		[SerializeField]
		private Transform _reportTransform;

		private Dictionary<IButton, Transform> _buttonTransforms;
	}
}

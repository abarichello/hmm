using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityInputField : IInputField
	{
		public UnityInputField(InputField input)
		{
			this._input = input;
		}

		~UnityInputField()
		{
			if (this._submitOnEnterSubject != null)
			{
				this._submitOnEnterSubject.OnCompleted();
				this._submitOnEnterSubject = null;
			}
		}

		public string Text
		{
			get
			{
				return this._input.text;
			}
			set
			{
				this._input.text = value;
			}
		}

		public bool Interactable
		{
			get
			{
				return this._input.interactable;
			}
			set
			{
				this._input.interactable = value;
			}
		}

		public int CharacterLimit
		{
			get
			{
				return this._input.characterLimit;
			}
			set
			{
				this._input.characterLimit = value;
			}
		}

		public bool IsFocused
		{
			get
			{
				return this._input.isFocused;
			}
		}

		public IObservable<string> OnValueChanged()
		{
			this.TryFixCaret();
			return UnityUIComponentExtensions.OnValueChangedAsObservable(this._input);
		}

		public IObservable<string> OnEndEdit()
		{
			this.TryFixCaret();
			return UnityUIComponentExtensions.OnEndEditAsObservable(this._input);
		}

		public IObservable<string> OnSubmitWithEnter()
		{
			this.TryFixCaret();
			return this._submitOnEnterSubject;
		}

		public void SetupSubmitWithEnterOnlyMode()
		{
			this.TryFixCaret();
			this._submitOnEnterSubject = new Subject<string>();
			this._input.lineType = 2;
			InputField input = this._input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(this.ValidateEnter));
		}

		private char ValidateEnter(string text, int charindex, char addedchar)
		{
			if (addedchar == '\r' || addedchar == '\n')
			{
				if (this._submitOnEnterSubject != null)
				{
					this._submitOnEnterSubject.OnNext(text);
				}
				return '\0';
			}
			return addedchar;
		}

		private void TryFixCaret()
		{
			if (!this._caretFixed)
			{
				this._caretFixed = true;
				string text = this._input.text;
				this._input.text = text + " ";
				this._input.text = text;
			}
		}

		public void DisablePlaceholderText()
		{
			this._input.placeholder.gameObject.SetActive(false);
		}

		public void EnablePlaceholderText()
		{
			this._input.placeholder.gameObject.SetActive(true);
		}

		public void Clear()
		{
			this._input.text = string.Empty;
		}

		[SerializeField]
		private InputField _input;

		private Subject<string> _submitOnEnterSubject;

		private bool _caretFixed;

		private bool _isFocused;
	}
}

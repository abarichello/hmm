using System;
using System.Collections.Generic;
using System.Diagnostics;
using FMod;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ConfirmWindowReference : GameHubBehaviour, IPersistentGenericConfirmWindowPresenter
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action EvtConfirmWindowOpened;

		public bool Visible
		{
			get
			{
				return this._visible;
			}
		}

		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public void ExecConfirm()
		{
			if (this._openedWindowProperties == null || this._openedWindowProperties.OnConfirm == null)
			{
				return;
			}
			this._openedWindowProperties.OnConfirm();
			if (this._openedWindowProperties != null)
			{
				this._openedWindowProperties.OnConfirm = null;
			}
		}

		public void ExecRefuse()
		{
			if (this._openedWindowProperties == null || this._openedWindowProperties.OnRefuse == null)
			{
				return;
			}
			this._openedWindowProperties.OnRefuse();
		}

		public void ExecOk()
		{
			if (this._openedWindowProperties == null || this._openedWindowProperties.OnOk == null)
			{
				return;
			}
			this._openedWindowProperties.OnOk();
		}

		public void ExecTimeOut()
		{
			if (this._openedWindowProperties == null || this._openedWindowProperties.OnTimeOut == null)
			{
				return;
			}
			this._openedWindowProperties.OnTimeOut();
		}

		private void SetupConfirmWindow(ConfirmWindowProperties properties)
		{
			this._openedWindowProperties = properties;
			this._backgroundSprite.alpha = ((properties.BackgroundAlpha < 0f) ? this._defaultBackgroundAlpha : properties.BackgroundAlpha);
			if (!string.IsNullOrEmpty(properties.TileText))
			{
				this.TitleLabel.gameObject.SetActive(true);
				this.TitleLabel.text = properties.TileText;
			}
			else
			{
				this.TitleLabel.gameObject.SetActive(false);
			}
			this.Label.text = properties.QuestionText;
			if (!string.IsNullOrEmpty(properties.HintText))
			{
				this.HintLabel.gameObject.SetActive(true);
				this.HintLabel.text = properties.HintText;
			}
			else
			{
				this.HintLabel.gameObject.SetActive(false);
			}
			if (!string.IsNullOrEmpty(properties.CheckboxText))
			{
				this.Checkbox.gameObject.SetActive(true);
				this.Checkbox.value = properties.CheckboxInitialState;
				this.CheckboxUILabel.text = properties.CheckboxText;
				this.CheckboxUILabel.gameObject.SetActive(true);
			}
			else
			{
				this.Checkbox.gameObject.SetActive(false);
				this.CheckboxUILabel.gameObject.SetActive(false);
			}
			if (properties.OnConfirm != null)
			{
				this.ConfirmButton.enabled = true;
				this.ConfirmButton.gameObject.SetActive(true);
				this.ConfirmButton.ButtonText.text = properties.ConfirmButtonText;
			}
			else
			{
				this.ConfirmButton.enabled = false;
				this.ConfirmButton.gameObject.SetActive(false);
			}
			if (properties.OnRefuse != null)
			{
				this.RefuseButton.enabled = true;
				this.RefuseButton.gameObject.SetActive(true);
				this.RefuseButton.ButtonText.text = properties.RefuseButtonText;
			}
			else
			{
				this.RefuseButton.enabled = false;
				this.RefuseButton.gameObject.SetActive(false);
			}
			if (properties.OnOk != null)
			{
				this.OkButton.enabled = true;
				this.OkButton.gameObject.SetActive(true);
				this.OkButton.ButtonText.text = properties.OkButtonText;
			}
			else
			{
				this.OkButton.enabled = false;
				this.OkButton.gameObject.SetActive(false);
			}
			if (properties.OnTimeOut != null && properties.CountDownTime > 0f)
			{
				this._countDownLabel.gameObject.SetActive(true);
				this._timer = properties.CountDownTime;
				this._countdownAudioPlayed = 0;
				this._timedWindowIsRunning = true;
			}
			else
			{
				this._countDownLabel.gameObject.SetActive(false);
				this._timedWindowIsRunning = false;
			}
			this._clockTextFormat = properties.ClockTextFormat;
			this.LoadingGameObject.SetActive(properties.EnableLoadGameObject);
			this._itemErrorGameObject.SetActive(properties.EnableItemErrorGameObject);
		}

		public bool IsShowing(Guid guid)
		{
			return this._openedWindowProperties != null && guid == this._openedWindowProperties.Guid;
		}

		private void ShowConfirmWindow(ConfirmWindowProperties properties)
		{
			base.gameObject.SetActive(true);
			this.SetupConfirmWindow(properties);
			this._visible = true;
			ConfirmWindowReference.Log.DebugFormat("Showing window. Question:{0} Text: {1}, guid: {2}", new object[]
			{
				properties.QuestionText,
				properties.CheckboxText,
				properties.Guid
			});
			if (ConfirmWindowReference.EvtConfirmWindowOpened != null)
			{
				ConfirmWindowReference.EvtConfirmWindowOpened();
			}
			GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
		}

		public void HideConfirmWindow(Guid guid)
		{
			if (this.TryHideWindowAndCloseIfOpened(guid))
			{
				return;
			}
			int num = this._confirmWindowStack.FindLastIndex((ConfirmWindowProperties p) => p.Guid == guid);
			if (num < 0)
			{
				return;
			}
			this._confirmWindowStack.RemoveAt(num);
			ConfirmWindowReference.Log.DebugFormat("Removed Guid \"{0}\" from stack. Stack Count: {1}", new object[]
			{
				guid,
				this._confirmWindowStack
			});
		}

		private bool TryHideWindowAndCloseIfOpened(Guid guid)
		{
			if (!this.IsCurrentGuidOpened(guid))
			{
				return false;
			}
			ConfirmWindowReference.Log.DebugFormat("Closing window. Text: {0}, guid: {1}", new object[]
			{
				this.Label.text,
				guid
			});
			base.gameObject.SetActive(false);
			GameHubBehaviour.Hub.CursorManager.Pop();
			this._visible = false;
			this._openedWindowProperties = null;
			this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			if (this._confirmWindowStack.Count <= 0)
			{
				return true;
			}
			int index = this._confirmWindowStack.Count - 1;
			ConfirmWindowProperties properties = this._confirmWindowStack[index];
			this._confirmWindowStack.RemoveAt(index);
			this.ShowConfirmWindow(properties);
			return true;
		}

		private bool IsCurrentGuidOpened(Guid guid)
		{
			return this._openedWindowProperties != null && guid == this._openedWindowProperties.Guid;
		}

		public void QueueConfirmWindow(ConfirmWindowProperties properties)
		{
			ConfirmWindowReference.Log.DebugFormat("Stacking window. Text: {0}, guid: {1}", new object[]
			{
				properties.CheckboxText,
				properties.Guid
			});
			this._confirmWindowStack.Add(properties);
		}

		public Guid OpenConfirmWindow(string questionText, string okButtonText, Action onOk)
		{
			Guid windowId = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = windowId,
				QuestionText = questionText,
				OkButtonText = okButtonText,
				OnOk = delegate()
				{
					onOk();
					this.HideConfirmWindow(windowId);
				}
			};
			this.OpenConfirmWindow(properties);
			return windowId;
		}

		public void OpenConfirmWindow(ConfirmWindowProperties properties)
		{
			if (this.Visible)
			{
				if (this._openedWindowProperties.IsStackable)
				{
					this.QueueConfirmWindow(this._openedWindowProperties);
					this.ShowConfirmWindow(properties);
					return;
				}
				this.HideConfirmWindow(this._openedWindowProperties.Guid);
			}
			this.ShowConfirmWindow(properties);
		}

		public void OpenCloseGameConfirmWindow(Action preQuitCallback)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("CloseGameQuestion", TranslationContext.MainMenuGui),
				ConfirmButtonText = Language.Get("CLOSEGAME_YES", TranslationContext.MainMenuGui),
				OnConfirm = delegate()
				{
					this.HideConfirmWindow(confirmWindowGuid);
					if (preQuitCallback != null)
					{
						preQuitCallback();
					}
					GameHubBehaviour.Hub.Quit(8);
				},
				RefuseButtonText = Language.Get("CLOSEGAME_NO", TranslationContext.MainMenuGui),
				OnRefuse = delegate()
				{
					this.HideConfirmWindow(confirmWindowGuid);
				}
			};
			this.OpenConfirmWindow(properties);
		}

		public void Update()
		{
			if (!this._timedWindowIsRunning)
			{
				return;
			}
			if (this._openedWindowProperties.TimerPausePredicate != null && this._openedWindowProperties.TimerPausePredicate())
			{
				return;
			}
			this._timer -= Time.deltaTime;
			if (this._clockTextFormat)
			{
				this._countDownLabel.text = string.Format("{0:0}:{1:0#}", Mathf.Floor(this._timer / 60f), Mathf.Floor(this._timer % 60f));
			}
			else
			{
				this._countDownLabel.text = Mathf.CeilToInt(this._timer).ToString();
			}
			if (this._timer <= 5f && Mathf.FloorToInt(this._timer) + this._countdownAudioPlayed != 5)
			{
				this._countdownAudioPlayed++;
				FMODAudioManager.PlayOneShotAt(this.CountdownAudio, Vector3.zero, 0);
			}
			if (this._timer > 0f)
			{
				return;
			}
			this.StopCountdownAndCallCallback();
		}

		private void StopCountdownAndCallCallback()
		{
			this._timedWindowIsRunning = false;
			if (this._openedWindowProperties.OnTimeOut != null)
			{
				this._openedWindowProperties.OnTimeOut();
			}
		}

		public IObservable<Unit> Show(DialogConfiguration configuration)
		{
			ConfirmWindowReference.<Show>c__AnonStorey3 <Show>c__AnonStorey = new ConfirmWindowReference.<Show>c__AnonStorey3();
			<Show>c__AnonStorey.configuration = configuration;
			<Show>c__AnonStorey.$this = this;
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				bool isDisposed = false;
				Guid id = Guid.NewGuid();
				<Show>c__AnonStorey.$this.ShowConfirmWindow(new ConfirmWindowProperties
				{
					Guid = id,
					TileText = <Show>c__AnonStorey.configuration.Title,
					QuestionText = <Show>c__AnonStorey.configuration.Message,
					OkButtonText = Language.Get("Ok", TranslationContext.GUI),
					OnOk = delegate()
					{
						if (!isDisposed)
						{
							observer.OnNext(Unit.Default);
							observer.OnCompleted();
							<Show>c__AnonStorey.HideConfirmWindow(id);
						}
					}
				});
				return Disposable.Create(delegate()
				{
					isDisposed = true;
				});
			});
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ConfirmWindowReference));

		private readonly List<ConfirmWindowProperties> _confirmWindowStack = new List<ConfirmWindowProperties>();

		private ConfirmWindowProperties _openedWindowProperties;

		[SerializeField]
		private UI2DSprite _backgroundSprite;

		[SerializeField]
		private float _defaultBackgroundAlpha = 0.9f;

		public UILabel TitleLabel;

		public UILabel Label;

		public UILabel HintLabel;

		public ButtonScriptReference ConfirmButton;

		public ButtonScriptReference RefuseButton;

		public ButtonScriptReference OkButton;

		public UIToggle Checkbox;

		public UILabel CheckboxUILabel;

		public GameObject LoadingGameObject;

		[SerializeField]
		private GameObject _itemErrorGameObject;

		[Header("Timed window properties")]
		[SerializeField]
		private UILabel _countDownLabel;

		private float _timer;

		private bool _timedWindowIsRunning;

		private bool _clockTextFormat;

		private bool _visible;

		private int _countdownAudioPlayed;

		public AudioEventAsset CountdownAudio;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}

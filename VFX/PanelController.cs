using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FMod;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public sealed class PanelController : SingletonMonoBehaviour<PanelController>
	{
		public ColorConfiguration ColorConfiguration
		{
			get
			{
				return this._colorConfiguration;
			}
		}

		public int HighestPanelDepth
		{
			get
			{
				return this._highestPanelDepth;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event PanelController.SystemMessageDelegate EvtSystemMessage;

		public MessageHintGuiItem BaseHintGuiItem
		{
			get
			{
				return this._baseHintGuiItem;
			}
		}

		public MessageHintGuiItem SendSystemMessage(string systemMessage, string senderUniversalId = "SystemMessage", bool sendChatMessage = true, bool isScreenTransition = false, StackableHintKind targetStackableHintKind = StackableHintKind.None, HintColorScheme colorScheme = HintColorScheme.System)
		{
			MessageHintGuiItem result = null;
			if (!isScreenTransition)
			{
				float timeoutSeconds = 5f;
				BaseHintContent baseHintContent = new BaseHintContent(systemMessage, timeoutSeconds, true, null, senderUniversalId);
				result = this.ShowMessageHint(baseHintContent, targetStackableHintKind, colorScheme);
			}
			if (sendChatMessage && PanelController.EvtSystemMessage != null)
			{
				PanelController.EvtSystemMessage(systemMessage, senderUniversalId);
			}
			return result;
		}

		protected override void Awake()
		{
			base.Awake();
			this._diContainer = this._diContainer.ParentContainers.First<DiContainer>();
		}

		private bool CanShowHint()
		{
			return !(GameHubBehaviour.Hub.State.Current is Game) && !(GameHubBehaviour.Hub.State.Current is PickModeSetup);
		}

		private void onHintDismissed(IHint dismissedHint)
		{
			this._activeHintList.Remove(dismissedHint);
			if (this._hintQueue.Count > 0)
			{
				IHint hint = this._hintQueue[0];
				this._hintQueue.Remove(hint);
				hint.ActivateHint();
				this.UpdateHintIndex(hint);
				this._activeHintList.Add(hint);
			}
			this._mustRefreshHintPosition = true;
		}

		public MessageHintGuiItem ShowMessageHint(BaseHintContent baseHintContent, StackableHintKind targetStackableHintKind = StackableHintKind.None, HintColorScheme colorScheme = HintColorScheme.System)
		{
			if (!this.CanShowHint())
			{
				return null;
			}
			FMODAudioManager.PlayOneShotAt(this.DefaultFeedBackSFX, base.transform.position, 0);
			bool flag = false;
			MessageHintGuiItem messageHintGuiItem;
			if (targetStackableHintKind == StackableHintKind.None)
			{
				messageHintGuiItem = this._baseHintGuiItem.CreateNewGuiItem(baseHintContent, false, null);
			}
			else if (this._currentActiveStackableSystemMessagesDict.TryGetValue(targetStackableHintKind, out messageHintGuiItem) && messageHintGuiItem != null)
			{
				messageHintGuiItem.SetProperties(baseHintContent);
				flag = true;
			}
			else
			{
				messageHintGuiItem = this._baseHintGuiItem.CreateNewGuiItem(baseHintContent, false, null);
				this._currentActiveStackableSystemMessagesDict[targetStackableHintKind] = messageHintGuiItem;
			}
			this.SetHintColorScheme<MessageHintGuiItem, BaseHintContent>(messageHintGuiItem, colorScheme);
			if (flag)
			{
				messageHintGuiItem.ActivateHint();
				return messageHintGuiItem;
			}
			messageHintGuiItem.EvtHintDismissed += this.onHintDismissed;
			if (this._activeHintList.Count >= 5)
			{
				this._hintQueue.Add(messageHintGuiItem);
				messageHintGuiItem.gameObject.SetActive(false);
				return messageHintGuiItem;
			}
			this._activeHintList.Add(messageHintGuiItem);
			this._mustRefreshHintPosition = true;
			messageHintGuiItem.ActivateHint();
			return messageHintGuiItem;
		}

		public QuestionHintGuiItem ShowQuestionHint(QuestionHintContent questionHintContent, HintColorScheme colorScheme = HintColorScheme.Group)
		{
			if (!this.CanShowHint())
			{
				return null;
			}
			FMODAudioManager.PlayOneShotAt(this.DefaultFeedBackSFX, base.transform.position, 0);
			QuestionHintGuiItem questionHintGuiItem = this._baseQuestionHintGuiItem.CreateNewGuiItem(questionHintContent, false, null);
			questionHintGuiItem.EvtHintDismissed += this.onHintDismissed;
			this.SetHintColorScheme<QuestionHintGuiItem, QuestionHintContent>(questionHintGuiItem, colorScheme);
			if (this._activeHintList.Count >= 5)
			{
				this._hintQueue.Add(questionHintGuiItem);
				questionHintGuiItem.gameObject.SetActive(false);
				return questionHintGuiItem;
			}
			this._activeHintList.Add(questionHintGuiItem);
			questionHintGuiItem.ActivateHint();
			this._mustRefreshHintPosition = true;
			return questionHintGuiItem;
		}

		private void SetHintColorScheme<T, TContent>(BaseHintGuiItem<T, TContent> baseHintGuiItem, HintColorScheme colorScheme) where T : BaseHintGuiItem<T, TContent> where TContent : BaseHintContent
		{
			switch (colorScheme)
			{
			case HintColorScheme.Friend:
				baseHintGuiItem.GlowEffect.color = this.ColorConfiguration.FriendTextColor;
				break;
			case HintColorScheme.Group:
				baseHintGuiItem.GlowEffect.color = this.ColorConfiguration.HintPartyGlowColor;
				break;
			case HintColorScheme.Chat:
				baseHintGuiItem.GlowEffect.color = this.ColorConfiguration.HintSystemGlowColor;
				break;
			case HintColorScheme.Refused:
				baseHintGuiItem.GlowEffect.color = this.ColorConfiguration.HintRefuseGlowColor;
				break;
			default:
				baseHintGuiItem.GlowEffect.color = this.ColorConfiguration.HintSystemGlowColor;
				break;
			}
		}

		public void DismissStackableHint(StackableHintKind targetStackableHintKind)
		{
			MessageHintGuiItem messageHintGuiItem;
			if (this._currentActiveStackableSystemMessagesDict.TryGetValue(targetStackableHintKind, out messageHintGuiItem) && messageHintGuiItem != null)
			{
				this._currentActiveStackableSystemMessagesDict.Remove(targetStackableHintKind);
				messageHintGuiItem.DismissQuestionHint();
			}
		}

		private void UpdateHintIndex(IHint hint)
		{
			hint.UpdateIndex(this._currentHintIndex);
			this._currentHintIndex += 1L;
		}

		private void TryRefreshHintGridPosition()
		{
			if (!this._mustRefreshHintPosition)
			{
				return;
			}
			this._mustRefreshHintPosition = false;
			for (int i = 0; i < this._activeHintList.Count; i++)
			{
				this.UpdateHintIndex(this._activeHintList[i]);
			}
			this.HintGrid.repositionNow = true;
		}

		public static bool IsCurrentEventLeftClick()
		{
			return UICamera.currentTouchID == -1;
		}

		public static bool IsCurrentEventRightClick()
		{
			return UICamera.currentTouchID == -2;
		}

		public static bool IsCurrentEventMiddleClick()
		{
			return UICamera.currentTouchID == -3;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this._uiConfiguration.AvailableModalGuiList.Clear();
		}

		private void Update()
		{
			this.TryRefreshHintGridPosition();
		}

		public static void Broadcast(UiEvent eventName)
		{
			SingletonMonoBehaviour<PanelController>.Instance.BroadcastMessage(eventName.ToString(), 1);
		}

		public Future<int> ShowDialog(string contentText, string confirmMsg, string cancelMsg = "", string[] otherMessages = null)
		{
			return this.ShowDialog(0, contentText, confirmMsg, cancelMsg, otherMessages);
		}

		public Future<int> ShowDialog(int templateIndex, string contentText, string confirmMsg, string cancelMsg = "", string[] otherMessages = null)
		{
			DialogGUIController dialogGUIController = this.CreatePanel<DialogGUIController>(this.BaseUIDialogTemplate[templateIndex]);
			return dialogGUIController.InitDialog(contentText, confirmMsg, cancelMsg, otherMessages);
		}

		public void ToggleModalWindow<T>() where T : ModalGUIController
		{
			if (this.TryCloseModalWindow<T>())
			{
				return;
			}
			this.ShowModalWindow<T>();
		}

		public bool TryCloseModalWindow<T>() where T : ModalGUIController
		{
			T t = (T)((object)this._activeModalControllers.Find((ModalGUIController controller) => controller is T));
			if (t == null)
			{
				return false;
			}
			t.ForceResolveModalWindow(false);
			return true;
		}

		public bool IsCurrentModalOfType<T>() where T : ModalGUIController
		{
			return this._modalControllersStack.Count > 0 && this._modalControllersStack.Peek() is T;
		}

		public bool IsModalOfTypeOpened<T>() where T : ModalGUIController
		{
			Stack<ModalGUIController>.Enumerator enumerator = this._modalControllersStack.GetEnumerator();
			bool result = false;
			while (enumerator.MoveNext())
			{
				ModalGUIController modalGUIController = enumerator.Current;
				if (modalGUIController is T)
				{
					result = true;
					break;
				}
			}
			enumerator.Dispose();
			return result;
		}

		public Future ShowModalWindow<T>(out T modalGUIController) where T : ModalGUIController
		{
			ModalGUIController modalGUIController2 = this._activeModalControllers.Find((ModalGUIController controller) => controller is T);
			if (modalGUIController2 != null)
			{
				modalGUIController = (T)((object)modalGUIController2);
				return modalGUIController.CurrentFuture;
			}
			Type typeFromHandle = typeof(T);
			T t = (T)((object)this.GetModalWindowFromType(typeFromHandle));
			if (t == null)
			{
				string text = string.Format("Couldn't find Modal Window prefab: \"{0}\"", typeFromHandle.Name);
				Debug.LogError(text);
				Future future = new Future();
				future.ExceptionThrowed = new MissingReferenceException(text);
				modalGUIController = (T)((object)null);
				return future;
			}
			ModalGUIController modalGUIController3;
			if (this._activeModalControllersByGroup.TryGetValue(t.ModalGroupID, out modalGUIController3))
			{
				modalGUIController3.ForceResolveModalWindow(false);
			}
			modalGUIController = this.CreatePanel<T>(t);
			return modalGUIController.InitModalWindow();
		}

		public Future ShowModalWindow<T>() where T : ModalGUIController
		{
			T t;
			return this.ShowModalWindow<T>(out t);
		}

		private ModalGUIController GetModalWindowFromType(Type modalType)
		{
			for (int i = 0; i < this._uiConfiguration.AvailableModalGuiList.Count; i++)
			{
				if (this._uiConfiguration.AvailableModalGuiList[i].GetType() == modalType)
				{
					return this._uiConfiguration.AvailableModalGuiList[i];
				}
			}
			return null;
		}

		public int ActiveModalControllersCount
		{
			get
			{
				return this._modalControllersStack.Count;
			}
		}

		public void RegisterAvailableModalWindow(ModalGUIController modalGUIController)
		{
			if (this._uiConfiguration.AvailableModalGuiList.Contains(modalGUIController))
			{
				Debug.LogWarning(string.Format("Trying to register Modal Window \"{0}\" again!", modalGUIController.gameObject.name), modalGUIController);
				return;
			}
			this._uiConfiguration.AvailableModalGuiList.Add(modalGUIController);
		}

		public void UnregisterAvailableModalWindow(ModalGUIController modalGUIController)
		{
			if (this._uiConfiguration.AvailableModalGuiList.Remove(modalGUIController))
			{
				return;
			}
			Debug.LogWarning(string.Format("Trying to UNregister Modal Window \"{0}\", but got no ocurrences on list!", modalGUIController.gameObject.name), modalGUIController);
		}

		public void RegisterActiveModalWindow(ModalGUIController modalGUIController)
		{
			this._modalControllersStack.Push(modalGUIController);
			if (modalGUIController.ModalGroupID >= 0)
			{
				this._activeModalControllersByGroup.Add(modalGUIController.ModalGroupID, modalGUIController);
			}
			if (!this._activeModalControllers.Contains(modalGUIController))
			{
				this._activeModalControllers.Add(modalGUIController);
			}
		}

		public void UnregisterActiveModalWindow(ModalGUIController modalGUIController)
		{
			if (this._modalControllersStack.Count > 0 && this._modalControllersStack.Peek() == modalGUIController)
			{
				this._modalControllersStack.Pop();
			}
			if (modalGUIController.ModalGroupID >= 0)
			{
				this._activeModalControllersByGroup.Remove(modalGUIController.ModalGroupID);
			}
			if (this._activeModalControllers.Contains(modalGUIController))
			{
				this._activeModalControllers.Remove(modalGUIController);
			}
		}

		public T CreatePanel<T>(T guiController) where T : BaseGUIController
		{
			T result;
			if (guiController.transform.root == base.transform)
			{
				guiController.Panel.alpha = 1f;
				guiController.DestroyOnScreenResolution = false;
				result = guiController;
			}
			else
			{
				result = this._diContainer.InstantiatePrefabForComponent<T>(guiController);
				result.transform.SetParent(this.MainDynamicParentTransform, false);
			}
			result.transform.localPosition = Vector3.zero;
			result.transform.localScale = Vector3.one;
			return result;
		}

		public void RemovePanel(BaseGUIController baseGUI)
		{
			if (baseGUI.DestroyOnScreenResolution)
			{
				Object.Destroy(baseGUI.gameObject);
				return;
			}
			baseGUI.Panel.alpha = 0f;
		}

		public const float SystemMessagesTimeout = 5f;

		[Header("Default Hint Colors")]
		[SerializeField]
		private ColorConfiguration _colorConfiguration;

		private int _highestPanelDepth;

		[Header("HintQuestion")]
		[SerializeField]
		private MessageHintGuiItem _baseHintGuiItem;

		[SerializeField]
		private QuestionHintGuiItem _baseQuestionHintGuiItem;

		[SerializeField]
		public UIGrid HintGrid;

		private bool _mustRefreshHintPosition;

		public Transform MainDynamicParentTransform;

		public DialogGUIController[] BaseUIDialogTemplate;

		[SerializeField]
		private UIBaseConfigurationSO _uiConfiguration;

		[Header("Audio")]
		public AudioEventAsset DefaultFeedBackSFX;

		[Inject]
		private DiContainer _diContainer;

		private Dictionary<StackableHintKind, MessageHintGuiItem> _currentActiveStackableSystemMessagesDict = new Dictionary<StackableHintKind, MessageHintGuiItem>();

		public const string SystemMsgId = "SystemMessage";

		private const int MaxActiveHints = 5;

		private List<IHint> _hintQueue = new List<IHint>();

		private List<IHint> _activeHintList = new List<IHint>();

		private long _currentHintIndex;

		private List<ModalGUIController> _activeModalControllers = new List<ModalGUIController>();

		private Dictionary<int, ModalGUIController> _activeModalControllersByGroup = new Dictionary<int, ModalGUIController>();

		private Stack<ModalGUIController> _modalControllersStack = new Stack<ModalGUIController>();

		public delegate void SystemMessageDelegate(string message, string senderId = null);
	}
}

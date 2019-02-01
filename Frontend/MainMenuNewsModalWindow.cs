using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuNewsModalWindow : HudWindow
	{
		protected void Awake()
		{
			this._canAutoOpenWindow = true;
		}

		public void Setup(MainMenuNewsInfoController.NewsJsonObject[] newsJsonObjects, EventDelegate.Callback onCloseWindowDelegate, MainMenuNewsModalWindow.HmmSelectionDelegate onItemTypeSelection)
		{
			this._onCloseWindowCallback = onCloseWindowDelegate;
			this._newsJsonObjects = newsJsonObjects;
			this._onItemTypeSelection = onItemTypeSelection;
			HeavyMetalMachines.Utils.Debug.Assert(this._newsJsonObjects.Length > 0, "[SD] MainMenuNewsModalWindow setup for an empty list.", HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			HeavyMetalMachines.Utils.Debug.Assert(this._newsJsonObjects.Length <= 5, string.Format("[MKT] News Window. Too many entries:[{0}]. Max:[{1}].", this._newsJsonObjects.Length, 5), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			this.LoadingGroupGameObject.SetActive(false);
			this.ImageLoadErrorGroupGameObject.SetActive(false);
			this.CloseButton.onClick.Clear();
			this.CloseButton.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnCloseButtonEventDelegate)));
			this.ImageButton.onClick.Clear();
			this.ImageButton.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnImageClickEventDelegate)));
			this._currentIndex = 0;
			if (MainMenuNewsModalWindow._textureCache == null)
			{
				MainMenuNewsModalWindow._textureCache = new Dictionary<string, Texture2D>(5);
			}
			bool hideInactive = this.ThumbGrid.hideInactive;
			this.ThumbGrid.hideInactive = false;
			List<Transform> childList = this.ThumbGrid.GetChildList();
			if (childList.Count == 1)
			{
				GUIUtils.CreateGridPool(this.ThumbGrid, 5);
				childList = this.ThumbGrid.GetChildList();
			}
			this._thumbInfos = new MainMenuNewsThumbInfo[childList.Count];
			for (int i = 0; i < childList.Count; i++)
			{
				Transform transform = childList[i];
				this._thumbInfos[i] = transform.GetComponent<MainMenuNewsThumbInfo>();
				this._thumbInfos[i].Setup(i, new Action<int>(this.OnThumbButtonAction));
				transform.gameObject.SetActive(false);
			}
			this.ThumbGrid.hideInactive = hideInactive;
			this._canCycleThumb = true;
			this.ResetThumbCycleTime();
		}

		protected void Update()
		{
			if (!this.IsVisible)
			{
				return;
			}
			if (this._canCycleThumb)
			{
				this._cycleThumbTimeInSec -= Time.deltaTime;
				if (this._cycleThumbTimeInSec <= 0f)
				{
					this.RightSelectNews();
				}
			}
		}

		private void OnCloseButtonEventDelegate()
		{
			this._canAutoOpenWindow = false;
			base.SetWindowVisibility(false);
		}

		private void OnImageClickEventDelegate()
		{
			string link = this._newsJsonObjects[this._currentIndex].link;
			if (string.IsNullOrEmpty(link))
			{
				return;
			}
			MainMenuNewsInfoController.NewsHmmSelectionType selectionType;
			Guid itemTypeId;
			bool flag = this.IsHmmItemTypeSelection(link, out selectionType, out itemTypeId);
			if (flag)
			{
				this._onItemTypeSelection(selectionType, itemTypeId);
			}
			else
			{
				OpenUrlUtils.OpenUrl(link);
			}
		}

		private bool IsHmmItemTypeSelection(string link, out MainMenuNewsInfoController.NewsHmmSelectionType selectionType, out Guid itemTypeId)
		{
			selectionType = MainMenuNewsInfoController.NewsHmmSelectionType.Cash;
			itemTypeId = Guid.Empty;
			string text = link.ToLower();
			if (!text.StartsWith("hmm://"))
			{
				return false;
			}
			text = text.Remove(0, "hmm://".Length);
			string[] array = text.Split(new char[]
			{
				'/'
			});
			if (array.Length == 0)
			{
				MainMenuNewsModalWindow.Log.ErrorFormat("Hmm ItemType format error. Link:{0}", new object[]
				{
					link
				});
				return true;
			}
			bool result;
			try
			{
				selectionType = (MainMenuNewsInfoController.NewsHmmSelectionType)Enum.Parse(typeof(MainMenuNewsInfoController.NewsHmmSelectionType), array[0], true);
				if (array.Length == 1)
				{
					result = true;
				}
				else if (string.IsNullOrEmpty(array[1]))
				{
					result = true;
				}
				else
				{
					itemTypeId = new Guid(array[1]);
					result = true;
				}
			}
			catch (Exception e)
			{
				MainMenuNewsModalWindow.Log.Warn(string.Format("Hmm ItemType selection exception for link [{0}]. Moving to faalback.", text), e);
				result = true;
			}
			return result;
		}

		private void OnRightButtonEventDelegate()
		{
			this.StopThumbCycle();
			this.RightSelectNews();
		}

		private void RightSelectNews()
		{
			this._currentIndex++;
			if (this._currentIndex >= this._newsJsonObjects.Length)
			{
				this._currentIndex = 0;
			}
			this.SelectNews(this._currentIndex);
		}

		private void OnLeftButtonEventDelegate()
		{
			this.StopThumbCycle();
			this._currentIndex--;
			if (this._currentIndex < 0)
			{
				this._currentIndex = this._newsJsonObjects.Length - 1;
			}
			this.SelectNews(this._currentIndex);
		}

		private void OnThumbButtonAction(int index)
		{
			this.StopThumbCycle();
			this._currentIndex = index;
			this.SelectNews(this._currentIndex);
		}

		public override void AnimationOnWindowExit()
		{
			base.AnimationOnWindowExit();
			this._onCloseWindowCallback();
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				this.LoadThumbnails();
				this.SelectNews(this._currentIndex);
			}
		}

		public override bool IsStackableWithType(Type type)
		{
			return true;
		}

		private void LoadThumbnails()
		{
			int num = 0;
			while (num < this._newsJsonObjects.Length && num < this._thumbInfos.Length)
			{
				MainMenuNewsInfoController.NewsJsonObject newsJsonObject = this._newsJsonObjects[num];
				MainMenuNewsThumbInfo mainMenuNewsThumbInfo = this._thumbInfos[num];
				mainMenuNewsThumbInfo.gameObject.SetActive(true);
				base.StartCoroutine(this.LoadSpriteAsync(newsJsonObject.thumb, mainMenuNewsThumbInfo.LoadingGameObject, mainMenuNewsThumbInfo.ErrorGameObject, mainMenuNewsThumbInfo.ImageTexture, null));
				num++;
			}
			this.ThumbGrid.Reposition();
		}

		private void SelectNews(int index)
		{
			this.ResetThumbCycleTime();
			for (int i = 0; i < this._thumbInfos.Length; i++)
			{
				MainMenuNewsThumbInfo mainMenuNewsThumbInfo = this._thumbInfos[i];
				mainMenuNewsThumbInfo.SelectedGameObject.SetActive(i == index);
				mainMenuNewsThumbInfo.GetComponent<Collider>().enabled = (i != index);
			}
			MainMenuNewsInfoController.NewsJsonObject newsJsonObject = this._newsJsonObjects[index];
			this.ImageButton.SetState(UIButtonColor.State.Normal, true);
			this.ImageButton.enabled = !string.IsNullOrEmpty(newsJsonObject.link);
			this.ImageButton.GetComponent<BoxCollider>().enabled = this.ImageButton.enabled;
			base.StartCoroutine(this.LoadSpriteAsync(newsJsonObject.image, this.LoadingGroupGameObject, this.ImageLoadErrorGroupGameObject, this.NewsTexture, null));
		}

		private IEnumerator LoadSpriteAsync(string url, GameObject loadingGameObject, GameObject errorGameObject, UITexture uiTexture, Action<Texture2D> onTextureLoaded)
		{
			loadingGameObject.SetActive(false);
			bool hasError = false;
			if (errorGameObject != null)
			{
				errorGameObject.SetActive(false);
			}
			Texture2D texture;
			if (!MainMenuNewsModalWindow._textureCache.TryGetValue(url, out texture))
			{
				loadingGameObject.SetActive(true);
				uiTexture.mainTexture = null;
				string urlRandomNum = "?" + UnityEngine.Random.Range(1, 9999);
				WWW www = new WWW(url + urlRandomNum);
				yield return www;
				if (string.IsNullOrEmpty(www.error))
				{
					if (www.texture.width == 8 && www.texture.height == 8)
					{
						MainMenuNewsModalWindow.Log.WarnFormat("Invalid texture on LoadSpriteAsync. Url:{0}", new object[]
						{
							url
						});
						hasError = true;
					}
					else
					{
						texture = www.texture;
						MainMenuNewsModalWindow._textureCache[url] = texture;
					}
				}
				else
				{
					MainMenuNewsModalWindow.Log.WarnFormat("LoadSpriteAsync - WWW error on load texture. Url:[{0}] - Msg:[{1}]. Probably the service is offline or the url is invalid.", new object[]
					{
						url,
						www.error
					});
					HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("[MKT] News: Error on load texture. Url:[{0}] - Msg:[{1}]. Probably the service is offline or the url is invalid.", url, www.error), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					hasError = true;
				}
			}
			if (onTextureLoaded != null)
			{
				onTextureLoaded(texture);
			}
			uiTexture.mainTexture = texture;
			loadingGameObject.SetActive(false);
			if (hasError && errorGameObject != null)
			{
				errorGameObject.SetActive(true);
				uiTexture.mainTexture = null;
			}
			yield break;
		}

		public static void ClearTextureCache()
		{
			if (MainMenuNewsModalWindow._textureCache != null)
			{
				MainMenuNewsModalWindow._textureCache.Clear();
			}
			MainMenuNewsModalWindow._textureCache = null;
		}

		private void ResetThumbCycleTime()
		{
			this._cycleThumbTimeInSec = this.CycleThumbTimeoutInSec;
		}

		private void StopThumbCycle()
		{
			this._canCycleThumb = false;
		}

		public bool TryToAutoOpenWindow()
		{
			if (!this._canAutoOpenWindow)
			{
				return false;
			}
			base.SetWindowVisibility(true);
			return true;
		}

		public void DisableButtonCollider()
		{
			this.ImageButton.GetComponent<BoxCollider>().enabled = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MainMenuNewsModalWindow));

		private const string LinkItemTypeIdPrefix = "hmm://";

		public float CycleThumbTimeoutInSec = 5f;

		private const int NumMaxThumbs = 5;

		[SerializeField]
		protected UITexture NewsTexture;

		[SerializeField]
		protected GameObject LoadingGroupGameObject;

		[SerializeField]
		protected UIButton CloseButton;

		[SerializeField]
		protected UIButton ImageButton;

		[SerializeField]
		protected GameObject ImageLoadErrorGroupGameObject;

		[SerializeField]
		protected UIGrid ThumbGrid;

		private EventDelegate.Callback _onCloseWindowCallback;

		private MainMenuNewsModalWindow.HmmSelectionDelegate _onItemTypeSelection;

		private MainMenuNewsInfoController.NewsJsonObject[] _newsJsonObjects;

		private int _currentIndex;

		private static Dictionary<string, Texture2D> _textureCache;

		private MainMenuNewsThumbInfo[] _thumbInfos;

		private float _cycleThumbTimeInSec;

		private bool _canCycleThumb;

		private bool _canAutoOpenWindow;

		public delegate void HmmSelectionDelegate(MainMenuNewsInfoController.NewsHmmSelectionType selectionType, Guid itemTypeId);
	}
}

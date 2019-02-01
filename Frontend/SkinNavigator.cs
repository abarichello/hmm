using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using ModelViewer;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class SkinNavigator
	{
		public int CurrentChosenSkinIndex { get; private set; }

		public int TargetChosenSkinIndex { get; private set; }

		public void Config(HMMHub hub)
		{
			SkinNavigator._hub = hub;
		}

		public void OpenSkinWindow(CollectionScriptableObject collection, Guid characterGuid)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = collection.AllItemTypes[characterGuid];
			CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
			int num = 0;
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<T>)itemTypeScriptableObject.Bag);
			List<Guid> list = collection.CharacterToSkinGuids[itemTypeScriptableObject.Id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject2 = collection.AllItemTypes[list[i]];
				if (!itemTypeScriptableObject2.Deleted)
				{
					bool flag = characterItemTypeBag.DefaultSkinGuid == itemTypeScriptableObject2.Id;
					if (flag || SkinNavigator._hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) || SkinNavigator._hub.User.Inventory.HasItemOfType(itemTypeScriptableObject2.Id))
					{
						SkinPrefabItemTypeComponent component2 = itemTypeScriptableObject2.GetComponent<SkinPrefabItemTypeComponent>();
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.CardPrefab);
						gameObject.SetActive(true);
						gameObject.name = itemTypeScriptableObject2.name;
						gameObject.transform.parent = this.TargetGameObject.transform;
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localScale = this.CardPrefab.transform.localScale;
						SkinConfig component3 = gameObject.GetComponent<SkinConfig>();
						component3.CharacterName = itemTypeScriptableObject.Name;
						component3.Item = itemTypeScriptableObject2;
						component3.IsDefault = flag;
						component3.Eventlistener.IntParameter = num;
						component3.OnHoverOverAction = new Action<int>(this.MoveToChosenSkin);
						component3.SkinSprite.SpriteName = itemTypeScriptableObject2.Name;
						component3.CardSkinName.text = Language.Get(component2.CardSkinDraft, TranslationSheets.Items);
						component3.CharacterNameLabel.text = Language.Get(component.MainAttributes.DraftName, TranslationSheets.CharactersBaseInfo);
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.SkinIndicatorPrefab.gameObject);
						gameObject2.name = i.ToString("000");
						gameObject2.transform.parent = this.SkinIndicatorParentGrid.transform;
						gameObject2.transform.localPosition = Vector3.zero;
						gameObject2.transform.localScale = this.SkinIndicatorPrefab.transform.localScale;
						UI2DSprite component4 = gameObject2.GetComponent<UI2DSprite>();
						this._skinIndicatorList.Add(component4);
						this._skins.Add(component3);
						num++;
					}
				}
			}
			this.SkinIndicatorParentGrid.Reposition();
			GUIUtils.ControllerSetSelectedObject(this.GetCurrentSkinGameObject());
			SkinNavigator._hub.StartCoroutine(this.TryToDisableSkinStatePostponed());
		}

		private IEnumerator TryToDisableSkinStatePostponed()
		{
			yield return null;
			if (this._skins.Count == 1)
			{
				this.DisableSkinState();
			}
			yield break;
		}

		private void UpdateActiveSkinIndicator()
		{
			if (this._activeSkinIndicator != null)
			{
				this._activeSkinIndicator.sprite2D = this.IndicatorBase;
			}
			this._activeSkinIndicator = this._skinIndicatorList[this.CurrentChosenSkinIndex];
			this._activeSkinIndicator.sprite2D = this.IndicatorActive;
		}

		public Guid GetChosenSkinGuid()
		{
			SkinConfig skinConfig = this._skins[this.CurrentChosenSkinIndex];
			if (skinConfig.IsDefault)
			{
				return Guid.Empty;
			}
			return this._skins[this.CurrentChosenSkinIndex].Item.Id;
		}

		public SkinConfig GetCurrentSkinConfig()
		{
			return this._skins[this.CurrentChosenSkinIndex];
		}

		public SkinConfig GetSkinConfig(Guid skin)
		{
			int index = this.SkinIndexPosition(skin.ToString());
			return this._skins[index];
		}

		public GameObject GetCurrentSkinGameObject()
		{
			return this.GetCurrentSkinConfig().CardButton.gameObject;
		}

		public void MoveToChosenSkin(int skinIndex)
		{
			this.TargetChosenSkinIndex = skinIndex;
			this.CardSkinIndexController();
		}

		public void MoveToSkin(string guid)
		{
			this.MoveToChosenSkin(this.SkinIndexPosition(guid));
		}

		public void MoveLeft()
		{
			if (this.TargetChosenSkinIndex <= 0)
			{
				this.TargetChosenSkinIndex = this._skins.Count - 1;
			}
			else
			{
				this.TargetChosenSkinIndex--;
			}
			this.CardSkinIndexController();
		}

		public void MoveRight()
		{
			if (this.TargetChosenSkinIndex >= this._skins.Count - 1)
			{
				this.TargetChosenSkinIndex = 0;
			}
			else
			{
				this.TargetChosenSkinIndex++;
			}
			this.CardSkinIndexController();
		}

		private int SkinIndexPosition(string guid)
		{
			Guid b = new Guid(guid);
			bool flag = b.Equals(Guid.Empty);
			for (int i = 0; i < this._skins.Count; i++)
			{
				Guid id = this._skins[i].Item.Id;
				if (id == b || (flag && this._skins[i].IsDefault))
				{
					return i;
				}
			}
			return 0;
		}

		public void ShowSelectSkin(string guid)
		{
			int num = this.SkinIndexPosition(guid);
			this.CurrentChosenSkinIndex = num;
			this.TargetChosenSkinIndex = num;
			this.UpdateSkinsPosition();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<SkinConfig> ListenToSkinSelectionChanged;

		private void UpdateSkinsPosition()
		{
			if (this.CurrentChosenSkinIndex < 0 || this.CurrentChosenSkinIndex >= this._skins.Count)
			{
				SkinNavigator.Log.WarnFormat("Chosen Skin Index is out of bounds: index: {0}", new object[]
				{
					this.CurrentChosenSkinIndex
				});
				return;
			}
			this._skins[this.CurrentChosenSkinIndex].SkinRarity.SpriteName = "1_skin_Default";
			this._skins[this.CurrentChosenSkinIndex].SkinName.text = Language.Get(string.Format("{0}_name", this._skins[this.CurrentChosenSkinIndex].Item.Name), TranslationSheets.Items);
			this._twoPI = 6.28318548f;
			this._itemsInterval = 1f / (float)this._skins.Count;
			this._targetAngle = -this._itemsInterval * (float)this.CurrentChosenSkinIndex - this.Tunning;
			float f = (float)this._skins.Count * 0.5f;
			int a = Mathf.Abs(this.TargetChosenSkinIndex - this.CurrentChosenSkinIndex) + 1;
			float time = 1f / (float)Mathf.Max(a, 1);
			float duration = this.SkinsRotationCurveSpeed.Evaluate(time);
			for (int i = 0; i < this._skins.Count; i++)
			{
				SkinConfig skinConfig = this._skins[i];
				NGUITools.MarkParentAsChanged(skinConfig.gameObject);
				float f2 = this.StartAngle + this.Curve.Evaluate(this._targetAngle + (float)i * this._itemsInterval) * this._twoPI;
				float num = Mathf.Sin(f2);
				skinConfig.Panel.depth = ((this.CurrentChosenSkinIndex != i) ? (50 - (int)((num + 1f) * Mathf.Abs(f))) : 51);
				TweenPosition tweenPosition = skinConfig.TweenPosition;
				tweenPosition.SetStartToCurrentValue();
				tweenPosition.to = new Vector3(Mathf.Cos(f2) * this.CircleSize, (num + 1f) * (float)this.HeightDiffSize, 0f);
				tweenPosition.duration = duration;
				tweenPosition.ResetToBeginning();
				tweenPosition.PlayForward();
			}
			EventDelegate.Add(this._skins[0].TweenPosition.onFinished, new EventDelegate.Callback(this.CardSkinIndexController), true);
			if (this.ListenToSkinSelectionChanged != null)
			{
				this.ListenToSkinSelectionChanged(this._skins[this.CurrentChosenSkinIndex]);
			}
			this._carModelViewer.ModelName = string.Format("{0}_shop", this._skins[this.CurrentChosenSkinIndex].Item.Name);
			this.UpdateActiveSkinIndicator();
		}

		public void CardSkinIndexController()
		{
			if (this.TargetChosenSkinIndex == this.CurrentChosenSkinIndex)
			{
				return;
			}
			if (this.CurrentChosenSkinIndex < this.TargetChosenSkinIndex)
			{
				bool flag = this.CurrentChosenSkinIndex == 0;
				bool flag2 = this.TargetChosenSkinIndex == this._skins.Count - 1;
				if (flag && flag2)
				{
					this.CurrentChosenSkinIndex = this._skins.Count - 1;
				}
				else
				{
					this.CurrentChosenSkinIndex++;
				}
			}
			else
			{
				bool flag3 = this.CurrentChosenSkinIndex == this._skins.Count - 1;
				bool flag4 = this.TargetChosenSkinIndex == 0;
				if (flag3 && flag4)
				{
					this.CurrentChosenSkinIndex = 0;
				}
				else
				{
					this.CurrentChosenSkinIndex--;
				}
			}
			this.UpdateSkinsPosition();
		}

		public void DisableSkinState()
		{
			for (int i = 0; i < this._skins.Count; i++)
			{
				SkinConfig skinConfig = this._skins[i];
				foreach (UIButton uibutton in skinConfig.CardButton.gameObject.GetComponents<UIButton>())
				{
					uibutton.SetState(UIButtonColor.State.Normal, true);
				}
				skinConfig.CardButton.GetComponent<BoxCollider>().enabled = false;
			}
			this.LeftButton.isEnabled = false;
			this.RightButton.isEnabled = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SkinNavigator));

		public GameObject TargetGameObject;

		public GameObject CardPrefab;

		[SerializeField]
		private BaseModelViewer _carModelViewer;

		public UIButton LeftButton;

		public UIButton RightButton;

		[Header("Skin Indicator")]
		public UIGrid SkinIndicatorParentGrid;

		public Sprite IndicatorBase;

		public Sprite IndicatorActive;

		public GameObject SkinIndicatorPrefab;

		private UI2DSprite _activeSkinIndicator;

		private readonly List<UI2DSprite> _skinIndicatorList = new List<UI2DSprite>();

		[Header("Skin Animation")]
		public float AnimationDuration = 0.5f;

		public float Tunning = 0.25f;

		public AnimationCurve Curve;

		public float StartAngle;

		public float CircleSize = 175f;

		public int HeightDiffSize = 50;

		private float _itemsInterval;

		private float _targetAngle;

		private float _twoPI;

		private static HMMHub _hub;

		private readonly List<SkinConfig> _skins = new List<SkinConfig>();

		public AnimationCurve SkinsRotationCurveSpeed;
	}
}

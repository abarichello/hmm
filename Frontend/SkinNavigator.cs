using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Utils;
using Hoplon.Serialization;
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
			int num = 0;
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<!0>)itemTypeScriptableObject.Bag);
			List<Guid> list = collection.CharacterToSkinGuids[itemTypeScriptableObject.Id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject2 = collection.AllItemTypes[list[i]];
				if (!itemTypeScriptableObject2.Deleted)
				{
					bool flag = characterItemTypeBag.DefaultSkinGuid == itemTypeScriptableObject2.Id;
					if (flag || SkinNavigator._hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) || SkinNavigator._hub.User.Inventory.HasItemOfType(itemTypeScriptableObject2.Id))
					{
						SkinPrefabItemTypeComponent component = itemTypeScriptableObject2.GetComponent<SkinPrefabItemTypeComponent>();
						GameObject gameObject = Object.Instantiate<GameObject>(this.CardPrefab);
						gameObject.SetActive(true);
						gameObject.name = itemTypeScriptableObject2.name;
						gameObject.transform.parent = this.TargetGameObject.transform;
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localScale = this.CardPrefab.transform.localScale;
						SkinConfig component2 = gameObject.GetComponent<SkinConfig>();
						component2.CharacterName = itemTypeScriptableObject.Name;
						component2.Item = itemTypeScriptableObject2;
						component2.IsDefault = flag;
						component2.Eventlistener.IntParameter = num;
						component2.OnHoverOverAction = new Action<int>(this.MoveToChosenSkin);
						component2.SkinSprite.SpriteName = itemTypeScriptableObject2.Name;
						component2.CardSkinName.text = Language.Get(component.CardSkinDraft, TranslationContext.Items);
						GameObject gameObject2 = Object.Instantiate<GameObject>(this.SkinIndicatorPrefab.gameObject);
						gameObject2.name = i.ToString("000");
						gameObject2.transform.parent = this.SkinIndicatorParentGrid.transform;
						gameObject2.transform.localPosition = Vector3.zero;
						gameObject2.transform.localScale = this.SkinIndicatorPrefab.transform.localScale;
						UI2DSprite component3 = gameObject2.GetComponent<UI2DSprite>();
						this._skinIndicatorList.Add(component3);
						this._skins.Add(component2);
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
			this._twoPI = 6.2831855f;
			this._itemsInterval = 1f / (float)this._skins.Count;
			this._targetAngle = -this._itemsInterval * (float)this.CurrentChosenSkinIndex - this.Tunning;
			float num = (float)this._skins.Count * 0.5f;
			int num2 = Mathf.Abs(this.TargetChosenSkinIndex - this.CurrentChosenSkinIndex) + 1;
			float num3 = 1f / (float)Mathf.Max(num2, 1);
			float duration = this.SkinsRotationCurveSpeed.Evaluate(num3);
			for (int i = 0; i < this._skins.Count; i++)
			{
				SkinConfig skinConfig = this._skins[i];
				NGUITools.MarkParentAsChanged(skinConfig.gameObject);
				float num4 = this.StartAngle + this.Curve.Evaluate(this._targetAngle + (float)i * this._itemsInterval) * this._twoPI;
				float num5 = Mathf.Sin(num4);
				skinConfig.Panel.depth = ((this.CurrentChosenSkinIndex != i) ? (50 - (int)((num5 + 1f) * Mathf.Abs(num))) : 51);
				TweenPosition tweenPosition = skinConfig.TweenPosition;
				tweenPosition.SetStartToCurrentValue();
				tweenPosition.to = new Vector3(Mathf.Cos(num4) * this.CircleSize, (num5 + 1f) * (float)this.HeightDiffSize, 0f);
				tweenPosition.duration = duration;
				tweenPosition.ResetToBeginning();
				tweenPosition.PlayForward();
			}
			EventDelegate.Add(this._skins[0].TweenPosition.onFinished, new EventDelegate.Callback(this.CardSkinIndexController), true);
			if (this.ListenToSkinSelectionChanged != null)
			{
				this.ListenToSkinSelectionChanged(this._skins[this.CurrentChosenSkinIndex]);
			}
			InventoryItemTypeComponent component = this._skins[this.CurrentChosenSkinIndex].Item.GetComponent<InventoryItemTypeComponent>();
			this._carModelViewer.ModelName = component.InventoryPreviewName;
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

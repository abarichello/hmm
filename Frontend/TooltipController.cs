using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class TooltipController : GameHubBehaviour
	{
		public void Start()
		{
			this._initialWidth = this.BackgroundUi2DSprite.width;
			this._iconWidth = this.IconUi2DSprite.width;
			this._iconHeight = this.IconUi2DSprite.height;
			this.HideWindow();
		}

		private bool IsVisible()
		{
			return this.TooltipWindowGameObject.activeSelf;
		}

		public void TryToOpenWindow(TooltipInfo tooltipInfo)
		{
			if (this.IsVisible())
			{
				this.HideWindow();
				return;
			}
			this.PopulateTooltip(tooltipInfo);
			this.TooltipWindowGameObject.SetActive(true);
		}

		public void HideWindow()
		{
			this.TooltipWindowGameObject.SetActive(false);
		}

		private void PopulateTooltip(TooltipInfo tooltipInfo)
		{
			if (!string.IsNullOrEmpty(tooltipInfo.SpriteName))
			{
				this.IconUi2DSprite.SpriteName = tooltipInfo.SpriteName;
			}
			else
			{
				this.IconUi2DSprite.sprite2D = tooltipInfo.IconSprite;
			}
			this.IconUi2DSprite.MakePixelPerfect();
			this.IconUi2DSprite.width = Mathf.Min(this.IconUi2DSprite.width, this._iconWidth);
			this.IconUi2DSprite.height = Mathf.Min(this.IconUi2DSprite.height, this._iconHeight);
			this.BorderIcon.gameObject.SetActive(!string.IsNullOrEmpty(tooltipInfo.SpriteName) || tooltipInfo.IconSprite != null);
			this.TitleUiLabel.text = tooltipInfo.Title;
			this.SimpleTextUiLabel.text = tooltipInfo.SimpleText;
			this.DescriptionFullUiLabel.text = tooltipInfo.DescriptionFull;
			this.CooldownDescriptionUiLabel.gameObject.SetActive(!string.IsNullOrEmpty(tooltipInfo.CooldownDescription));
			this.CooldownDescriptionUiLabel.text = tooltipInfo.CooldownDescription;
			this.DescriptionFullUiLabel.TryUpdateText();
			float num = this.DescriptionFullUiLabel.transform.localPosition.y - (float)this.DescriptionFullUiLabel.height - this.BackgroundUi2DSprite.border.y;
			this.DescriptionLineTransform.gameObject.SetActive(true);
			this.ArrowLeftUi2DSprite.gameObject.SetActive(false);
			this.ArrowRightUi2DSprite.gameObject.SetActive(false);
			this.ArrowTopUi2DSprite.gameObject.SetActive(false);
			this.ArrowBotUi2DSprite.gameObject.SetActive(false);
			int num2 = Mathf.Abs(Mathf.RoundToInt(num));
			this.BackgroundUi2DSprite.width = this._initialWidth;
			this.BackgroundUi2DSprite.height = num2;
			if (tooltipInfo.Type == TooltipInfo.TooltipType.SimpleText)
			{
				this.SimpleTextUiLabel.ProcessText();
				this.DescriptionLineTransform.gameObject.SetActive(false);
				this.ArrowLeftUi2DSprite.gameObject.SetActive(false);
				this.BackgroundUi2DSprite.width = this.SimpleTextUiLabel.width + this.SimpleTextBorderWidth;
				this.BackgroundUi2DSprite.height = this.SimpleTextUiLabel.height + this.SimpleTextBorderHeight;
			}
			Vector3 vector = UICamera.mainCamera.WorldToScreenPoint(tooltipInfo.Position);
			UI2DSprite ui2DSprite;
			switch (tooltipInfo.ArrowAnchor)
			{
			case PreferredDirection.Left:
				ui2DSprite = this.ArrowLeftUi2DSprite;
				break;
			case PreferredDirection.Bottom:
				ui2DSprite = this.ArrowBotUi2DSprite;
				break;
			case PreferredDirection.Right:
				ui2DSprite = this.ArrowRightUi2DSprite;
				break;
			case PreferredDirection.Top:
				ui2DSprite = this.ArrowTopUi2DSprite;
				break;
			default:
				ui2DSprite = this.NoneUi2DSprite;
				break;
			}
			ui2DSprite.gameObject.SetActive(true);
			Vector3 localPosition = ui2DSprite.transform.localPosition;
			if (tooltipInfo.ArrowAnchor == PreferredDirection.Right || tooltipInfo.ArrowAnchor == PreferredDirection.Left)
			{
				if (vector.y < (float)num2)
				{
					localPosition.y = num - this.IconUi2DSprite.transform.localPosition.y;
				}
				else
				{
					localPosition.y = this.IconUi2DSprite.transform.localPosition.y;
				}
			}
			else if (tooltipInfo.ArrowAnchor == PreferredDirection.Bottom)
			{
				localPosition.y = -(this.BackgroundUi2DSprite.transform.localPosition.y + (float)this.BackgroundUi2DSprite.height + (float)ui2DSprite.width * 0.5f);
			}
			ui2DSprite.transform.localPosition = localPosition;
			this.TooltipWindowGameObject.transform.position = tooltipInfo.Position;
			Vector3 localPosition2 = this.TooltipWindowGameObject.transform.localPosition;
			switch (tooltipInfo.ArrowAnchor)
			{
			case PreferredDirection.Left:
				localPosition2.x += (float)this.BackgroundUi2DSprite.width * 0.5f + (float)ui2DSprite.height;
				localPosition2.y -= ui2DSprite.transform.localPosition.y;
				break;
			case PreferredDirection.Bottom:
				localPosition2.y += (float)(this.BackgroundUi2DSprite.height + ui2DSprite.width);
				break;
			case PreferredDirection.Right:
				localPosition2.x -= (float)this.BackgroundUi2DSprite.width * 0.5f + (float)ui2DSprite.height;
				localPosition2.y -= ui2DSprite.transform.localPosition.y;
				break;
			case PreferredDirection.Top:
				localPosition2.y -= (float)ui2DSprite.width;
				break;
			}
			this.TooltipWindowGameObject.transform.localPosition = localPosition2;
			Vector3 localPosition3 = this.TooltipWindowGameObject.transform.localPosition;
			Vector3 vector2 = new Vector3(localPosition3.x + (float)this.BackgroundUi2DSprite.width * 0.5f + (float)ui2DSprite.width, localPosition3.y + (float)this.BackgroundUi2DSprite.height * 0.5f + (float)ui2DSprite.height);
			if (vector2.x >= (float)Screen.width)
			{
				Vector3 localPosition4 = this.TooltipWindowGameObject.transform.localPosition;
				localPosition4.x = (float)Screen.width;
				localPosition4.x -= (float)this.BackgroundUi2DSprite.width * 0.5f + (float)ui2DSprite.width;
				this.TooltipWindowGameObject.transform.localPosition = localPosition4;
			}
			if (localPosition3.y >= (float)Screen.height && (tooltipInfo.ArrowAnchor == PreferredDirection.Right || tooltipInfo.ArrowAnchor == PreferredDirection.Left))
			{
				float num3 = localPosition3.y - (float)Screen.height + this.BackgroundUi2DSprite.border.y;
				localPosition.y = num - this.IconUi2DSprite.transform.localPosition.y + num3;
				ui2DSprite.transform.localPosition = localPosition;
				this.TooltipWindowGameObject.transform.position = tooltipInfo.Position;
				localPosition2 = this.TooltipWindowGameObject.transform.localPosition;
				PreferredDirection arrowAnchor = tooltipInfo.ArrowAnchor;
				if (arrowAnchor != PreferredDirection.Right)
				{
					if (arrowAnchor == PreferredDirection.Left)
					{
						localPosition2.x += (float)this.BackgroundUi2DSprite.width * 0.5f + (float)ui2DSprite.height;
						localPosition2.y -= ui2DSprite.transform.localPosition.y;
					}
				}
				else
				{
					localPosition2.x -= (float)this.BackgroundUi2DSprite.width * 0.5f + (float)ui2DSprite.height;
					localPosition2.y -= ui2DSprite.transform.localPosition.y;
				}
				this.TooltipWindowGameObject.transform.localPosition = localPosition2;
			}
		}

		public GameObject TooltipWindowGameObject;

		public int SimpleTextBorderWidth = 20;

		public int SimpleTextBorderHeight = 30;

		public UI2DSprite BackgroundUi2DSprite;

		public HMMUI2DDynamicSprite IconUi2DSprite;

		public HMMUI2DDynamicSprite BorderIcon;

		public UILabel SimpleTextUiLabel;

		public UILabel TitleUiLabel;

		public Transform DescriptionLineTransform;

		public UILabel DescriptionFullUiLabel;

		public UILabel CooldownDescriptionUiLabel;

		public UI2DSprite ArrowLeftUi2DSprite;

		public UI2DSprite ArrowRightUi2DSprite;

		public UI2DSprite ArrowTopUi2DSprite;

		public UI2DSprite ArrowBotUi2DSprite;

		public UI2DSprite NoneUi2DSprite;

		private int _initialWidth;

		private int _iconWidth;

		private int _iconHeight;
	}
}

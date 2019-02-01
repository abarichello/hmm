using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeTitleController : MonoBehaviour
	{
		private void Start()
		{
			this.SetStage(1);
		}

		public void SetStage(int stage)
		{
			switch (stage)
			{
			case 1:
				this.TitleLabel.text = Language.Get(this.Stage1Draft, TranslationSheets.MainMenuGui);
				this.stage1_background.sprite2D = this.ActiveSprite;
				this.stage1_check.SetActive(false);
				this.stage1_label.SetActive(true);
				this.stage2_check.SetActive(false);
				this.stage3_check.SetActive(false);
				this.stage4_activated.SetActive(false);
				break;
			case 2:
				this.TitleLabel.text = Language.Get(this.Stage2Draft, TranslationSheets.MainMenuGui);
				this.stage1_background.sprite2D = this.DeActiveSprite;
				this.stage1_check.SetActive(true);
				this.stage1_label.SetActive(false);
				this.stage2_background.sprite2D = this.ActiveSprite;
				this.stage2_check.SetActive(false);
				this.stage2_label.SetActive(true);
				this.stage4_activated.SetActive(false);
				break;
			case 3:
				this.TitleLabel.text = Language.Get(this.Stage3Draft, TranslationSheets.MainMenuGui);
				this.stage1_background.sprite2D = this.DeActiveSprite;
				this.stage1_check.SetActive(true);
				this.stage1_label.SetActive(false);
				this.stage2_background.sprite2D = this.DeActiveSprite;
				this.stage2_check.SetActive(true);
				this.stage2_label.SetActive(false);
				this.stage3_background.sprite2D = this.ActiveSprite;
				this.stage3_check.SetActive(false);
				this.stage3_label.SetActive(true);
				this.stage4_activated.SetActive(false);
				break;
			case 4:
				this.TitleLabel.text = Language.Get(this.Stage4Draft, TranslationSheets.MainMenuGui);
				this.stage1_background.sprite2D = this.DeActiveSprite;
				this.stage1_check.SetActive(true);
				this.stage1_label.SetActive(false);
				this.stage2_background.sprite2D = this.DeActiveSprite;
				this.stage2_check.SetActive(true);
				this.stage2_label.SetActive(false);
				this.stage3_background.sprite2D = this.DeActiveSprite;
				this.stage3_check.SetActive(true);
				this.stage3_label.SetActive(false);
				this.stage4_activated.SetActive(true);
				break;
			}
		}

		public UILabel TitleLabel;

		public string Stage1Draft;

		public string Stage2Draft;

		public string Stage3Draft;

		public string Stage4Draft;

		public UI2DSprite stage1_background;

		public GameObject stage1_check;

		public GameObject stage1_label;

		public UI2DSprite stage2_background;

		public GameObject stage2_check;

		public GameObject stage2_label;

		public UI2DSprite stage3_background;

		public GameObject stage3_check;

		public GameObject stage3_label;

		public UI2DSprite stage4_background;

		public GameObject stage4_label;

		public GameObject stage4_activated;

		public Sprite ActiveSprite;

		public Sprite DeActiveSprite;
	}
}

using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUILabelDelegate : MonoBehaviour
	{
		public string text
		{
			get
			{
				return this._text;
			}
			set
			{
				this.ChangeText(this._text);
				this._text = value;
			}
		}

		private void Start()
		{
			this._selfLabel = base.GetComponent<UILabel>();
			this.ChangeText(this._text);
		}

		private void ChangeText(string pText)
		{
			if (this._selfLabel != null)
			{
				this._selfLabel.text = pText;
				this._selfLabel.MarkAsChanged();
			}
			if (this.assignedLabels != null)
			{
				for (int i = 0; i < this.assignedLabels.Length; i++)
				{
					UILabel uilabel = this.assignedLabels[i];
					if (uilabel == null)
					{
						return;
					}
					uilabel.text = pText;
					uilabel.MarkAsChanged();
				}
			}
		}

		[SerializeField]
		[TextArea(3, 20)]
		private string _text;

		private UILabel _selfLabel;

		public UILabel[] assignedLabels;
	}
}

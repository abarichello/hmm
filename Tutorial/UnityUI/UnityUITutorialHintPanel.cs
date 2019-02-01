using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	public class UnityUITutorialHintPanel : MonoBehaviour, ITutorialPanelView
	{
		protected void Awake()
		{
			TutorialUIController tutorialUIController = UnityEngine.Object.FindObjectOfType<TutorialUIController>();
			this._tutorialHintComponent = tutorialUIController.HintPanelComponent;
		}

		protected void Start()
		{
			this._tutorialHintComponent.RegisterView(this);
		}

		protected void OnEnable()
		{
			this._tutorialHintComponent.RegisterEvents();
		}

		protected void OnDisable()
		{
			this._tutorialHintComponent.UnregisterEvents();
		}

		public void SetVisibility(bool visibility)
		{
			this._hintText.enabled = visibility;
		}

		private ITutorialComponent _tutorialHintComponent;

		[SerializeField]
		private Text _hintText;
	}
}

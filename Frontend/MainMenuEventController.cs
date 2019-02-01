using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuEventController : MonoBehaviour
	{
		private void OnEnable()
		{
			Debug.Log("EVENT WINDOW - onenable");
			this.Eventstatus = MainMenuEventController.EventStatus.PreEvent;
			this._currentTime = DateTime.Now;
			this._startTime = new DateTime(this.StartDate.Year, this.StartDate.Month, this.StartDate.Day, this.StartDate.Hour, this.StartDate.Minute, this.StartDate.Seconde);
			this._endTime = new DateTime(this.EndDate.Year, this.EndDate.Month, this.EndDate.Day, this.EndDate.Hour, this.EndDate.Minute, this.EndDate.Seconde);
			this._remaingTime = this._startTime - this._currentTime;
			if (this._remaingTime.TotalSeconds > 0.0)
			{
				this.Eventstatus = MainMenuEventController.EventStatus.PreEvent;
			}
			else
			{
				this._remaingTime = this._endTime - this._currentTime;
				if (this._remaingTime.TotalSeconds > 0.0)
				{
					this.Eventstatus = MainMenuEventController.EventStatus.OnEvent;
				}
				else
				{
					this.Eventstatus = MainMenuEventController.EventStatus.EventFinished;
				}
			}
			this._remaingTotalsecondes = (float)this._remaingTime.TotalSeconds;
			if (this.Eventstatus != MainMenuEventController.EventStatus.EventFinished)
			{
				this.EnableEventWindow(this.Eventstatus);
			}
			else
			{
				this.DisableEventWindow();
			}
		}

		private void EnableEventWindow(MainMenuEventController.EventStatus Eventstatus)
		{
			if (Eventstatus != MainMenuEventController.EventStatus.PreEvent)
			{
				if (Eventstatus == MainMenuEventController.EventStatus.OnEvent)
				{
					this.LightOne.GetComponent<UITexture>().mainTexture = this.LightEnabled;
					this.LightTwo.GetComponent<UITexture>().mainTexture = this.LightEnabled;
					this.CountDownLabel.color = this.OnEventColor;
					this.EventWindowTitleLabel_Started.enabled = true;
					this.EventWindowTitleLabel_Notstarted.enabled = false;
					this.EventWindowCountDownLabel.color = this.OnEventColor;
					this.EventWindow.SetActive(true);
					this.EventNearOrHapenning = true;
				}
			}
			else
			{
				this.LightOne.GetComponent<UITexture>().mainTexture = this.LightDisabled;
				this.LightTwo.GetComponent<UITexture>().mainTexture = this.LightDisabled;
				this.CountDownLabel.color = this.PreEventColor;
				this.EventWindowTitleLabel_Started.enabled = false;
				this.EventWindowTitleLabel_Notstarted.enabled = true;
				this.EventWindowCountDownLabel.color = this.PreEventColor;
				this.EventWindow.SetActive(true);
				this.EventNearOrHapenning = true;
			}
		}

		private void DisableEventWindow()
		{
			this.EventNearOrHapenning = false;
			this.EventWindow.SetActive(false);
		}

		private void Update()
		{
			if (!this.EventNearOrHapenning)
			{
				return;
			}
			this.remainingdays = this._remaingTime.Days;
			this.remaininghours = this._remaingTime.Hours;
			this.remainingminutes = this._remaingTime.Minutes;
			this.remainingsecondes = this._remaingTime.Seconds;
			string text = (this._remaingTime.Days * 24 + this._remaingTime.Hours).ToString();
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			string text2 = this._remaingTime.Minutes.ToString();
			if (text2.Length < 2)
			{
				text2 = "0" + text2;
			}
			string text3 = this.remainingsecondes.ToString();
			if (text3.Length < 2)
			{
				text3 = "0" + text3;
			}
			this.CountDownLabel.text = string.Concat(new string[]
			{
				text,
				":",
				text2,
				":",
				text3
			});
			this.EventWindowCountDownLabel.text = string.Concat(new string[]
			{
				text,
				":",
				text2,
				":",
				text3
			});
			this._remaingTime -= TimeSpan.FromSeconds((double)Time.deltaTime);
			this._remaingTotalsecondes = (float)this._remaingTime.TotalSeconds;
			if (this._remaingTotalsecondes < 0f)
			{
				this.OnEnable();
			}
		}

		public GameObject TopAdvisor;

		public UILabel TitleLabel;

		public UILabel CountDownLabel;

		public MainMenuEventController.EventDate StartDate;

		public MainMenuEventController.EventDate EndDate;

		public MainMenuEventController.EventStatus Eventstatus;

		public GameObject EventWindow;

		public UILabel EventWindowTitleLabel_Started;

		public UILabel EventWindowTitleLabel_Notstarted;

		public UILabel EventWindowCountDownLabel;

		public GameObject LightOne;

		public GameObject LightTwo;

		public Texture LightEnabled;

		public Texture LightDisabled;

		public Color PreEventColor;

		public Color OnEventColor;

		public bool EventNearOrHapenning;

		public DateTime _currentTime;

		public DateTime _startTime;

		public DateTime _endTime;

		public TimeSpan _remaingTime;

		public float _remaingTotalsecondes;

		public int remainingdays;

		public int remaininghours;

		public int remainingminutes;

		public int remainingsecondes;

		public bool editorDebug;

		[Serializable]
		public class EventDate
		{
			public int Month;

			public int Day;

			public int Year;

			public int Hour;

			public int Minute;

			public int Seconde;
		}

		public enum EventStatus
		{
			PreEvent,
			OnEvent,
			EventFinished
		}
	}
}

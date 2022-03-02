using System;
using System.Collections.Generic;
using System.Linq;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.HMMChat
{
	public class ChatTabController : GameHubBehaviour
	{
		public void Configure()
		{
			this.groupTabButtonReference.SetKind(true);
			this.groupTabButtonReference.IsGroup = true;
			this.SetFocusOnGroupTab(true);
			this.ShowTabs();
			this.ConfigureNavigationUnreadMessageButtons();
		}

		public void CreateNewTab(string universalId, string chatname)
		{
			ChatTabButtonReference component = this.Grid.gameObject.AddChild(this.TabPrefab).GetComponent<ChatTabButtonReference>();
			component.gameObject.name = chatname;
			component.TabNameLabel.text = chatname;
			component.Listener.StringParameter = universalId;
			component.CloseButtonListener.StringParameter = universalId;
			component.SetKind(false);
			this.Tabs.Add(universalId, component);
			this.ShowTabs();
			this.ConfigureNavigationUnreadMessageButtons();
		}

		public void TabClicked(string universalId)
		{
		}

		public void GroupTabClicked()
		{
		}

		public void SetFocusOnTab(string universalid, bool focus)
		{
			ChatTabButtonReference chatTabButtonReference;
			if (!this.Tabs.TryGetValue(universalid, out chatTabButtonReference))
			{
				return;
			}
			if (focus)
			{
				chatTabButtonReference.SetFocus();
				this.currentChatTabButtonReference = chatTabButtonReference;
			}
			else
			{
				chatTabButtonReference.SetNormal();
			}
		}

		public void SetFocusOnGroupTab(bool focus)
		{
			if (focus)
			{
				this.groupTabButtonReference.SetFocus();
				this.currentChatTabButtonReference = this.groupTabButtonReference;
			}
			else
			{
				this.groupTabButtonReference.SetNormal();
			}
		}

		public void SetUnreadMessagesFX(string universalid, bool isenabled)
		{
			ChatTabButtonReference chatTabButtonReference;
			this.Tabs.TryGetValue(universalid, out chatTabButtonReference);
			chatTabButtonReference.SetHasUnreadMessage(isenabled);
		}

		public void SetUnreadMessagesFXForGroup(bool isenabled)
		{
			this.groupTabButtonReference.SetHasUnreadMessage(isenabled);
		}

		public void SetOnline(string universalid, bool online, bool withfocus)
		{
			if (!online)
			{
				ChatTabButtonReference chatTabButtonReference;
				this.Tabs.TryGetValue(universalid, out chatTabButtonReference);
				chatTabButtonReference.SetOffLine();
			}
			else
			{
				ChatTabButtonReference chatTabButtonReference2;
				this.Tabs.TryGetValue(universalid, out chatTabButtonReference2);
				chatTabButtonReference2.SetOnLine();
				if (withfocus)
				{
					chatTabButtonReference2.SetFocus();
				}
				else
				{
					chatTabButtonReference2.SetNormal();
				}
			}
		}

		public void CloseTabButtonClicked(string universalid)
		{
		}

		public void HideTab(string universalid)
		{
			ChatTabButtonReference chatTabButtonReference;
			this.Tabs.TryGetValue(universalid, out chatTabButtonReference);
			this.Tabs.Remove(universalid);
			Object.Destroy(chatTabButtonReference.gameObject);
			this.ShowTabs();
			this.ConfigureNavigationUnreadMessageButtons();
		}

		private void ShowTabs()
		{
			List<ChatTabButtonReference> list = this.Tabs.Values.ToList<ChatTabButtonReference>();
			for (int i = 0; i < list.Count; i++)
			{
				if (i >= this.firstTabIndex && i < this.firstTabIndex + 4)
				{
					list[i].gameObject.SetActive(true);
				}
				else
				{
					list[i].gameObject.SetActive(false);
				}
			}
			this.Grid.repositionNow = true;
		}

		private void ConfigureNavigationUnreadMessageButtons()
		{
			if (this.Tabs.Count <= this.MaxVisibleTabs)
			{
				this.LeftNavigationGroup.SetActive(false);
				this.RightNavigationGroup.SetActive(false);
			}
			else
			{
				if (this.firstTabIndex > 0)
				{
					this.LeftNavigationGroup.SetActive(true);
				}
				else
				{
					this.LeftNavigationGroup.SetActive(false);
				}
				if (this.Tabs.Count - 1 > this.firstTabIndex + 3)
				{
					this.RightNavigationGroup.SetActive(true);
				}
				else
				{
					this.RightNavigationGroup.SetActive(false);
				}
			}
		}

		public void NavigateToLeft()
		{
			this.firstTabIndex--;
			this.ShowTabs();
			this.ConfigureNavigationUnreadMessageButtons();
		}

		public void NavigateToRight()
		{
			this.firstTabIndex++;
			this.ShowTabs();
			this.ConfigureNavigationUnreadMessageButtons();
		}

		[Header("EXTERNAL")]
		[Header("INTERNAL")]
		public GameObject LeftNavigationGroup;

		public GameObject LeftNavigationUnreadMessageAdvisor;

		public GameObject RightNavigationGroup;

		public GameObject RightNavigationUnreadMessageAdvisor;

		public UIGrid Grid;

		public int MaxVisibleTabs = 5;

		[Header("TABs")]
		public GameObject TabPrefab;

		public Dictionary<string, ChatTabButtonReference> Tabs = new Dictionary<string, ChatTabButtonReference>();

		public int firstTabIndex;

		public ChatTabButtonReference groupTabButtonReference;

		public ChatTabButtonReference currentChatTabButtonReference;
	}
}

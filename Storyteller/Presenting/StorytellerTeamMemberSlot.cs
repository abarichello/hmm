using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class StorytellerTeamMemberSlot : MonoBehaviour, IStorytellerTeamMemberSlot
	{
		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public IActivatable PsnIdIconActivatable
		{
			get
			{
				return this._psnIdGroupGameObjectActivatable;
			}
		}

		public ILabel PsnIdLabel
		{
			get
			{
				return this._psnIdLabel;
			}
		}

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private GameObjectActivatable _psnIdGroupGameObjectActivatable;

		[SerializeField]
		private UnityLabel _psnIdLabel;
	}
}

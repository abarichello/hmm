using System;
using System.Diagnostics;

namespace HeavyMetalMachines.VFX
{
	public class MessageHintGuiItem : BaseHintGuiItem<MessageHintGuiItem, BaseHintContent>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event MessageHintGuiItem.MessageHintClickDelegate EvtMessageHintClicked;

		public virtual void OnButtonClick_HintClicked()
		{
			if (string.IsNullOrEmpty(base.ReferenceObject.OwnerId))
			{
				return;
			}
			if (MessageHintGuiItem.EvtMessageHintClicked != null)
			{
				MessageHintGuiItem.EvtMessageHintClicked(base.ReferenceObject.OwnerId);
			}
		}

		public delegate void MessageHintClickDelegate(string ownerId);
	}
}

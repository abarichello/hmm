using System;
using HeavyMetalMachines.Frontend;
using Hoplon.GadgetScript;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Social
{
	public class SendChatMessageBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (!ihmmgadgetContext.IsClient)
			{
				((IHMMEventContext)eventContext).SendToClient();
				return this._nextBlock;
			}
			if (ihmmgadgetContext.Owner.IsLocalPlayer)
			{
				this.SendMessage(gadgetContext);
			}
			return this._nextBlock;
		}

		private void SendMessage(IGadgetContext gadgetContext)
		{
			string value = this._draft.GetValue(gadgetContext);
			ContextTag contextTag = (ContextTag)this._draftContext.GetValue(gadgetContext);
			string[] parameters = this.GetParameters(gadgetContext);
			if (this._type == ChatMessageType.AsLocalPlayerNotification)
			{
				string formatted = Language.MainTranslatedLanguage.GetFormatted(value, contextTag, parameters);
				this.SendLocalNotification(formatted);
			}
			else
			{
				this.SendChatMessage(value, contextTag, parameters);
			}
		}

		private void SendChatMessage(string draft, ContextTag context, string[] messageParameters)
		{
			bool toTeam = this._type == ChatMessageType.ToPlayersOnSameTeam;
			GameHubBehaviour.Hub.Chat.SendDraftMessage(toTeam, draft, context, messageParameters);
		}

		private void SendLocalNotification(string localizedMessage)
		{
			GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>().HudChatController.AddChatMessage(localizedMessage);
		}

		private string[] GetParameters(IGadgetContext context)
		{
			string[] array = new string[this._messageParameters.Length];
			for (int i = 0; i < this._messageParameters.Length; i++)
			{
				array[i] = this._messageParameters[i].GetValue(context);
			}
			return array;
		}

		[Header("Configuration")]
		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private StringParameter _draft;

		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private StringParameter _draftContext;

		[SerializeField]
		private ChatMessageType _type;

		[SerializeField]
		private StringParameter[] _messageParameters;
	}
}

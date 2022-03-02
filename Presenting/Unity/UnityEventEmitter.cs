using System;
using System.Diagnostics;
using System.Linq;
using Hoplon.Logging;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Presenting.Unity
{
	public class UnityEventEmitter : MonoBehaviour, IEventEmitter
	{
		public Event[] Events
		{
			get
			{
				return this._events;
			}
		}

		public void Emit(string eventName)
		{
			Event @event = this.GetEvent(eventName);
			if (@event == null)
			{
				return;
			}
			foreach (UnityAnimation unityAnimation in @event.Animations)
			{
				ObservableExtensions.Subscribe<Unit>(unityAnimation.Play());
			}
			this._logger.DebugFormat("{0}: emitted event '{1}'.", new object[]
			{
				base.gameObject.name,
				eventName
			});
		}

		public void EmitInstant(string eventName)
		{
			Event @event = this.GetEvent(eventName);
			if (@event == null)
			{
				return;
			}
			foreach (UnityAnimation unityAnimation in @event.Animations)
			{
				unityAnimation.ResetToLastFrame();
			}
			this._logger.DebugFormat("{0}: emitted event (instant) '{1}'.", new object[]
			{
				base.gameObject.name,
				eventName
			});
		}

		private Event GetEvent(string eventName)
		{
			Event @event = this._events.FirstOrDefault((Event e) => e.Name == eventName);
			if (@event == null)
			{
				return null;
			}
			if (@event.Animations.Length == 0)
			{
				return null;
			}
			return @event;
		}

		[Conditional("AllowHacks")]
		private void LogEventNotConfigured(string eventName)
		{
			this._logger.WarnFormat("{0}: emitted event '{1}' but it is not configured.", new object[]
			{
				base.gameObject.name,
				eventName
			});
		}

		[Conditional("AllowHacks")]
		private void LogEventWithEmptyAnimations(string eventName)
		{
			this._logger.WarnFormat("{0}: emitted event '{1}' but it has an empty list of animations.", new object[]
			{
				base.gameObject.name,
				eventName
			});
		}

		[Inject]
		private ILogger<UnityEventEmitter> _logger;

		[SerializeField]
		private Event[] _events;
	}
}

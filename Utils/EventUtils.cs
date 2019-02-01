using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class EventUtils
	{
		public static void DebugDelegateInvocationList(Delegate del)
		{
			Debug.LogError(string.Format("DebugDelegateInvocationList - START - {0}", del));
			Delegate[] invocationList = del.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Delegate @delegate = invocationList[i];
				Debug.LogError(string.Format(" => DebugDelegateInvocationList - {0}[{1}] - [{2}].[{3}]", new object[]
				{
					del,
					i,
					@delegate.Method.ReflectedType,
					@delegate.Method
				}));
			}
			Debug.LogError(string.Format("DebugDelegateInvocationList - END - {0}", del));
		}

		internal struct SActionCallbackConcainer<TOperationType, TParameterType>
		{
			public SActionCallbackConcainer(Action<TParameterType> oCallback)
			{
				this.m_oForwardCallback = oCallback;
				this.m_oOperationContainer = default(TOperationType);
				EventUtils.SActionCallbackConcainer<TOperationType, TParameterType>.m_cCallbacks.Add(this);
			}

			public void Set(TOperationType oOperation)
			{
				this.m_oOperationContainer = oOperation;
			}

			public void OnAction(TParameterType oParameter)
			{
				this.m_oForwardCallback(oParameter);
				EventUtils.SActionCallbackConcainer<TOperationType, TParameterType>.m_cCallbacks.Remove(this);
				this.m_oOperationContainer = default(TOperationType);
			}

			public TOperationType m_oOperationContainer;

			public Action<TParameterType> m_oForwardCallback;

			private static HashSet<EventUtils.SActionCallbackConcainer<TOperationType, TParameterType>> m_cCallbacks = new HashSet<EventUtils.SActionCallbackConcainer<TOperationType, TParameterType>>();
		}
	}
}

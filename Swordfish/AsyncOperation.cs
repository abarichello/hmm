using System;
using System.Collections.Generic;
using HeavyMetalMachines.DataTransferObjects.Result;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	[Serializable]
	public abstract class AsyncOperation : GameHubObject
	{
		protected AsyncOperation(Action<NetResult> callback)
		{
			AsyncOperation.Operations.Add(this);
			this.Result = new NetResult
			{
				Success = false
			};
			this.Callback = callback;
		}

		protected abstract string ErrorLog { get; }

		protected abstract string ErrorMsg { get; }

		protected abstract string SuccessMsg { get; }

		protected void Destroy()
		{
			if (this.Callback != null)
			{
				this.Callback(this.Result);
			}
			AsyncOperation.Operations.Remove(this);
		}

		protected void OnError(object state, Exception exception)
		{
			AsyncOperation.Log.Fatal(this.ErrorLog, exception);
			this.Result.Error = -666;
			this.Result.Msg = this.ErrorMsg;
			this.Destroy();
		}

		protected void OnSuccess(object state, long l)
		{
			this.Result.Success = true;
			this.Result.Error = 0;
			this.Result.Msg = this.SuccessMsg;
			this.Destroy();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(AsyncOperation));

		public static List<AsyncOperation> Operations = new List<AsyncOperation>(20);

		protected Action<NetResult> Callback;

		protected NetResult Result;
	}
}

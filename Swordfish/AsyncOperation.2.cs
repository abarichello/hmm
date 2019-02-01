using System;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	[Serializable]
	public abstract class AsyncOperation<T>
	{
		protected AsyncOperation(Action<T> callback)
		{
			this.Callback = callback;
		}

		protected abstract string ErrorLog { get; }

		protected void Destroy()
		{
			if (this.Callback != null)
			{
				this.Callback(this.Result);
			}
		}

		protected void OnError(object state, Exception exception)
		{
			AsyncOperation<T>.Log.Fatal(this.ErrorLog, exception);
			this.Destroy();
		}

		protected void OnSuccess()
		{
			this.Destroy();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(AsyncOperation<T>));

		protected T Result;

		protected Action<T> Callback;
	}
}

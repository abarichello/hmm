using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public abstract class ModalGUIController : BaseGUIController, IHudWindow
	{
		public int ModalGroupID
		{
			get
			{
				return this._modalGroupID;
			}
		}

		public Future<object> CurrentFuture
		{
			get
			{
				return this._currentFuture;
			}
		}

		protected virtual void Start()
		{
			if (this._registerAsAvailableOnEnable && SingletonMonoBehaviour<PanelController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<PanelController>.Instance.RegisterAvailableModalWindow(this);
			}
		}

		protected virtual void OnDestroy()
		{
			if (this._registerAsAvailableOnEnable && SingletonMonoBehaviour<PanelController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<PanelController>.Instance.UnregisterAvailableModalWindow(this);
			}
			if (HudWindowManager.Instance != null)
			{
				HudWindowManager.Instance.Remove(this);
			}
		}

		private void ResetBaseValues()
		{
			this.IsResolved = false;
			this._resolvingWindow = false;
			this.IgnoreCurrentWindowResolution = false;
			this.ResolutionWasForced = false;
		}

		public Future<object> InitModalWindow()
		{
			SingletonMonoBehaviour<PanelController>.Instance.RegisterActiveModalWindow(this);
			this._currentFuture = new Future<object>();
			this.ResetBaseValues();
			if (this.CloseButton)
			{
				GUIEventListener guieventListener = this.CloseButton.gameObject.GetComponent<GUIEventListener>() ?? this.CloseButton.gameObject.AddComponent<GUIEventListener>();
				guieventListener.TargetGameObject = base.gameObject;
				guieventListener.MethodName = "ResolveModalWindow";
			}
			this.InitDialogTasks();
			HudWindowManager.Instance.Push(this);
			base.gameObject.SetActive(true);
			return this._currentFuture;
		}

		protected abstract void InitDialogTasks();

		public void ForceResolveModalWindow(bool stateTransition = false)
		{
			if (this._resolvingWindow && !stateTransition)
			{
				return;
			}
			this.ResolutionWasForced = true;
			this.ResolveModalWindow(stateTransition);
		}

		protected void ResolveModalWindow()
		{
			this.ResolveModalWindow(false);
		}

		protected void ResolveModalWindow(bool stateTransition)
		{
			if (HMMHub.IsEditorLeavingPlayMode())
			{
				return;
			}
			if (this._resolvingWindow && !stateTransition)
			{
				return;
			}
			this._resolvingWindow = true;
			if (SingletonMonoBehaviour<PanelController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<PanelController>.Instance.UnregisterActiveModalWindow(this);
			}
			MonoBehaviour monoBehaviour = null;
			if (SingletonMonoBehaviour<PanelController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<PanelController>.Instance.UnregisterActiveModalWindow(this);
				monoBehaviour = SingletonMonoBehaviour<PanelController>.Instance;
			}
			else if (base.gameObject.activeInHierarchy)
			{
				monoBehaviour = this;
			}
			if (monoBehaviour != null)
			{
				monoBehaviour.StartCoroutine(this.ResolveModalWindowCoRoutine(stateTransition));
			}
			HudWindowManager.Instance.Remove(this);
		}

		private IEnumerator ResolveModalWindowCoRoutine(bool stateTransition = false)
		{
			if (!stateTransition)
			{
				yield return base.StartCoroutine(this.ResolveModalWindowTasks());
			}
			if (!stateTransition && this.IgnoreCurrentWindowResolution)
			{
				Debug.LogWarning(string.Format("Ignored Resolution for Panel: {0}", base.gameObject.name));
				this.ResolutionWasForced = false;
				this.IgnoreCurrentWindowResolution = false;
				this._resolvingWindow = false;
				yield break;
			}
			this.IsResolved = true;
			if (this._currentFuture == null)
			{
				Debug.LogWarning(string.Format("Future for Modal Window \"{0}\" is null! Why???", base.gameObject.name));
			}
			else
			{
				this._currentFuture.Result = null;
			}
			SingletonMonoBehaviour<PanelController>.Instance.UnregisterActiveModalWindow(this);
			if (this.DestroyOnScreenResolution)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				base.Panel.alpha = 0f;
			}
			yield break;
		}

		protected abstract IEnumerator ResolveModalWindowTasks();

		protected virtual void Update()
		{
		}

		public int GetHighestPanelDepth()
		{
			UIPanel[] componentsInChildren = base.GetComponentsInChildren<UIPanel>();
			int num = -10;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				num = Mathf.Max(num, componentsInChildren[i].depth);
			}
			return num;
		}

		public int GetLowestPanelDepth()
		{
			UIPanel[] componentsInChildren = base.GetComponentsInChildren<UIPanel>();
			int num = 10;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				num = Mathf.Min(num, componentsInChildren[i].depth);
			}
			return num;
		}

		public virtual bool CanBeHiddenByEscKey()
		{
			return true;
		}

		public virtual void ChangeWindowVisibility(bool targetVisibleState)
		{
			if (base.gameObject.activeSelf == targetVisibleState)
			{
				return;
			}
			if (targetVisibleState)
			{
				this.InitModalWindow();
				return;
			}
			this.ResolveModalWindow();
		}

		public virtual bool IsWindowVisible()
		{
			return base.gameObject.activeSelf && (base.Panel == null || base.Panel.alpha > 0f);
		}

		public int GetDepth()
		{
			return (!(base.Panel == null)) ? base.Panel.depth : 0;
		}

		public virtual bool IsStackableWithType(Type type)
		{
			return true;
		}

		public virtual bool CanOpen()
		{
			return true;
		}

		[SerializeField]
		private bool _registerAsAvailableOnEnable;

		[Header("If one or more modal screens share the same ID the previous one will be closed when another is opened. \"-1\" = no group")]
		[SerializeField]
		[Tooltip("\"Highlander\" window")]
		private int _modalGroupID = -1;

		public UIButton CloseButton;

		private Future<object> _currentFuture;

		protected bool IsResolved;

		protected bool IgnoreCurrentWindowResolution;

		protected bool ResolutionWasForced;

		private bool _resolvingWindow;
	}
}

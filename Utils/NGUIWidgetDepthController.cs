using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIWidgetDepthController : MonoBehaviour, ISerializationCallbackReceiver
	{
		public List<UIWidget> widgets { get; private set; }

		public List<NGUIWidgetDepthController> depthControllers { get; private set; }

		private void OnEnable()
		{
			this.GetWidgets();
			this.FindDepths();
		}

		public void GetWidgets()
		{
			if (this.includeOnlyImmediateChildren)
			{
				if (this.widgets == null)
				{
					this.widgets = new List<UIWidget>();
				}
				this.widgets.Clear();
				if (this.depthControllers == null)
				{
					this.depthControllers = new List<NGUIWidgetDepthController>();
				}
				this.depthControllers.Clear();
				this.GetDepthOrWidgetFor(base.transform);
				IEnumerator enumerator = base.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform pTransform = (Transform)obj;
						this.GetDepthOrWidgetFor(pTransform);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			else
			{
				this.widgets = new List<UIWidget>(base.GetComponentsInChildren<UIWidget>(this.includeInnactive));
				this.depthControllers = new List<NGUIWidgetDepthController>(base.GetComponentsInChildren<NGUIWidgetDepthController>(this.includeInnactive));
			}
		}

		private void GetDepthOrWidgetFor(Transform pTransform)
		{
			NGUIWidgetDepthController component = pTransform.GetComponent<NGUIWidgetDepthController>();
			if (component != null && component != this)
			{
				this.depthControllers.Add(component);
			}
			else
			{
				UIWidget component2 = pTransform.GetComponent<UIWidget>();
				if (component2 != null)
				{
					this.widgets.Add(component2);
				}
			}
		}

		public void FindDepths()
		{
			this.FindDepthsForControllers();
			this.FindDepthsForWidgets();
		}

		private void FindDepthsForControllers()
		{
			if (this.depthControllers == null || this.serializedControllerDepths == null)
			{
				return;
			}
			bool flag = false;
			if (this.depthControllers.Count > this.serializedControllerDepths.Count)
			{
				Debug.Log(string.Format("Recalculating depthController: {0} != {1}", this.depthControllers.Count, this.serializedControllerDepths.Count), base.gameObject);
				flag = true;
				this.serializedControllerDepths.Clear();
				this.controllersMap.Clear();
			}
			for (int i = 0; i < this.depthControllers.Count; i++)
			{
				NGUIWidgetDepthController nguiwidgetDepthController = this.depthControllers[i];
				if (nguiwidgetDepthController == null)
				{
					Debug.LogWarning(string.Format("Invalid depthControllers for index '{0}', removing it.", i), base.gameObject);
				}
				else if (flag)
				{
					this.controllersMap[nguiwidgetDepthController] = nguiwidgetDepthController.depth;
					this.serializedControllerDepths.Add(nguiwidgetDepthController.depth);
				}
				else
				{
					this.controllersMap[nguiwidgetDepthController] = this.serializedControllerDepths[i];
				}
			}
		}

		private void FindDepthsForWidgets()
		{
			if (this.widgets == null || this.serializedWidgetDepths == null)
			{
				return;
			}
			bool flag = false;
			if (this.widgets.Count > this.serializedWidgetDepths.Count)
			{
				Debug.Log(string.Format("Recalculating widgets depth: {0} != {1}", this.widgets.Count, this.serializedWidgetDepths.Count), base.gameObject);
				flag = true;
				this.serializedWidgetDepths.Clear();
				this.widgetsMap.Clear();
			}
			for (int i = 0; i < this.widgets.Count; i++)
			{
				UIWidget uiwidget = this.widgets[i];
				if (uiwidget == null)
				{
					Debug.LogWarning(string.Format("Invalid widget for index '{0}', removing it.", i), base.gameObject);
				}
				else if (flag)
				{
					this.widgetsMap[uiwidget] = uiwidget.depth - this.depth;
					this.serializedWidgetDepths.Add(uiwidget.depth - this.depth);
				}
				else
				{
					this.widgetsMap[uiwidget] = this.serializedWidgetDepths[i];
				}
			}
		}

		public void UpdateDepths()
		{
			if (this.depthControllers != null)
			{
				for (int i = 0; i < this.depthControllers.Count; i++)
				{
					NGUIWidgetDepthController nguiwidgetDepthController = this.depthControllers[i];
					nguiwidgetDepthController.depth = this.depth + this.controllersMap[nguiwidgetDepthController];
					nguiwidgetDepthController.UpdateDepths();
				}
			}
			if (this.widgets != null)
			{
				for (int j = 0; j < this.widgets.Count; j++)
				{
					UIWidget uiwidget = this.widgets[j];
					uiwidget.depth = this.depth + this.widgetsMap[uiwidget];
					uiwidget.SetDirty();
				}
			}
		}

		public void OnBeforeSerialize()
		{
			this.GetWidgets();
			this.FindDepths();
			List<int> list = new List<int>();
			if (this.depthControllers != null && this.controllersMap != null)
			{
				List<NGUIWidgetDepthController> list2 = new List<NGUIWidgetDepthController>(this.depthControllers);
				for (int i = 0; i < this.depthControllers.Count; i++)
				{
					NGUIWidgetDepthController nguiwidgetDepthController = this.depthControllers[i];
					if (nguiwidgetDepthController == null)
					{
						list2.Remove(nguiwidgetDepthController);
					}
					else
					{
						list.Add(this.controllersMap[nguiwidgetDepthController]);
					}
				}
				this.depthControllers = list2;
				this.serializedControllerDepths = list;
			}
			if (this.widgets != null && this.widgetsMap != null)
			{
				List<int> list3 = new List<int>();
				List<UIWidget> list4 = new List<UIWidget>(this.widgets);
				for (int j = 0; j < this.widgets.Count; j++)
				{
					UIWidget uiwidget = this.widgets[j];
					if (uiwidget == null)
					{
						list4.Remove(uiwidget);
					}
					else
					{
						list3.Add(this.widgetsMap[uiwidget]);
					}
				}
				this.widgets = list4;
				this.serializedWidgetDepths = list3;
			}
		}

		public void OnAfterDeserialize()
		{
		}

		public bool includeOnlyImmediateChildren = true;

		public bool includeInnactive = true;

		public int depth;

		public List<int> serializedWidgetDepths = new List<int>();

		public List<int> serializedControllerDepths = new List<int>();

		public Dictionary<UIWidget, int> widgetsMap = new Dictionary<UIWidget, int>();

		public Dictionary<NGUIWidgetDepthController, int> controllersMap = new Dictionary<NGUIWidgetDepthController, int>();
	}
}

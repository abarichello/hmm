using System;
using UnityEngine;

namespace HeavyMetalMachines.Infra.GUI
{
	public class NguiTooltipTrigger : MonoBehaviour
	{
		private void OnEnable()
		{
		}

		private void OnHover(bool isOver)
		{
			if (this._tooltipGameObject.activeSelf == isOver)
			{
				return;
			}
			this._tooltipGameObject.SetActive(isOver);
		}

		[SerializeField]
		private GameObject _tooltipGameObject;
	}
}

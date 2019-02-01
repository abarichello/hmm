using System;
using System.Collections;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class GUIUtils
	{
		public static void ControllerSetSelectedObject(GameObject target)
		{
			UICamera.selectedObject = target;
			UICamera.hoveredObject = target;
			UICamera.controller.current = target;
			UICamera.controllerNavigationObject = target;
		}

		public static void PopulateNavigationList(int maxPerLine, List<UIKeyNavigation> keyNavigations, bool isCircular)
		{
			for (int i = 0; i < keyNavigations.Count; i++)
			{
				GUIUtils.PopulateNavigationkey(maxPerLine, keyNavigations, i);
			}
			if (!isCircular || keyNavigations.Count <= 0)
			{
				return;
			}
			keyNavigations[0].onLeft = keyNavigations[keyNavigations.Count - 1].gameObject;
			keyNavigations[keyNavigations.Count - 1].onRight = keyNavigations[0].gameObject;
		}

		public static void PopulateNavigationkey(int maxPerLine, List<UIKeyNavigation> keyNavigations, int listIndex)
		{
			int num = listIndex / maxPerLine;
			int num2 = listIndex + 1;
			int num3 = num2 / maxPerLine;
			if (num < maxPerLine - 1 && num == num3 && num2 < keyNavigations.Count)
			{
				keyNavigations[listIndex].onRight = keyNavigations[num2].gameObject;
			}
			else
			{
				keyNavigations[listIndex].onRight = null;
			}
			int num4 = listIndex + maxPerLine;
			if (num4 < keyNavigations.Count)
			{
				keyNavigations[listIndex].onDown = keyNavigations[num4].gameObject;
			}
			else
			{
				keyNavigations[listIndex].onDown = null;
			}
			int num5 = listIndex - 1;
			int num6 = num5 / maxPerLine;
			if (num5 >= 0 && num == num6)
			{
				keyNavigations[listIndex].onLeft = keyNavigations[num5].gameObject;
			}
			else
			{
				keyNavigations[listIndex].onLeft = null;
			}
			int num7 = listIndex - maxPerLine;
			if (num7 >= 0)
			{
				keyNavigations[listIndex].onUp = keyNavigations[num7].gameObject;
			}
			else
			{
				keyNavigations[listIndex].onUp = null;
			}
		}

		public static void AnimateTweenAlpha(TweenAlpha tweenAlpha, bool forward, bool invertAnimtion)
		{
			if (tweenAlpha == null)
			{
				return;
			}
			if (invertAnimtion)
			{
				forward = !forward;
			}
			if (forward)
			{
				tweenAlpha.to = 1f;
				tweenAlpha.PlayForward();
			}
			else
			{
				tweenAlpha.from = 0.002f;
				tweenAlpha.PlayReverse();
			}
		}

		public static int InverseSortByName<T>(T a, T b) where T : UnityEngine.Object
		{
			return -string.CompareOrdinal(a.name, b.name);
		}

		public static int SortByName<T>(T a, T b) where T : UnityEngine.Object
		{
			return string.CompareOrdinal(a.name, b.name);
		}

		public static string GetShortName(string name, int nameMaxChars)
		{
			return (name.Length > nameMaxChars) ? (name.Substring(0, nameMaxChars) + "...") : name;
		}

		public static void CreateGridPool(UIGrid grid, int quantity)
		{
			GUIUtils.CreateGridPool(grid, grid.GetChild(0), quantity - 1);
		}

		public static void CreateGridPool(UIGrid grid, Transform reference, int quantity)
		{
			for (int i = 0; i < quantity; i++)
			{
				GUIUtils.GridPoolInstantiate(grid, reference);
			}
			grid.Reposition();
		}

		public static IEnumerator CreateGridPoolAsync(MonoBehaviour behaviour, UIGrid grid, int quantity, GUIUtils.GridPoolAsyncCompleteCallback callback)
		{
			bool hideInactiveCache = grid.hideInactive;
			grid.hideInactive = false;
			Transform reference = grid.GetChild(0);
			reference.gameObject.SetActive(false);
			yield return behaviour.StartCoroutine(GUIUtils.CreateGridPoolAsync(grid, reference, quantity - 1, null));
			grid.hideInactive = hideInactiveCache;
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		public static IEnumerator CreateGridPoolAsync(UIGrid grid, Transform reference, int quantity, GUIUtils.GridPoolAsyncCompleteCallback callback)
		{
			reference.gameObject.SetActive(false);
			for (int i = 0; i < quantity; i++)
			{
				GUIUtils.GridPoolInstantiate(grid, reference);
				yield return null;
			}
			grid.Reposition();
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		private static void GridPoolInstantiate(UIGrid grid, Transform reference)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(reference, Vector3.zero, Quaternion.identity);
			transform.parent = grid.transform;
			transform.localScale = reference.localScale;
		}

		public static IEnumerator HackScrollReposition(UIScrollView scrollView, UIScrollBar scrollBar, UIGrid grid)
		{
			scrollView.ResetPosition();
			yield return null;
			grid.repositionNow = true;
			yield return null;
			UIPanel panel = scrollView.GetComponent<UIPanel>();
			if (panel != null)
			{
				panel.Refresh();
			}
			scrollBar.value = 0f;
			scrollView.UpdateScrollbars(true);
			yield break;
		}

		public static bool ClampLabel(UILabel label, string text)
		{
			if (label.overflowMethod != UILabel.Overflow.ClampContent)
			{
				Debug.Assert(false, string.Format("ClampLabel only support ClampContent overflow method. Current:[{0}]", label.overflowMethod), Debug.TargetTeam.All);
				return false;
			}
			label.text = text;
			label.TryUpdateText();
			string processedText = label.processedText;
			if (processedText.ToLower() != text.ToLower() && processedText.Length > 3)
			{
				label.text = processedText.Substring(0, processedText.Length - 3) + "...";
				return true;
			}
			return false;
		}

		public static void PlayAnimation(Animation animation, bool reverse = false, float speedMultiplier = 1f, string targetAnimationName = "")
		{
			if (animation == null)
			{
				return;
			}
			foreach (object obj in animation)
			{
				AnimationState animationState = (AnimationState)obj;
				if (reverse)
				{
					animationState.speed = -1f;
					animationState.time = animationState.length;
				}
				else
				{
					animationState.speed = 1f;
				}
				animationState.speed *= speedMultiplier;
			}
			if (string.IsNullOrEmpty(targetAnimationName))
			{
				animation.Play();
			}
			else
			{
				animation.Play(targetAnimationName);
			}
		}

		public static void AnimationSetFirstFrame(Animation animation)
		{
			if (animation == null)
			{
				return;
			}
			foreach (object obj in animation)
			{
				AnimationState animationState = (AnimationState)obj;
				animationState.speed = -1f;
				animationState.time = 0f;
			}
			animation.Play();
		}

		public static void ResetAnimation(Animation animation)
		{
			if (animation == null)
			{
				return;
			}
			foreach (object obj in animation)
			{
				AnimationState animationState = (AnimationState)obj;
				animationState.time = 0f;
				animationState.enabled = true;
				animationState.weight = 1f;
				animation.Sample();
				animationState.enabled = false;
			}
		}

		public static void AnimationSetLastFrame(Animation animation)
		{
			if (animation == null)
			{
				GUIUtils.Log.FatalFormat("animation null on AnimationSetLastFrame", new object[0]);
				return;
			}
			foreach (object obj in animation)
			{
				AnimationState animationState = (AnimationState)obj;
				animationState.speed = 1f;
				animationState.time = animationState.length;
			}
			animation.Play();
		}

		public static IEnumerator LoadSpriteAsync(string url, Action<Texture2D> onTextureLoaded, Action<string> onError)
		{
			if (string.IsNullOrEmpty(url))
			{
				if (onError != null)
				{
					onError("URL is empty.");
				}
				yield break;
			}
			string urlRandomNum = "?" + UnityEngine.Random.Range(1, 9999);
			WWW www = new WWW(url + urlRandomNum);
			yield return www;
			if (string.IsNullOrEmpty(www.error))
			{
				if (www.texture.width == 8 && www.texture.height == 8)
				{
					if (onError != null)
					{
						onError(string.Format("Invalid texture on LoadSpriteAsync. Url:{0}", url));
					}
				}
				else if (onTextureLoaded != null)
				{
					onTextureLoaded(www.texture);
				}
			}
			else if (onError != null)
			{
				onError(string.Format("LoadSpriteAsync - WWW error on load texture. Url:[{0}] - Msg:[{1}]. Probably the service is offline or the url is invalid.", url, www.error));
			}
			yield break;
		}

		public static void SetLabelColorGradient(UILabel label, Color normalColor, Color gradientTopColor, Color gradientBottonColor)
		{
			label.color = normalColor;
			label.gradientTop = gradientTopColor;
			label.gradientBottom = gradientBottonColor;
			label.applyGradient = true;
		}

		public static IEnumerator WaitAndDisable(float seconds, GameObject toBeDisabled)
		{
			yield return new WaitForSecondsRealtime(seconds);
			toBeDisabled.SetActive(false);
			yield break;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GUIUtils));

		public delegate void GridPoolAsyncCompleteCallback();
	}
}

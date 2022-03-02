using System;
using System.Collections.Generic;
using System.Linq;
using Hoplon.Logging;
using Hoplon.Reactive;
using UniRx;

namespace HeavyMetalMachines.Presenting.Navigation
{
	public class PresenterTree : IPresenterTree
	{
		public PresenterTree(ILogger<PresenterTree> logger)
		{
			this._logger = logger;
		}

		public IObservable<Unit> Initialize(PresenterTreeSettings settings)
		{
			IPresenterNode rootNode = settings.RootNode;
			this._rootNode = rootNode;
			this._nodesParent = new Dictionary<IPresenterNode, IPresenterNode>();
			this.InitializeParentOfNode(rootNode, null);
			this._redirectingNodes = this.GetRedirections(settings);
			return this.NavigateToNode(this._rootNode);
		}

		private Dictionary<IPresenterNode, IPresenterNode> GetRedirections(PresenterTreeSettings settings)
		{
			if (settings.ForcedRedirections != null)
			{
				return settings.ForcedRedirections;
			}
			return new Dictionary<IPresenterNode, IPresenterNode>();
		}

		private void InitializeParentOfNode(IPresenterNode node, IPresenterNode parent)
		{
			this._nodesParent[node] = parent;
			if (node.Children == null)
			{
				return;
			}
			foreach (IPresenterNode node2 in node.Children)
			{
				this.InitializeParentOfNode(node2, node);
			}
		}

		public void Dispose()
		{
			Stack<IPresenterNode> navigationSequenceFromRoot = this.GetNavigationSequenceFromRoot(this._currentNode);
			IEnumerable<IObservable<Unit>> enumerable = from node in navigationSequenceFromRoot
			select node.Dispose();
			ObservableExtensions.Subscribe<Unit>(Observable.Merge<Unit>(enumerable));
			this._isNavigating = false;
		}

		public IObservable<Unit> NavigateToNode(IPresenterNode node)
		{
			IPresenterNode navigableNode = this.GetNavigableNode(node);
			if (this._currentNode == navigableNode)
			{
				return Observable.ReturnUnit();
			}
			return this.NavigateFromTo(this._currentNode, navigableNode);
		}

		public IObservable<Unit> NavigateToRelativeNode(string nodeName)
		{
			string[] array = nodeName.Split(new char[]
			{
				'/'
			});
			IPresenterNode presenterNode = this._currentNode;
			foreach (string text in array)
			{
				if (string.CompareOrdinal(text, "..") == 0)
				{
					presenterNode = this._nodesParent[presenterNode];
				}
				else
				{
					for (int j = 0; j < presenterNode.Children.Length; j++)
					{
						IPresenterNode presenterNode2 = presenterNode.Children[j];
						string[] array2 = presenterNode2.NavigationName.Split(new char[]
						{
							'/'
						});
						string a = array2[array2.Length - 1];
						if (a == text)
						{
							presenterNode = presenterNode2;
							break;
						}
					}
				}
			}
			return this.NavigateToNode(presenterNode);
		}

		private IObservable<Unit> NavigateFromTo(IPresenterNode sourceNode, IPresenterNode targetNode)
		{
			Stack<IPresenterNode> navigationSequenceFromRoot = this.GetNavigationSequenceFromRoot(sourceNode);
			Stack<IPresenterNode> navigationSequenceFromRoot2 = this.GetNavigationSequenceFromRoot(targetNode);
			PresenterTree.RemoveDuplicatedSequence(navigationSequenceFromRoot, navigationSequenceFromRoot2);
			IEnumerable<IPresenterNode> hideSequence = navigationSequenceFromRoot.Reverse<IPresenterNode>();
			Stack<IPresenterNode> showSequence = navigationSequenceFromRoot2;
			IObservable<Unit> navigationObservable = this.CreateHideAndShowSequenceObservable(hideSequence, showSequence);
			return this.CreateNavigationObservable(navigationObservable, targetNode);
		}

		private static void RemoveDuplicatedSequence(Stack<IPresenterNode> firstStack, Stack<IPresenterNode> secondStack)
		{
			while (firstStack.Any<IPresenterNode>() && secondStack.Any<IPresenterNode>() && firstStack.Peek() == secondStack.Peek())
			{
				firstStack.Pop();
				secondStack.Pop();
			}
		}

		private IObservable<Unit> CreateNavigationObservable(IObservable<Unit> navigationObservable, IPresenterNode node)
		{
			return Observable.Create<Unit>((IObserver<Unit> observer) => this.PerformNavigation(observer, navigationObservable, node));
		}

		private IDisposable PerformNavigation(IObserver<Unit> observer, IObservable<Unit> navigationObservable, IPresenterNode node)
		{
			if (this._isNavigating)
			{
				return this.PerformEmptyNavigation(observer, node);
			}
			this.SetCurrentNode(node);
			this.LogNodeNavigation(node);
			bool wasDisposed = false;
			ObservableExtensions.Subscribe<Unit>(navigationObservable, delegate()
			{
				if (wasDisposed)
				{
					return;
				}
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
			});
			return Disposable.Create(delegate()
			{
				wasDisposed = true;
			});
		}

		private IDisposable PerformEmptyNavigation(IObserver<Unit> observer, IPresenterNode node)
		{
			this._enqueuedNode = node;
			this.LogNavigationRequestedWhileAlreadyNavigating(node);
			observer.OnNext(Unit.Default);
			observer.OnCompleted();
			return Disposable.Empty;
		}

		private void SetCurrentNode(IPresenterNode node)
		{
			this.NotifyNodeLeft(this._currentNode);
			this._currentNode = node;
			this.NotifyNodeEntered(this._currentNode);
		}

		public IObservable<Unit> NavigateBackwards()
		{
			if (this._currentNode == this._rootNode)
			{
				throw new InvalidOperationException("Cannot navigate backwards while on root node.");
			}
			IPresenterNode navigableTargetNode = this.GetNavigableTargetNode(this._nodesParent[this._currentNode]);
			return this.NavigateToNode(navigableTargetNode);
		}

		public IPresenterNode GetNavigableTargetNode(IPresenterNode node)
		{
			if (this._rootNode == node || node == null)
			{
				return this._rootNode;
			}
			if (this._redirectingNodes.ContainsKey(node))
			{
				IPresenterNode node2 = this._nodesParent[node];
				return this.GetNavigableTargetNode(node2);
			}
			return node;
		}

		public IPresenterNode GetNavigableNode(IPresenterNode node)
		{
			IPresenterNode node2;
			if (this._redirectingNodes.TryGetValue(node, out node2))
			{
				return this.GetNavigableNode(node2);
			}
			return node;
		}

		public IObservable<IPresenterNode> ObserveNodeEnter(IPresenterNode node)
		{
			return Observable.Where<IPresenterNode>(this._onNodeEnteredSubject, (IPresenterNode enteredNode) => enteredNode == node);
		}

		public IObservable<IPresenterNode> ObserveNodeLeave(IPresenterNode node)
		{
			return Observable.Where<IPresenterNode>(this._onNodeLeftSubject, (IPresenterNode enteredNode) => enteredNode == node);
		}

		private void LogNodeNavigation(IPresenterNode node)
		{
			this._logger.Info(string.Format("Navigating to node {0}.", this._currentNode.NavigationName));
		}

		private void LogNavigationRequestedWhileAlreadyNavigating(IPresenterNode requestedNode)
		{
			this._logger.Warn(string.Format("Requested navigation to node {0} but was already navigating to another node ({1}).", requestedNode.NavigationName, this._currentNode.NavigationName));
		}

		private IObservable<Unit> CreateHideAndShowSequenceObservable(IEnumerable<IPresenterNode> hideSequence, IEnumerable<IPresenterNode> showSequence)
		{
			List<IPresenterNode> list = showSequence.ToList<IPresenterNode>();
			List<IPresenterNode> list2 = hideSequence.ToList<IPresenterNode>();
			return Observable.DoOnCancel<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._isNavigating = true;
			}), PresenterTree.CreateInitializationObservable(list)), PresenterTree.CreateHideShowObservable(list, list2)), PresenterTree.CreateDisposingObservable(list2)), this.NavigateToEnqueuedIfNeeded()), delegate()
			{
				this._isNavigating = false;
			});
		}

		private IObservable<Unit> NavigateToEnqueuedIfNeeded()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._isNavigating = false;
				if (this._enqueuedNode == null)
				{
					this._logger.DebugFormat("Navigation completed to {0}", new object[]
					{
						this._currentNode.NavigationName
					});
					return Observable.ReturnUnit();
				}
				this._logger.DebugFormat("Renavigation to {0}", new object[]
				{
					this._enqueuedNode.NavigationName
				});
				IPresenterNode enqueuedNode = this._enqueuedNode;
				this._enqueuedNode = null;
				return this.NavigateToNode(enqueuedNode);
			});
		}

		private static IObservable<Unit> CreateInitializationObservable(List<IPresenterNode> initializeSequenceList)
		{
			if (!initializeSequenceList.Any<IPresenterNode>())
			{
				return Observable.ReturnUnit();
			}
			IEnumerable<IObservable<Unit>> enumerable = from node in initializeSequenceList
			select node.Initialize();
			return Observable.Merge<Unit>(enumerable);
		}

		private static IObservable<Unit> CreateHideShowObservable(List<IPresenterNode> showSequenceList, List<IPresenterNode> hideSequenceList)
		{
			if (!showSequenceList.Any<IPresenterNode>() && !hideSequenceList.Any<IPresenterNode>())
			{
				return Observable.ReturnUnit();
			}
			IEnumerable<IObservable<Unit>> first = from node in hideSequenceList
			select node.Hide();
			IEnumerable<IObservable<Unit>> second = from node in showSequenceList
			select node.Show();
			IEnumerable<IObservable<Unit>> enumerable = first.Concat(second);
			return Observable.Merge<Unit>(enumerable);
		}

		private static IObservable<Unit> CreateDisposingObservable(List<IPresenterNode> disposeSequenceList)
		{
			if (!disposeSequenceList.Any<IPresenterNode>())
			{
				return Observable.ReturnUnit();
			}
			IEnumerable<IObservable<Unit>> enumerable = from node in disposeSequenceList
			select node.Dispose();
			return Observable.Merge<Unit>(enumerable);
		}

		private void NotifyNodeEntered(IPresenterNode node)
		{
			this._onNodeEnteredSubject.OnNext(node);
		}

		private void NotifyNodeLeft(IPresenterNode node)
		{
			this._onNodeLeftSubject.OnNext(node);
		}

		private Stack<IPresenterNode> GetNavigationSequenceFromRoot(IPresenterNode targetNode)
		{
			Stack<IPresenterNode> stack = new Stack<IPresenterNode>();
			if (targetNode == null)
			{
				return stack;
			}
			PresenterTree.AddNodeToSequence(this._rootNode, targetNode, stack);
			return stack;
		}

		private static bool AddNodeToSequence(IPresenterNode currentNode, IPresenterNode targetNode, Stack<IPresenterNode> sequence)
		{
			if (currentNode == targetNode)
			{
				sequence.Push(currentNode);
				return true;
			}
			if (currentNode.Children == null)
			{
				return false;
			}
			foreach (IPresenterNode currentNode2 in currentNode.Children)
			{
				if (PresenterTree.AddNodeToSequence(currentNode2, targetNode, sequence))
				{
					sequence.Push(currentNode);
					return true;
				}
			}
			return false;
		}

		private readonly ILogger<PresenterTree> _logger;

		private readonly Subject<IPresenterNode> _onNodeEnteredSubject = new Subject<IPresenterNode>();

		private readonly Subject<IPresenterNode> _onNodeLeftSubject = new Subject<IPresenterNode>();

		private IPresenterNode _rootNode;

		private IPresenterNode _currentNode;

		private Dictionary<IPresenterNode, IPresenterNode> _nodesParent;

		private Dictionary<IPresenterNode, IPresenterNode> _redirectingNodes;

		private bool _isNavigating;

		private IPresenterNode _enqueuedNode;
	}
}

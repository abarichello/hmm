using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class UndoTextControl
	{
		public UndoTextControl()
		{
		}

		public UndoTextControl(string initialText)
		{
			this.PushNewText(initialText);
		}

		public string CurrentText
		{
			get
			{
				return this._currentText;
			}
		}

		public void Clear()
		{
			this._currentText = null;
			this._undoStack.Clear();
			this._redoStack.Clear();
		}

		public void PushNewText(string text)
		{
			if (string.Equals(text, this._currentText, StringComparison.Ordinal))
			{
				return;
			}
			this._undoStack.Push(this._currentText);
			this._currentText = text;
			this._redoStack.Clear();
		}

		public string Redo()
		{
			if (this._redoStack.Count == 0)
			{
				return this._currentText;
			}
			this._undoStack.Push(this._currentText);
			this._currentText = this._redoStack.Pop();
			return this._currentText;
		}

		public string Undo()
		{
			if (this._undoStack.Count == 0)
			{
				if (!string.IsNullOrEmpty(this._currentText))
				{
					this._redoStack.Push(this._currentText);
				}
				this._currentText = null;
				return this._currentText;
			}
			this._redoStack.Push(this._currentText);
			this._currentText = this._undoStack.Pop();
			return this._currentText;
		}

		~UndoTextControl()
		{
			this.Clear();
		}

		public static bool CheckRedoInput()
		{
			return Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y);
		}

		public static bool CheckUndoInput()
		{
			return Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z);
		}

		private readonly Stack<string> _undoStack = new Stack<string>();

		private readonly Stack<string> _redoStack = new Stack<string>();

		private string _currentText;
	}
}

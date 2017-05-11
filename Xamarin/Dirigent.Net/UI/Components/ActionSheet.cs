using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dirigent.Net.Sys.Core;
using UIKit;

namespace Dirigent.Net.UI.Components {
	public class ActionSheet : IDisposable {
		private readonly IEnumerable<ActionSheetItem> items;
		private UIActionSheet actionSheet;
		private TaskCompletionSource<ActionSheetItem> tcs;

		public ActionSheet(string title, string cancel, IEnumerable<ActionSheetItem> items) {
			this.items = items;
			var del = new ActionSheetDelegate(this);
			var titles = items.Select(i => i.Title).ToArray();
			actionSheet = new UIActionSheet(title, del, cancel, null, titles);
		}

		public Task<ActionSheetItem> Present(UIView sender) {
			tcs = new TaskCompletionSource<ActionSheetItem>();
			actionSheet.ShowFrom(sender.Frame, sender, true);
			return tcs.Task;
		}

		public void OnDismissed(int idx) {
			if (idx >= 0 && idx < items.Count()) {
				tcs.TrySetResult(items.ElementAt(idx));
			}
			else {
				tcs.TrySetCanceled();
			}
		}

		public void OnClicked(int idx) {
		}

		public void OnCancelled() {
			tcs.TrySetCanceled();
		}

		public void Dispose() {
			if (actionSheet != null && actionSheet.Handle != IntPtr.Zero) {
				actionSheet.Delegate = null;
				actionSheet.Dispose();
				actionSheet = null;
			}
		}
	}

	internal class ActionSheetDelegate : UIActionSheetDelegate {
		private WeakReferenceEx<ActionSheet> parentRef;

		public ActionSheetDelegate(ActionSheet parent) {
			parentRef = WeakReferenceEx.Create(parent);
		}

		public override void Canceled(UIActionSheet actionSheet) {
			parentRef.Target.OnCancelled();
		}

		public override void Clicked(UIActionSheet actionSheet, nint buttonIndex) {
			parentRef.Target.OnClicked((int)buttonIndex);
		}

		public override void Dismissed(UIActionSheet actionSheet, nint buttonIndex) {
			parentRef.Target.OnDismissed((int)buttonIndex);
		}

		public override void Presented(UIActionSheet actionSheet) {
		}
	}

	public class ActionSheetItem {
		public string Title { get; private set; }
		public Func<ActionSheetItem, Task> Handler { get; private set; }

		public ActionSheetItem(string title, Func<ActionSheetItem, Task> handler) {
			Title = title;
			Handler = handler;
		}
	}
}
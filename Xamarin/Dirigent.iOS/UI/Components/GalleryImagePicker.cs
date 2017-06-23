using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dirigent.iOS.Logging;
using Foundation;
using UIKit;

namespace Dirigent.iOS.UI.Components {
	internal class GalleryImagePicker : IDisposable {
		private UIImagePickerController imagePickerController;
		private TaskCompletionSource<IEnumerable<string>> tcs;
		private static readonly Logger Logger = LogManager.GetLogger<GalleryImagePicker>();

		public GalleryImagePicker() {
			imagePickerController = new UIImagePickerController() {
				SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
				AllowsEditing = false,
				Delegate = new GalleryImagePickerDelegate(this)
			};
		}

		public Task<IEnumerable<string>> PickAsync(object sender) {
			tcs = new TaskCompletionSource<IEnumerable<string>>();

			var navigationController = UIApplication.SharedApplication.KeyWindow.RootViewController as UIViewController;
			navigationController.ShowViewController(imagePickerController, sender as NSObject);

			return tcs.Task;
		}

		public void Dispose() {
			if (imagePickerController != null) {
				imagePickerController.Delegate = null;
				imagePickerController.Dispose();
				imagePickerController = null;
			}
		}

		private void OnImagePicked(string imagePath) {
			tcs.TrySetResult(new [] { imagePath });
		}

		private void OnImageCancelled() {
			tcs.TrySetCanceled();
		}

		internal class GalleryImagePickerDelegate : UIImagePickerControllerDelegate {
			private readonly GalleryImagePicker parent;

			public GalleryImagePickerDelegate(GalleryImagePicker parent) {
				this.parent = parent;
			}

			public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info) {
				var nsImage = (NSObject)null;
				info.TryGetValue(new NSString("UIImagePickerControllerReferenceURL"), out nsImage);
				var imageName = nsImage.ToString();
				picker.DismissViewController(true, null);
				parent.OnImagePicked(imageName);
			}

			public override void Canceled(UIImagePickerController picker) {
				picker.DismissViewController(true, null);
				parent.OnImageCancelled();
			}
		}
	}
}
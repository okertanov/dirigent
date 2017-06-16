using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace Dirigent.Net.Services {
	public interface IPhotoLibraryService {
		Task<IEnumerable<string>> PickAsync(object sender);

		Task<IEnumerable<PHAssetCollection>> RequestCollectionsAsync();
		Task<IEnumerable<PHAssetCollection>> RequestCollectionsContainingAsync(PHAsset asset);

		Task<IEnumerable<PHAsset>> RequestAssetsAsync(IEnumerable<string> imagePaths);

		Task<PhotoLibraryImage> RequestImageAsync(PHAsset asset, CGSize targetSize);
		Task<PhotoLibraryImageData> RequestImageDataAsync(PHAsset asset);
	}

	public sealed class PhotoLibraryImage {
		public UIImage Image { get; }
		public NSDictionary Info { get; }

		public PhotoLibraryImage(UIImage image, NSDictionary info) {
			Image = image;
			Info = info;
		}
	}

	public sealed class PhotoLibraryImageData {
		public NSData Data { get; }
		public NSString DataUti { get; }
		public UIImageOrientation Orientation { get; }
		public NSDictionary Info { get; }

		public PhotoLibraryImageData(NSData data, NSString dataUti, UIImageOrientation orientation, NSDictionary info) {
			Data = data;
			DataUti = dataUti;
			Orientation = orientation;
			Info = info;
		}
	}
}
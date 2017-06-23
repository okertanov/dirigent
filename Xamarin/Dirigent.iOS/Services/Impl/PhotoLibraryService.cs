using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Dirigent.iOS.UI.Components;
using Foundation;
using Photos;
using UIKit;

namespace Dirigent.iOS.Services.Impl {
	public class PhotoLibraryService : IPhotoLibraryService {
		public PhotoLibraryService() {
		}

		public Task<IEnumerable<string>> PickAsync(object sender) {
			var picker = new GalleryImagePicker();
			return picker.PickAsync(this);
		}

		public Task<IEnumerable<PHAssetCollection>> RequestCollectionsAsync() {
			return Task.Run(() => {
				var fetchOptions = new PHFetchOptions {
					IncludeAssetSourceTypes = PHAssetSourceType.UserLibrary | PHAssetSourceType.iTunesSynced | PHAssetSourceType.CloudShared,
					WantsIncrementalChangeDetails = false
				};
				var albumCollectionsResult = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.Album, PHAssetCollectionSubtype.Any, fetchOptions);
				var smartAlbumCollectionResult = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum, PHAssetCollectionSubtype.Any, fetchOptions);
				var momentsCollectionResult = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.Moment, PHAssetCollectionSubtype.Any, fetchOptions);

				var albumCollections = albumCollectionsResult != null ? albumCollectionsResult.Cast<PHAssetCollection>() : Enumerable.Empty<PHAssetCollection>();
				var smartAlbumCollection = smartAlbumCollectionResult != null ? smartAlbumCollectionResult.Cast<PHAssetCollection>() : Enumerable.Empty<PHAssetCollection>();
				var momentsCollection = momentsCollectionResult != null ? momentsCollectionResult.Cast<PHAssetCollection>() : Enumerable.Empty<PHAssetCollection>();

				var collections = albumCollections.Concat(smartAlbumCollection).Concat(momentsCollection);
				return collections;
			});
		}

		public Task<IEnumerable<PHAssetCollection>> RequestCollectionsContainingAsync(PHAsset asset) {
			return Task.Run(() => {
				var fetchOptions = new PHFetchOptions {
					IncludeAssetSourceTypes = PHAssetSourceType.UserLibrary | PHAssetSourceType.iTunesSynced | PHAssetSourceType.CloudShared,
					WantsIncrementalChangeDetails = false
				};
				var albumCollectionsResult = PHAssetCollection.FetchAssetCollections(asset, PHAssetCollectionType.Album, fetchOptions);
				var smartAlbumCollectionResult = PHAssetCollection.FetchAssetCollections(asset, PHAssetCollectionType.SmartAlbum, fetchOptions);
				var momentsCollectionResult = PHAssetCollection.FetchAssetCollections(asset, PHAssetCollectionType.Moment, fetchOptions);

				var albumCollections = albumCollectionsResult != null ? albumCollectionsResult.Cast<PHAssetCollection>() : Enumerable.Empty<PHAssetCollection>();
				var smartAlbumCollection = smartAlbumCollectionResult != null ? smartAlbumCollectionResult.Cast<PHAssetCollection>() : Enumerable.Empty<PHAssetCollection>();
				var momentsCollection = momentsCollectionResult != null ? momentsCollectionResult.Cast<PHAssetCollection>() : Enumerable.Empty<PHAssetCollection>();

				var collections = albumCollections.Concat(smartAlbumCollection).Concat(momentsCollection);
				return collections;
			});
		}

		public Task<IEnumerable<PHAsset>> RequestAssetsAsync(IEnumerable<string> imagePaths) {
			return Task.Run(() => {
				var imagesUrls = imagePaths.Select(p => new NSUrl(p)).ToArray();
				var fetchOptions = new PHFetchOptions {
					IncludeAssetSourceTypes = PHAssetSourceType.UserLibrary | PHAssetSourceType.iTunesSynced | PHAssetSourceType.CloudShared,
					WantsIncrementalChangeDetails = false,
					SortDescriptors = new[] { new NSSortDescriptor("creationDate", false) }
				};
				var assetResult = PHAsset.FetchAssets(imagesUrls, fetchOptions);
				var assets = assetResult.Cast<PHAsset>();
				return assets;
			});
		}

		public Task<PhotoLibraryImage> RequestImageAsync(PHAsset asset, CGSize targetSize) {
			var tcs = new TaskCompletionSource<PhotoLibraryImage>();
			var requestOptions = new PHImageRequestOptions {
				Synchronous = false,
				DeliveryMode = PHImageRequestOptionsDeliveryMode.FastFormat,
				ResizeMode = PHImageRequestOptionsResizeMode.Fast
			};
			var requestId = PHImageManager.DefaultManager.RequestImageForAsset(asset, targetSize, PHImageContentMode.Default, requestOptions, (image, info) => {
				tcs.TrySetResult(new PhotoLibraryImage(image, info));
			});
			return tcs.Task;
		}

		public Task<PhotoLibraryImageData> RequestImageDataAsync(PHAsset asset) {
			var tcs = new TaskCompletionSource<PhotoLibraryImageData>();
			var requestOptions = new PHImageRequestOptions {
				Synchronous = false,
				DeliveryMode = PHImageRequestOptionsDeliveryMode.FastFormat,
				ResizeMode = PHImageRequestOptionsResizeMode.Fast
			};
			var requestId = PHImageManager.DefaultManager.RequestImageData(asset, requestOptions, (data, dataUti, orientation, info) => {
				tcs.TrySetResult(new PhotoLibraryImageData(data, dataUti, orientation, info));
			});
			return tcs.Task;
		}
	}
}
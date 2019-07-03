using System;
using AVFoundation;
using CoreMedia;
using Foundation;

namespace XCamera.iOS
{
	public class XCameraPhotoCaptureDelegate : NSObject, IAVCapturePhotoCaptureDelegate
	{
		public AVCapturePhotoSettings RequestedPhotoSettings { get; set; }
		NSData PhotoData { get; set; }
		Action<byte[]> CompletionHandler;
		byte[] imageBytes;

		public XCameraPhotoCaptureDelegate(AVCapturePhotoSettings requestedPhotoSettings, Action<byte[]> completionHandler)
		{
			RequestedPhotoSettings = requestedPhotoSettings;
			CompletionHandler = completionHandler;
		}

		[Export("captureOutput:willBeginCaptureForResolvedSettings:")]
		public virtual void WillBeginCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{

		}

		[Export("captureOutput:willCapturePhotoForResolvedSettings:")]
		public virtual void WillCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{

		}

		[Export("captureOutput:didFinishProcessingPhoto:error:")]
		public virtual void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, AVCapturePhoto photo, NSError error)
		{
			if (error != null)
			{
				Console.WriteLine($"Error capturing photo: {error}", error);
				return;
			}

			PhotoData = photo.FileDataRepresentation();

			imageBytes = new byte[PhotoData.Length];
			System.Runtime.InteropServices.Marshal.Copy(PhotoData.Bytes, imageBytes, 0, Convert.ToInt32(PhotoData.Length));
		}

		[Export("captureOutput:didFinishRecordingLivePhotoMovieForEventualFileAtURL:resolvedSettings:")]
		public virtual void DidFinishRecordingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, AVCaptureResolvedPhotoSettings resolvedSettings)
		{

		}

		[Export("captureOutput:didFinishProcessingLivePhotoToMovieFileAtURL:duration:photoDisplayTime:resolvedSettings:error:")]
		public virtual void DidFinishProcessingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, CMTime duration, CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
			if (error != null)
			{
				Console.WriteLine($"Error processing live photo companion movie: {error}", error);
				return;
			}
		}

		[Export("captureOutput:didFinishCaptureForResolvedSettings:error:")]
		public virtual void DidFinishCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
			if (error != null)
			{
				Console.WriteLine($"Error capturing photo: {error}", error);
			}

			if (PhotoData == null)
			{
				Console.WriteLine("No photo data resource");
			}

			CompletionHandler(imageBytes);
		}
	}
}

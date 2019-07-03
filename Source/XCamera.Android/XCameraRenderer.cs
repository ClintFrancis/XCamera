using System;
using System.ComponentModel;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XCamera.Droid;
using XCamera.Shared;
using XCamera.Shared.Events;

[assembly: ExportRenderer(typeof(XCameraView), typeof(XCameraRenderer))]
namespace XCamera.Droid
{
	public class XCameraRenderer : ViewRenderer<XCameraView, XCameraCaptureView>
	{
		XCameraCaptureView cameraPreview;
		XCameraView element;

		public XCameraRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<XCameraView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				cameraPreview.PhotoCaptured -= e.NewElement.PhotoCaptured;
				cameraPreview.FrameCaptured -= e.NewElement.FrameCaptured;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					cameraPreview = new XCameraCaptureView(Context, e.NewElement.CameraOption);
					cameraPreview.CaptureFrames = e.NewElement.CaptureFrames;
					cameraPreview.PhotoCaptured += e.NewElement.PhotoCaptured;
					cameraPreview.FrameCaptured += e.NewElement.FrameCaptured;

					cameraPreview.Initialize();

					SetNativeControl(cameraPreview);
				}

				// Subscribe
				element = e.NewElement;
				element.SetNativeCamera(cameraPreview);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CameraPropertyIds.CameraOption)
			{
				var view = (XCameraView)sender;
				cameraPreview.CameraOption = view.CameraOption;
			}

			if (e.PropertyName == CameraPropertyIds.CaptureFrames)
			{
				var view = (XCameraView)sender;
				cameraPreview.CaptureFrames = view.CaptureFrames;
			}

			if (e.PropertyName == CameraPropertyIds.FrameRate)
			{
				var view = (XCameraView)sender;
				cameraPreview.SetFrameRate(view.FrameRate);
			}

			//else if (e.PropertyName == CameraPropertyIds.Width)
			//{
			//	cameraPreview.SetNeedsDisplay();
			//}
		}

		bool tempHasCaptured = false;
		void CaptureToFile()
		{

			tempHasCaptured = false;
			cameraPreview.Capture();
		}

		void ImageCaptured(object sender, NativeImageCaptureEvent e)
		{
			if (tempHasCaptured)
				return;

			tempHasCaptured = true;
			//captureBytesCallbackAction(e.Data);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				Control.CaptureSession?.Dispose();
				Control.Dispose();
			}
		}
	}
}

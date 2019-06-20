using System;
using System.ComponentModel;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XCamera.Droid;
using XCamera.Shared;

[assembly: ExportRenderer(typeof(XCameraView), typeof(XCameraCaptureView))]
namespace XCamera.Droid
{
	public class XCameraRenderer : ViewRenderer<XCameraView, XCameraCaptureView>
	{
		XCameraCaptureView cameraPreview;
		XCameraView element;
		Action<byte[]> captureBytesCallbackAction;

		public XCameraRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<XCameraView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				cameraPreview = new XCameraCaptureView(Context, e.NewElement.CameraOption);
				cameraPreview.ImageCaptured += ImageCaptured;
				SetNativeControl(cameraPreview);
			}
			if (e.OldElement != null)
			{
				// Unsubscribe
				captureBytesCallbackAction = null;
				element.Capture = null;
				element.StartCamera = null;
				element.StopCamera = null;
			}
			if (e.NewElement != null)
			{
				// Subscribe
				element = e.NewElement;
				captureBytesCallbackAction = element.CaptureBytesCallback;
				element.Capture = new Command(() => CaptureToFile());
				element.StartCamera = new Command(() =>
				{
					cameraPreview.StartPreviewing();
				});
				element.StopCamera = new Command(() =>
				{
					cameraPreview.StopPreviewing();
				});
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "CameraOption")
			{
				var view = (XCameraView)sender;
				cameraPreview.UpdateCameraOption(view.CameraOption);
			}
		}

		bool tempHasCaptured = false;
		void CaptureToFile()
		{
			if (captureBytesCallbackAction == null)
				return;

			tempHasCaptured = false;
			cameraPreview.Capture();
		}

		void ImageCaptured(object sender, ImageCaptureEventArgs e)
		{
			if (tempHasCaptured)
				return;

			tempHasCaptured = true;
			captureBytesCallbackAction(e.Bytes);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				cameraPreview.ImageCaptured -= ImageCaptured;
				Control.CaptureSession?.Dispose();
				Control.Dispose();
			}
		}
	}
}

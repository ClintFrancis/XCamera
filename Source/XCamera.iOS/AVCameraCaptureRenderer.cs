using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XCamera.iOS;
using XCamera.Shared;

[assembly: ExportRenderer(typeof(XCameraView), typeof(AVCameraCaptureRenderer))]
namespace XCamera.iOS
{
	public class AVCameraCaptureRenderer : ViewRenderer<XCameraView, AVCameraCaptureView>
	{
		XCameraView element;
		AVCameraCaptureView uiCameraPreview;
		Action<byte[]> capturePathCallbackAction;

		protected override void OnElementChanged(ElementChangedEventArgs<XCameraView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				// Unsubscribe
				uiCameraPreview.ImageCaptured -= UiCameraPreview_ImageCaptured;
				capturePathCallbackAction = null;
				element.Capture = null;
				element.StartCamera = null;
				element.StopCamera = null;
			}
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					uiCameraPreview = new AVCameraCaptureView(e.NewElement.CameraOption);
					uiCameraPreview.ImageCaptured += UiCameraPreview_ImageCaptured;
					SetNativeControl(uiCameraPreview);
				}

				// Subscribe
				element = e.NewElement;
				capturePathCallbackAction = element.CaptureBytesCallback;
				element.Capture = new Command(() => uiCameraPreview.Capture());
				element.StartCamera = new Command(() => uiCameraPreview.StartPreviewing());
				element.StopCamera = new Command(() => uiCameraPreview.StopPreviewing());
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			Console.WriteLine($"AVCameraCaptureRenderer Property Changed: {e.PropertyName}");

			if (e.PropertyName == CameraPropertyIds.CameraOption)
			{
				var view = (XCameraView)sender;
				uiCameraPreview.UpdateCameraOption(view.CameraOption);
			}

			else if (e.PropertyName == CameraPropertyIds.Width)
			{
				uiCameraPreview.SetNeedsDisplay();
			}
		}

		void UiCameraPreview_ImageCaptured(object sender, ImageCapturedEventArgs e)
		{
			capturePathCallbackAction(e.Data);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Control.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}

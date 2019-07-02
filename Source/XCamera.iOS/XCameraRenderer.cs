using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XCamera.iOS;
using XCamera.Shared;
using XCamera.Shared.Events;

[assembly: ExportRenderer(typeof(XCameraView), typeof(XCameraRenderer))]
namespace XCamera.iOS
{
	public class XCameraRenderer : ViewRenderer<XCameraView, XCameraCaptureView>
	{
		XCameraView element;
		XCameraCaptureView uiCameraPreview;
		Action<byte[]> capturePathCallbackAction;

		protected override void OnElementChanged(ElementChangedEventArgs<XCameraView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				// Unsubscribe
				uiCameraPreview.ImageCaptured -= UiCameraPreview_ImageCaptured;
				capturePathCallbackAction = null;
			}
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					uiCameraPreview = new XCameraCaptureView(e.NewElement.CameraOption);
					uiCameraPreview.ImageCaptured += UiCameraPreview_ImageCaptured;
					SetNativeControl(uiCameraPreview);
				}

				// Subscribe
				element = e.NewElement;
				element.SetNativeCamera(uiCameraPreview);
				capturePathCallbackAction = element.CaptureBytesCallback;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CameraPropertyIds.CameraOption)
			{
				var view = (XCameraView)sender;
				uiCameraPreview.CameraOption = view.CameraOption;
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

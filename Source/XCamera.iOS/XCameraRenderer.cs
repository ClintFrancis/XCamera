using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XCamera.iOS;
using XCamera.Shared;

[assembly: ExportRenderer(typeof(XCameraView), typeof(XCameraRenderer))]
namespace XCamera.iOS
{
	public class XCameraRenderer : ViewRenderer<XCameraView, XCameraCaptureView>
	{
		XCameraView element;
		XCameraCaptureView uiCameraPreview;

		protected override void OnElementChanged(ElementChangedEventArgs<XCameraView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				uiCameraPreview.PhotoCaptured -= e.NewElement.PhotoCaptured;
				uiCameraPreview.FrameCaptured -= e.NewElement.FrameCaptured;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					uiCameraPreview = new XCameraCaptureView(e.NewElement.CameraOption, e.NewElement.FrameRate);
					uiCameraPreview.CaptureFrames = e.NewElement.CaptureFrames;
					uiCameraPreview.PhotoCaptured += e.NewElement.PhotoCaptured;
					uiCameraPreview.FrameCaptured += e.NewElement.FrameCaptured;

					uiCameraPreview.Initialize();

					SetNativeControl(uiCameraPreview);
				}

				// Subscribe
				element = e.NewElement;
				element.SetNativeCamera(uiCameraPreview);
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

			if (e.PropertyName == CameraPropertyIds.CaptureFrames)
			{
				var view = (XCameraView)sender;
				uiCameraPreview.CaptureFrames = view.CaptureFrames;
			}

			if (e.PropertyName == CameraPropertyIds.FrameRate)
			{
				var view = (XCameraView)sender;
				uiCameraPreview.SetFrameRate(view.FrameRate);
			}

			else if (e.PropertyName == CameraPropertyIds.Width)
			{
				uiCameraPreview.SetNeedsDisplay();
			}

			// todo event handlers?
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
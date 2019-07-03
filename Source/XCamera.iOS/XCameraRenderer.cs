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
		XCameraCaptureView cameraPreview;

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
					cameraPreview = new XCameraCaptureView(e.NewElement.CameraOption, e.NewElement.FrameRate);
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

			else if (e.PropertyName == CameraPropertyIds.Width)
			{
				cameraPreview.SetNeedsDisplay();
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
using System;
using System.Linq;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using XCamera.Shared;
using XCamera.Shared.Events;
using XCamera.Shared.Interfaces;

namespace XCamera.iOS
{
	public class XCameraCaptureView : UIView, INativeCameraView
	{
		AVCaptureVideoPreviewLayer previewLayer;
		AVCaptureSession captureSession;
		AVCapturePhotoOutput photoOutput;
		AVCapturePhotoSettings photoSettings;
		AVCaptureDeviceInput videoDeviceInput;
		XCameraCaptureDelegate photoCaptureDelegate;
		CameraOptions cameraOptions;

		public event ImageCapturedEventHandler ImageCaptured;

		public bool IsPreviewing { get; private set; }

		public CameraOptions CameraOption
		{
			get { return cameraOptions; }
			set
			{
				if (cameraOptions == value)
					return;

				cameraOptions = value;
				UpdateCameraOption();
			}
		}

		public XCameraCaptureView(CameraOptions options)
		{
			cameraOptions = options;
			IsPreviewing = false;
			Initialize();
		}

		public override void Draw(CGRect rect)
		{
			previewLayer.Frame = rect;

			var deviceOrientation = UIDevice.CurrentDevice.Orientation;

			// Update orientation
			if (deviceOrientation.IsPortrait() || deviceOrientation.IsLandscape())
			{
				previewLayer.Connection.VideoOrientation = (AVCaptureVideoOrientation)deviceOrientation;

				var photoOutputConnection = photoOutput.ConnectionFromMediaType(AVMediaType.Video);
				photoOutputConnection.VideoOrientation = previewLayer.Connection.VideoOrientation;
			}
		}

		void Initialize()
		{
			// Create the capture session
			captureSession = new AVCaptureSession();
			previewLayer = new AVCaptureVideoPreviewLayer(captureSession)
			{
				Frame = Bounds,
				VideoGravity = AVLayerVideoGravity.ResizeAspectFill
			};

			captureSession.BeginConfiguration();
			SetupVideoInput();
			SetupPhotoCapture();
			captureSession.CommitConfiguration();

			Layer.AddSublayer(previewLayer);
		}

		void SetupVideoInput()
		{
			// Video Input

			var videoDevices = AVCaptureDeviceDiscoverySession.Create(
							new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera, AVCaptureDeviceType.BuiltInDualCamera },
							AVMediaType.Video,
							AVCaptureDevicePosition.Unspecified
						);

			var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
			var device = videoDevices.Devices.FirstOrDefault(d => d.Position == cameraPosition);
			if (device == null)
				return;

			ConfigureCameraForDevice(device);

			NSError error;
			videoDeviceInput = new AVCaptureDeviceInput(device, out error);
			captureSession.AddInput(videoDeviceInput);
		}

		void SetupPhotoCapture()
		{
			captureSession.SessionPreset = AVCaptureSession.PresetPhoto;

			// Add photo output.
			photoOutput = new AVCapturePhotoOutput();
			photoOutput.IsHighResolutionCaptureEnabled = true;
			captureSession.AddOutput(photoOutput);
			captureSession.CommitConfiguration();
		}

		void ConfigureCameraForDevice(AVCaptureDevice device)
		{
			var error = new NSError();
			if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
			{
				device.LockForConfiguration(out error);
				device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
				device.UnlockForConfiguration();
			}
			else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
			{
				device.LockForConfiguration(out error);
				device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
				device.UnlockForConfiguration();
			}
			else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
			{
				device.LockForConfiguration(out error);
				device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
				device.UnlockForConfiguration();
			}
		}

		public void StartPreview()
		{
			captureSession.StartRunning();
			IsPreviewing = true;
		}

		public void StopPreview()
		{
			captureSession.StopRunning();
			IsPreviewing = false;
		}

		void UpdateCameraOption()
		{
			var devices = AVCaptureDeviceDiscoverySession.Create(
										new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera, AVCaptureDeviceType.BuiltInDualCamera },
										AVMediaType.Video,
										AVCaptureDevicePosition.Unspecified
									);

			var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
			var device = devices.Devices.FirstOrDefault(d => d.Position == cameraPosition);

			if (device != null)
			{
				var lVideoDeviceInput = AVCaptureDeviceInput.FromDevice(device);

				captureSession.BeginConfiguration();

				// Remove the existing device input first, since using the front and back camera simultaneously is not supported.
				captureSession.RemoveInput(videoDeviceInput);

				if (captureSession.CanAddInput(lVideoDeviceInput))
				{
					captureSession.AddInput(lVideoDeviceInput);
					videoDeviceInput = lVideoDeviceInput;
				}
				else
				{
					captureSession.AddInput(videoDeviceInput);
				}

				captureSession.CommitConfiguration();
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void Capture()
		{
			// Create Photo Settings
			photoSettings = AVCapturePhotoSettings.FromFormat(new NSDictionary<NSString, NSObject>(AVVideo.CodecKey, AVVideo.CodecJPEG));
			photoSettings.IsHighResolutionPhotoEnabled = true;
			photoSettings.IsDepthDataDeliveryEnabled(false);

			if (photoSettings.AvailablePreviewPhotoPixelFormatTypes.Count() > 0)
				photoSettings.PreviewPhotoFormat = new NSDictionary<NSString, NSObject>(CoreVideo.CVPixelBuffer.PixelFormatTypeKey, photoSettings.AvailablePreviewPhotoPixelFormatTypes.First());

			// Use a separate object for the photo capture delegate to isolate each capture life cycle.
			photoCaptureDelegate = new XCameraCaptureDelegate(photoSettings, CompletionHandler);

			photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
		}

		void CompletionHandler(byte[] bytes)
		{
			var eventData = new ImageCapturedEventArgs(bytes);
			ImageCaptured?.Invoke(this, eventData);
		}
	}
}

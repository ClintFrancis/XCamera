using System;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using CoreVideo;
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
		AVCaptureDevice captureDevice;
		XCameraPhotoCaptureDelegate photoCaptureDelegate;
		XCameraVideoOutputDelegate videoCaptureDelegate;
		CameraOptions cameraOptions;
		int targetFramerate;

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

		public XCameraCaptureView(CameraOptions options, int frameRate = 60)
		{
			cameraOptions = options;
			targetFramerate = frameRate;
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

				if (photoOutput != null)
				{
					var photoOutputConnection = photoOutput.ConnectionFromMediaType(AVMediaType.Video);
					photoOutputConnection.VideoOrientation = previewLayer.Connection.VideoOrientation;
				}

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
			SetupCaptureDevice();
			//SetupPhotoCapture();
			SetupVideoCapture();
			SetFrameRate();
			captureSession.CommitConfiguration();

			Layer.AddSublayer(previewLayer);
		}

		void SetupCaptureDevice()
		{
			var videoDevices = AVCaptureDeviceDiscoverySession.Create(
							new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera, AVCaptureDeviceType.BuiltInDualCamera },
							AVMediaType.Video,
							AVCaptureDevicePosition.Unspecified
						);

			var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
			captureDevice = videoDevices.Devices.FirstOrDefault(d => d.Position == cameraPosition);
			if (captureDevice == null)
				return;

			ConfigureCameraForDevice(captureDevice);

			NSError error;
			videoDeviceInput = new AVCaptureDeviceInput(captureDevice, out error);
			captureSession.AddInput(videoDeviceInput);
		}

		void SetupPhotoCapture()
		{
			captureSession.SessionPreset = AVCaptureSession.PresetPhoto;

			// Add photo output.
			photoOutput = new AVCapturePhotoOutput();
			photoOutput.IsHighResolutionCaptureEnabled = true;

			if (captureSession.CanAddOutput(photoOutput))
				captureSession.AddOutput(photoOutput);

			captureSession.CommitConfiguration();
		}

		XCameraVideoOutputDelegate videoDelegate;
		DispatchQueue queue;
		AVCaptureVideoDataOutput videoOutput;

		void SetupVideoCapture()
		{
			var settings = new AVVideoSettingsUncompressed();
			settings.PixelFormatType = CVPixelFormatType.CV32BGRA;

			videoDelegate = new XCameraVideoOutputDelegate(VideoCaptureCallback);
			queue = new DispatchQueue("XCamera.CameraQueue");

			videoOutput = new AVCaptureVideoDataOutput();
			videoOutput.UncompressedVideoSetting = settings;
			videoOutput.AlwaysDiscardsLateVideoFrames = true;
			videoOutput.SetSampleBufferDelegateQueue(videoDelegate, queue);

			if (captureSession.CanAddOutput(videoOutput))
				captureSession.AddOutput(videoOutput);

			// We want the buffers to be in portrait orientation otherwise they are
			// rotated by 90 degrees. Need to set this _after_ addOutput()!
			var captureConnection = videoOutput.ConnectionFromMediaType(AVMediaType.Video);
			captureConnection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
		}

		void SetFrameRate()
		{
			// Based on code from https://github.com/dokun1/Lumina/
			var activeDimensions = (captureDevice.ActiveFormat.FormatDescription as CMVideoFormatDescription).Dimensions;
			foreach (var vFormat in captureDevice.Formats)
			{
				var dimensions = (vFormat.FormatDescription as CMVideoFormatDescription).Dimensions;
				var ranges = vFormat.VideoSupportedFrameRateRanges;
				var frameRate = ranges[0];

				if (frameRate.MaxFrameRate >= (double)targetFramerate &&
					frameRate.MinFrameRate <= (double)targetFramerate &&
					activeDimensions.Width == dimensions.Width &&
					activeDimensions.Height == dimensions.Height &&
					vFormat.FormatDescription.MediaSubType == 875704422) // meant for full range 420f
				{
					try
					{
						NSError error;
						captureDevice.LockForConfiguration(out error);
						captureDevice.ActiveFormat = vFormat as AVCaptureDeviceFormat;
						captureDevice.ActiveVideoMinFrameDuration = new CMTime(1, targetFramerate);
						captureDevice.ActiveVideoMaxFrameDuration = new CMTime(1, targetFramerate);
						captureDevice.UnlockForConfiguration();
					}
					catch (Exception ex)
					{
						continue;
					}
				}

				Console.WriteLine("Camera format: " + captureDevice.ActiveFormat);
			}
		}

		void VideoCaptureCallback(byte[] input)
		{
			throw new NotImplementedException();
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
			if (!captureSession.Running)
				captureSession.StartRunning();
			IsPreviewing = true;
		}

		public void StopPreview()
		{
			if (captureSession.Running)
				captureSession.StopRunning();
			IsPreviewing = false;
		}

		public void SetFrameRate(int frameRate)
		{
			targetFramerate = frameRate;
			// TODO update the framerate?

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
			photoCaptureDelegate = new XCameraPhotoCaptureDelegate(photoSettings, CompletionHandler);

			photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
		}

		void CompletionHandler(byte[] bytes)
		{
			var eventData = new ImageCapturedEventArgs(bytes);
			ImageCaptured?.Invoke(this, eventData);
		}


	}
}

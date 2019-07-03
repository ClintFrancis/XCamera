using System;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using CoreVideo;
using Foundation;
using UIKit;
using XCamera.Events;
using XCamera.Shared;
using XCamera.Shared.Events;
using XCamera.Shared.Interfaces;

namespace XCamera.iOS
{
	public class XCameraCaptureView : UIView, INativeCameraView
	{
		public event NativeImageCaptureEventHandler PhotoCaptured;
		public event NativeImageCaptureEventHandler FrameCaptured;

		AVCaptureVideoPreviewLayer previewLayer;
		AVCaptureSession captureSession;
		AVCapturePhotoOutput photoOutput;
		AVCapturePhotoSettings photoSettings;
		AVCaptureDeviceInput videoDeviceInput;
		AVCaptureDevice captureDevice;
		XCameraPhotoCaptureDelegate photoCaptureDelegate;
		XCameraVideoOutputDelegate videoCaptureDelegate;
		DispatchQueue queue;
		AVCaptureVideoDataOutput videoOutput;
		int targetFramerate;
		bool isInitialized;

		public bool IsPreviewing { get; private set; }

		CameraOptions cameraOptions;
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

		bool shouldCaptureFrames;
		public bool CaptureFrames
		{
			get { return shouldCaptureFrames; }
			set
			{
				if (shouldCaptureFrames == value)
					return;

				shouldCaptureFrames = value;

				if (isInitialized)
				{
					captureSession.BeginConfiguration();
					SetCaptureType();
					captureSession.CommitConfiguration();
				}
			}
		}

		public XCameraCaptureView(CameraOptions options, int frameRate = 60)
		{
			cameraOptions = options;
			targetFramerate = frameRate;
			IsPreviewing = false;
		}

		public override void Draw(CGRect rect)
		{
			previewLayer.Frame = rect;

			var deviceOrientation = UIDevice.CurrentDevice.Orientation;

			// Update orientation

			if (deviceOrientation.IsPortrait() || deviceOrientation.IsLandscape())
			{
				previewLayer.Connection.VideoOrientation = (AVCaptureVideoOrientation)deviceOrientation;

				// TODO Switch on whether we're capturing
				//if (photoOutput != null)
				//{
				//	var photoOutputConnection = photoOutput.ConnectionFromMediaType(AVMediaType.Video);
				//	photoOutputConnection.VideoOrientation = previewLayer.Connection.VideoOrientation;
				//}

			}
		}

		public void Initialize()
		{
			captureSession = new AVCaptureSession();

			captureSession.BeginConfiguration();
			SetupCaptureDevice();
			SetupPhotoCapture();
			SetupVideoCapture();
			SetCaptureType();
			SetFrameRate();
			captureSession.CommitConfiguration();

			Layer.AddSublayer(previewLayer);

			isInitialized = true;
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

		public void Capture()
		{
			// Create Photo Settings
			photoSettings = AVCapturePhotoSettings.FromFormat(new NSDictionary<NSString, NSObject>(AVVideo.CodecKey, AVVideo.CodecJPEG));
			photoSettings.IsHighResolutionPhotoEnabled = true;
			photoSettings.IsDepthDataDeliveryEnabled(false);

			if (photoSettings.AvailablePreviewPhotoPixelFormatTypes.Count() > 0)
				photoSettings.PreviewPhotoFormat = new NSDictionary<NSString, NSObject>(CVPixelBuffer.PixelFormatTypeKey, photoSettings.AvailablePreviewPhotoPixelFormatTypes.First());

			// Use a separate object for the photo capture delegate to isolate each capture life cycle.
			photoCaptureDelegate = new XCameraPhotoCaptureDelegate(photoSettings, PhotoCapturedHandler);

			photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
		}

		void SetupCaptureDevice()
		{
			captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
			if (captureDevice == null)
			{
				Console.WriteLine("Error: no video devices available");
				return;
			}

			videoDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
			if (videoDeviceInput == null)
			{
				Console.WriteLine("Error: could not create AVCaptureDeviceInput");
				return;
			}

			if (captureSession.CanAddInput(videoDeviceInput))
			{
				captureSession.AddInput(videoDeviceInput);
			}

			previewLayer = AVCaptureVideoPreviewLayer.FromSession(captureSession);
			previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspect;
			previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
		}

		void SetupPhotoCapture()
		{
			captureSession.SessionPreset = AVCaptureSession.PresetPhoto;

			// Add photo output.
			photoOutput = new AVCapturePhotoOutput();
			photoOutput.IsHighResolutionCaptureEnabled = true;

			if (captureSession.CanAddOutput(photoOutput))
				captureSession.AddOutput(photoOutput);
		}

		void SetupVideoCapture()
		{
			var settings = new AVVideoSettingsUncompressed();
			settings.PixelFormatType = CVPixelFormatType.CV32BGRA;

			videoCaptureDelegate = new XCameraVideoOutputDelegate(FrameCapturedHandler);
			queue = new DispatchQueue("XCamera.CameraQueue");

			videoOutput = new AVCaptureVideoDataOutput();
			videoOutput.UncompressedVideoSetting = settings;
			videoOutput.AlwaysDiscardsLateVideoFrames = true;
			videoOutput.SetSampleBufferDelegateQueue(videoCaptureDelegate, queue);
		}

		void SetCaptureType()
		{
			if (shouldCaptureFrames && captureSession.CanAddOutput(videoOutput))
			{
				captureSession.AddOutput(videoOutput);

				// We want the buffers to be in portrait orientation otherwise they are
				// rotated by 90 degrees. Need to set this _after_ addOutput()!
				var captureConnection = videoOutput.ConnectionFromMediaType(AVMediaType.Video);
				captureConnection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
			}
			else if (captureSession.Outputs.Contains(videoOutput))
			{
				captureSession.RemoveOutput(videoOutput);
			}
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

		void PhotoCapturedHandler(byte[] bytes)
		{
			PhotoCaptured?.Invoke(this, new ImageBytesCaptureEvent(bytes));
		}

		void FrameCapturedHandler(CVPixelBuffer buffer)
		{
			FrameCaptured?.Invoke(this, new CVPixelBufferCapturedEvent(buffer));
		}
	}
}

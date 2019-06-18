using System;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using XCamera.Shared;

namespace XCamera.iOS
{
	public class UIXCameraView : UIView
	{
		AVCaptureVideoPreviewLayer previewLayer;
		AVCaptureStillImageOutput output;
		AVCaptureDeviceInput captureDeviceInput;

		public CameraOptions CameraOption { get; private set; }
		public AVCaptureSession CaptureSession { get; private set; }

		public bool IsPreviewing { get; private set; }

		public UIXCameraView(CameraOptions options)
		{
			CameraOption = options;
			IsPreviewing = false;
			Initialize();
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			previewLayer.Frame = rect;
		}

		public void StartPreviewing()
		{
			if (IsPreviewing)
				return;

			CaptureSession.StartRunning();
			IsPreviewing = true;
			previewLayer.Hidden = false;
		}

		public void StopPreviewing()
		{
			if (!IsPreviewing)
				return;

			CaptureSession.StopRunning();
			IsPreviewing = false;
			previewLayer.Hidden = true;
		}

		public AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
		{
			var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

			foreach (var device in devices)
			{
				if (device.Position == orientation)
				{
					return device;
				}
			}

			return null;
		}

		public async Task<byte[]> Capture()
		{
			var buffer = await output.CaptureStillImageTaskAsync(output.Connections[0]);
			NSData imageData = AVCaptureStillImageOutput.JpegStillToNSData(buffer);

			byte[] myByteArray = new byte[imageData.Length];
			System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
			return myByteArray;
		}

		void Initialize()
		{
			CaptureSession = new AVCaptureSession();
			previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession);
			previewLayer.Frame = Bounds;
			previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
			previewLayer.Orientation = AVCaptureVideoOrientation.Portrait;

			var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
			var cameraPosition = (CameraOption == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
			var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

			if (device == null)
			{
				return;
			}

			ConfigureCameraForDevice(device);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
			CaptureSession.AddInput(captureDeviceInput);
			Layer.AddSublayer(previewLayer);

			output = new AVCaptureStillImageOutput { OutputSettings = new NSDictionary(AVVideo.CodecKey, AVVideo.CodecJPEG) };
			CaptureSession.AddOutput(output);
		}

		public void UpdateCameraOption(CameraOptions option)
		{
			if (CameraOption == option)
				return;

			CameraOption = option;

			var cameraPosition = (CameraOption == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
			var device = GetCameraForOrientation(cameraPosition);
			ConfigureCameraForDevice(device);

			CaptureSession.BeginConfiguration();
			CaptureSession.RemoveInput(captureDeviceInput);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
			CaptureSession.AddInput(captureDeviceInput);
			CaptureSession.CommitConfiguration();
		}

		public void ConfigureCameraForDevice(AVCaptureDevice device)
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
	}
}

using System;
using XCamera.Shared.Events;

namespace XCamera.Shared.Interfaces
{
	public interface INativeCameraView
	{
		void Initialize();
		void Capture();
		void StartPreview();
		void StopPreview();
		void SetFrameRate(int frameRate);
		bool CaptureFrames { get; set; }
		CameraOptions CameraOption { get; set; }
		event NativeImageCaptureEventHandler PhotoCaptured;
		event NativeImageCaptureEventHandler FrameCaptured;

	}
}

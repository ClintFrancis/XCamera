using System;
using XCamera.Shared.Events;

namespace XCamera.Shared.Interfaces
{
	public interface INativeCameraView
	{
		void Capture();
		void StartPreview();
		void StopPreview();
		void SetFrameRate(int frameRate);
		CameraOptions CameraOption { get; set; }
		event ImageCapturedEventHandler ImageCaptured;
	}
}

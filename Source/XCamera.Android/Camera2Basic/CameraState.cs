using System;
namespace Camera2Basic
{
	public enum CameraState
	{
		Stopped,
		Preview,
		WaitingLock,
		WaitingPrecapture,
		WaitingNonPrecapture,
		PictureTaken
	}
}

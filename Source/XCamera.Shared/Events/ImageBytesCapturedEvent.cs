using System;
using XCamera.Shared.Interfaces;

namespace XCamera.Shared.Events
{
	public delegate void ImageBytesCapturedEventHandler(object sender, ImageBytesCaptureEvent e);

	public class ImageBytesCaptureEvent : NativeImageCaptureEvent
	{
		byte[] rawData;

		public ImageBytesCaptureEvent(byte[] data)
		{
			rawData = data;
		}

		public override byte[] GetBytes()
		{
			return rawData;
		}

		public override object GetRaw()
		{
			return rawData;
		}
	}
}

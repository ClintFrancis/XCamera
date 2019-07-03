using System;
using CoreVideo;
using XCamera.Shared.Events;

namespace XCamera.Events
{
	public class CVPixelBufferCapturedEvent : NativeImageCaptureEvent
	{
		CVPixelBuffer pixelBuffer;

		public CVPixelBufferCapturedEvent(CVPixelBuffer buffer)
		{
			pixelBuffer = buffer;
		}

		public override byte[] GetBytes()
		{
			throw new NotImplementedException();
		}

		public override object GetRaw()
		{
			return pixelBuffer;
		}
	}
}

using System;
using CoreVideo;
using XCamera.Shared.Events;

namespace XCamera.Events
{
	public class CVPixelBufferCapturedEvent : NativeImageCaptureEvent
	{
		static Type NativeType = typeof(CVPixelBuffer);

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

		public override Type GetRawType()
		{
			return NativeType;
		}
	}
}

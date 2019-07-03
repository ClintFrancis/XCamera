using System;
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using ImageIO;
using UIKit;
using Vision;

namespace XCamera
{
	public class XCameraVideoOutputDelegate : NSObject, IAVCaptureVideoDataOutputSampleBufferDelegate
	{
		Action<CVPixelBuffer> bufferOutputhandler;
		NSDictionary options = new NSDictionary();

		public XCameraVideoOutputDelegate(Action<CVPixelBuffer> bufferOutputhandler)
		{
			this.bufferOutputhandler = bufferOutputhandler;
		}

		CGImagePropertyOrientation ExifOrientationFromDeviceOrientation()
		{
			UIDeviceOrientation curDeviceOrientation = UIDevice.CurrentDevice.Orientation;
			CGImagePropertyOrientation exifOrientation;

			switch (curDeviceOrientation)
			{
				case UIDeviceOrientation.PortraitUpsideDown:
					exifOrientation = CGImagePropertyOrientation.Left;
					break;
				case UIDeviceOrientation.LandscapeLeft:
					exifOrientation = CGImagePropertyOrientation.UpMirrored;
					break;
				case UIDeviceOrientation.LandscapeRight:
					exifOrientation = CGImagePropertyOrientation.Down;
					break;
				case UIDeviceOrientation.Portrait:
					exifOrientation = CGImagePropertyOrientation.Up;
					break;
				default:
					exifOrientation = CGImagePropertyOrientation.Left;
					break;
			}

			return exifOrientation;
		}

		[Export("captureOutput:didOutputSampleBuffer:fromConnection:")]
		public virtual void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			CVPixelBuffer pixelBuffer = null;
			VNImageRequestHandler imageRequestHandler = null;

			try
			{
				pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer;
				if (pixelBuffer == null)
				{
					return;
				}

				// TODO See if this causes issues disposing directly after
				bufferOutputhandler.Invoke(pixelBuffer);
			}
			catch (Exception x)
			{
				Console.WriteLine(x.Message);
			}
			finally
			{
				if (sampleBuffer != null)
				{
					sampleBuffer.Dispose();
				}

				if (pixelBuffer != null)
				{
					pixelBuffer.Dispose();
				}

				if (imageRequestHandler != null)
				{
					imageRequestHandler.Dispose();
				}
			}
		}
	}
}

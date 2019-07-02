using System;
namespace XCamera.Shared.Events
{
	public delegate void ImageCapturedEventHandler(object sender, ImageCapturedEventArgs e);

	public class ImageCapturedEventArgs : EventArgs
	{
		public byte[] Data { get; private set; }

		public ImageCapturedEventArgs(byte[] data)
		{
			Data = data;
		}
	}

}

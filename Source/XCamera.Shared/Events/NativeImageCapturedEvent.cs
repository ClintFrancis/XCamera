using System;
using XCamera.Shared.Interfaces;

namespace XCamera.Shared.Events
{
	public delegate void NativeImageCaptureEventHandler(object sender, NativeImageCaptureEvent e);

	public abstract class NativeImageCaptureEvent : EventArgs
	{
		protected NativeImageCaptureEvent() { }

		public abstract byte[] GetBytes();

		public abstract object GetRaw();

		public abstract Type GetRawType();

	}
}

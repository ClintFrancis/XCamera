using System;
using Xamarin.Forms;
using XCamera.Shared.Events;
using XCamera.Shared.Interfaces;

namespace XCamera.Shared
{
	public class XCameraView : View
	{
		public event EventHandler<EventArgs> CameraReady;

		#region Bindable Properties
		public static readonly BindableProperty CameraProperty = BindableProperty.Create(
			propertyName: nameof(CameraOption),
			returnType: typeof(CameraOptions),
			declaringType: typeof(XCameraView),
			defaultValue: CameraOptions.Rear,
			defaultBindingMode: BindingMode.OneWay,
			propertyChanged: CameraPropertyChanged
		);

		public CameraOptions CameraOption
		{
			get { return (CameraOptions)GetValue(CameraProperty); }
			set { SetValue(CameraProperty, value); }
		}

		private static void CameraPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var control = (XCameraView)bindable;
			control.CameraOption = (CameraOptions)newValue;
		}

		public static readonly BindableProperty AutoVisibleProperty = BindableProperty.Create(
			propertyName: nameof(AutoVisible),
			returnType: typeof(bool),
			declaringType: typeof(XCameraView),
			defaultValue: false);

		public bool AutoVisible
		{
			get { return (bool)GetValue(AutoVisibleProperty); }
			set { SetValue(AutoVisibleProperty, value); }
		}

		public static readonly BindableProperty CaptureFramesProperty = BindableProperty.Create(
			propertyName: nameof(CaptureFrames),
			returnType: typeof(bool),
			declaringType: typeof(XCameraView),
			defaultValue: false);


		public bool CaptureFrames
		{
			get { return (bool)GetValue(CaptureFramesProperty); }
			set { SetValue(CaptureFramesProperty, value); }
		}

		public static readonly BindableProperty FrameRateProperty = BindableProperty.Create(
			propertyName: nameof(FrameRate),
			returnType: typeof(int),
			declaringType: typeof(XCameraView),
			defaultValue: 60);

		public int FrameRate
		{
			get { return (int)GetValue(FrameRateProperty); }
			set { SetValue(FrameRateProperty, value); }
		}

		public static readonly BindableProperty PhotoCapturedProperty = BindableProperty.Create(
			propertyName: nameof(PhotoCaptured),
			returnType: typeof(NativeImageCaptureEventHandler),
			declaringType: typeof(XCameraView));

		public NativeImageCaptureEventHandler PhotoCaptured
		{
			get { return (NativeImageCaptureEventHandler)GetValue(PhotoCapturedProperty); }
			set
			{
				SetValue(PhotoCapturedProperty, value);
			}
		}

		public static readonly BindableProperty FrameCapturedProperty = BindableProperty.Create(
			propertyName: nameof(FrameCaptured),
			returnType: typeof(NativeImageCaptureEventHandler),
			declaringType: typeof(XCameraView));

		public NativeImageCaptureEventHandler FrameCaptured
		{
			get { return (NativeImageCaptureEventHandler)GetValue(FrameCapturedProperty); }
			set
			{
				SetValue(FrameCapturedProperty, value);
			}
		}

		#endregion

		INativeCameraView cameraInstance;

		public void SetNativeCamera(INativeCameraView nativeCamera)
		{
			cameraInstance = nativeCamera;
			CameraReady?.Invoke(this, EventArgs.Empty);
		}

		public void Capture()
		{
			cameraInstance?.Capture();
		}

		public void StartPreview()
		{
			cameraInstance?.StartPreview();
		}

		public void StopPreview()
		{
			cameraInstance?.StopPreview();
		}
	}
}
using System;
using System.Windows.Input;
using Xamarin.Forms;
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

		// File Path callback
		public static readonly BindableProperty CaptureBytesCallbackProperty = BindableProperty.Create(
			propertyName: nameof(CaptureBytesCallback),
			returnType: typeof(Action<byte[]>),
			declaringType: typeof(XCameraView),
			defaultValue: null);

		public Action<byte[]> CaptureBytesCallback
		{
			get { return (Action<byte[]>)GetValue(CaptureBytesCallbackProperty); }
			set { SetValue(CaptureBytesCallbackProperty, value); }
		}

		public static readonly BindableProperty CaptureCommandProperty = BindableProperty.Create(
			propertyName: nameof(CaptureCommand),
			returnType: typeof(ICommand),
			declaringType: typeof(XCameraView),
			defaultValue: null);

		public ICommand CaptureCommand
		{
			get { return (ICommand)GetValue(CaptureCommandProperty); }
			set
			{
				SetValue(CaptureCommandProperty, value);
			}
		}

		public static readonly BindableProperty StartCommandProperty = BindableProperty.Create(
			propertyName: nameof(StartCameraCommand),
			returnType: typeof(ICommand),
			declaringType: typeof(XCameraView),
			defaultValue: null);

		public ICommand StartCameraCommand
		{
			get { return (ICommand)GetValue(StartCommandProperty); }
			set
			{
				SetValue(StartCommandProperty, value);
			}
		}

		public static readonly BindableProperty StopCommandProperty = BindableProperty.Create(
			propertyName: nameof(StopCameraCommand),
			returnType: typeof(ICommand),
			declaringType: typeof(XCameraView));

		public ICommand StopCameraCommand
		{
			get { return (ICommand)GetValue(StopCommandProperty); }
			set
			{
				SetValue(StopCommandProperty, value);
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

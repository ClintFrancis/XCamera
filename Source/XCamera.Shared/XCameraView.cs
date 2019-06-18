using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace XCamera.Shared
{
	public static class CameraPropertyIds
	{
		public static string CameraOption = "CameraOption";
		public static string Width = "Width";
		public static string Height = "Height";
	}

	public class XCameraView : View
	{
		public event EventHandler<EventArgs> CameraReady;

		#region Bindable Properties
		public static readonly BindableProperty CameraProperty = BindableProperty.Create(
			propertyName: CameraPropertyIds.CameraOption,
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
			propertyName: "AutoVisible",
			returnType: typeof(bool),
			declaringType: typeof(XCameraView),
			defaultValue: false);

		public bool AutoVisible
		{
			get { return (bool)GetValue(AutoVisibleProperty); }
			set { SetValue(AutoVisibleProperty, value); }
		}

		// File Path callback
		public static readonly BindableProperty CaptureBytesCallbackProperty = BindableProperty.Create(
			propertyName: "CaptureBytesCallback",
			returnType: typeof(Action<byte[]>),
			declaringType: typeof(XCameraView),
			defaultValue: null);

		public Action<byte[]> CaptureBytesCallback
		{
			get { return (Action<byte[]>)GetValue(CaptureBytesCallbackProperty); }
			set { SetValue(CaptureBytesCallbackProperty, value); }
		}

		public static readonly BindableProperty CaptureCommandProperty = BindableProperty.Create(
			propertyName: nameof(CaptureCommandProperty),
			returnType: typeof(ICommand),
			declaringType: typeof(XCameraView),
			defaultValue: null);

		public ICommand Capture
		{
			get { return (ICommand)GetValue(CaptureCommandProperty); }
			set
			{
				SetValue(CaptureCommandProperty, value);
				CheckIsReady();
			}
		}

		//public static readonly BindableProperty StartCommandProperty = BindableProperty.Create(
		//	propertyName: nameof(StartCamera),
		//	returnType: typeof(ICommand),
		//	declaringType: typeof(XCameraView),
		//	defaultValue: null);

		public static readonly BindableProperty StartCommandProperty = BindableProperty.Create<XCameraView, ICommand>(p => p.StartCamera, null);

		public ICommand StartCamera
		{
			get { return (ICommand)GetValue(StartCommandProperty); }
			set
			{
				SetValue(StartCommandProperty, value);
				CheckIsReady();
			}
		}

		public static readonly BindableProperty StopCommandProperty = BindableProperty.Create(
			nameof(StopCamera),
			typeof(ICommand),
			typeof(XCameraView));

		public ICommand StopCamera
		{
			get { return (ICommand)GetValue(StopCommandProperty); }
			set
			{
				SetValue(StopCommandProperty, value);
				CheckIsReady();
			}
		}
		#endregion

		public XCameraView()
		{

		}

		void CheckIsReady()
		{
			if (StartCommandProperty != null && CaptureCommandProperty != null && StopCommandProperty != null)
			{
				CameraReady?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}

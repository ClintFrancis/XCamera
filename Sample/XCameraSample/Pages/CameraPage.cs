using System;
using System.IO;
using FFImageLoading.Forms;
using Plugin.Media;
using Xamarin.Forms;
using XCamera.Shared;
using XCameraSample.Utils;
using XCameraSample.ViewModels;

namespace XCameraSample.Pages
{
	public class CameraPage : ContentPage
	{
		private RelativeLayout layout;
		private XCameraView cameraPreview;
		private Label messageLabel;
		private Label titleLabel;
		CameraOptions cameraOption;
		CaptureViewModel captureModel;
		CachedImage cachedCapture;

		public CameraPage()
		{
			BindingContext = captureModel = new CaptureViewModel();
			Title = "Camera";
			layout = new RelativeLayout();

			if (CrossMedia.Current.IsCameraAvailable)
			{
				BackgroundColor = Color.Black;

				// Camera Preview
				cameraPreview = new XCameraView();
				//cameraPreview.CameraOption = Settings.CameraOption;
				cameraPreview.CaptureBytesCallback = new Action<byte[]>(ProcessCameraPhoto);
				cameraPreview.CameraReady += (s, e) => StartCamera();

				layout.Children.Add(cameraPreview,
					Constraint.Constant(0),
					Constraint.RelativeToParent((parent) =>
					{
						var viewHeight = MathUtils.FitSize4X3(parent.Width, parent.Height).Height;
						return parent.Height / 2 - viewHeight / 2;
					}),
					Constraint.RelativeToParent((parent) =>
					{
						return MathUtils.FitSize4X3(parent.Width, parent.Height).Width;
					}),
					Constraint.RelativeToParent((parent) =>
					{
						return MathUtils.FitSize4X3(parent.Width, parent.Height).Height;
					}));

				// Capture Button
				var buttonSize = 60;
				var captureButton = new Button();
				captureButton.Clicked += CaptureButton_Clicked;
				captureButton.BackgroundColor = Color.LightGray.MultiplyAlpha(.5);
				captureButton.WidthRequest = buttonSize;
				captureButton.HeightRequest = buttonSize;
				captureButton.CornerRadius = buttonSize / 2;
				captureButton.BorderWidth = 1;
				captureButton.BorderColor = Color.DarkGray;
				captureButton.HorizontalOptions = LayoutOptions.Center;

				layout.Children.Add(captureButton,
					Constraint.RelativeToParent((parent) => { return (parent.Width * .5) - (buttonSize * .5); }),
					Constraint.RelativeToParent((parent) => { return (parent.Height * .9) - (buttonSize * .5); }));

				// Last Capture
				cachedCapture = new CachedImage();
				cachedCapture.Aspect = Aspect.AspectFill;
				cachedCapture.BackgroundColor = Color.White;

				layout.Children.Add(cachedCapture,
					Constraint.Constant(20),
					Constraint.RelativeToView(captureButton, (parent, sibling) => { return sibling.Y; }),
					Constraint.Constant(buttonSize),
					Constraint.Constant(buttonSize));

				this.ToolbarItems.Add(
					new ToolbarItem("Toggle", null, () => ToggleCamera()) { Icon = "toggle.png" }
				);
			}

			else
			{
				messageLabel = new Label()
				{
					WidthRequest = 300,
					TextColor = Color.SlateGray,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				};
				messageLabel.Text = "The camera not supported on this device.";

				layout.Children.Add(messageLabel,
				Constraint.RelativeToParent((parent) =>
				{
					return (parent.Width * .5) - (messageLabel.Width / 2);
				}),
					Constraint.RelativeToParent((parent) =>
					{
						return (parent.Height * .8) - (messageLabel.Height);
					})
				);

				titleLabel = new Label()
				{
					WidthRequest = 300,
					HeightRequest = 20,
					FontAttributes = FontAttributes.Bold,
					TextColor = Color.Black,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				};
				titleLabel.Text = "No Camera";
				layout.Children.Add(titleLabel,
				Constraint.RelativeToParent((parent) =>
				{
					return (parent.Width * .5) - (titleLabel.Width / 2);
				}),
					Constraint.RelativeToView(messageLabel, (parent, sibling) =>
					{
						return messageLabel.Y - titleLabel.Height - 10;
					})
				);
			}

			Content = layout;
		}

		void CaptureButton_Clicked(object sender, EventArgs e)
		{
			if (cameraPreview != null && cameraPreview.Capture != null)
				cameraPreview.Capture.Execute(null);
		}

		void ToggleCamera()
		{
			if (cameraPreview != null && cameraPreview.Capture != null)
				cameraPreview.CameraOption = (cameraPreview.CameraOption == CameraOptions.Rear) ? CameraOptions.Front : CameraOptions.Rear;
		}

		void StopCamera()
		{
			if (CrossMedia.Current.IsCameraAvailable && cameraPreview != null && cameraPreview.StopCamera != null)
				cameraPreview.StopCamera.Execute(null);
		}

		void StartCamera()
		{
			if (CrossMedia.Current.IsCameraAvailable && cameraPreview != null && cameraPreview.StartCamera != null)
			{
				cameraPreview.CameraOption = cameraOption;
				cameraPreview.StartCamera.Execute(null);
			}
		}

		void ProcessCameraPhoto(byte[] imageBytes)
		{
			var filename = captureModel.SaveBytes(imageBytes);

			cachedCapture.Source = ImageSource.FromStream(() =>
			{
				return new MemoryStream(imageBytes);
			});
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			//StartCamera();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			StopCamera();
			//if (cachedCapture != null)
			//	cachedCapture.Source = null;
		}
	}
}


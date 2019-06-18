
using Android;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;

namespace Camera2Basic
{
	public class ConfirmationDialog : DialogFragment
	{
		//private static Fragment mParent;
		private class PositiveListener : Java.Lang.Object, IDialogInterfaceOnClickListener
		{
			public void OnClick(IDialogInterface dialog, int which)
			{
				//FragmentCompat.RequestPermissions(mParent,
				//new string[] { Manifest.Permission.Camera }, Camera2BasicFragment.REQUEST_CAMERA_PERMISSION);
			}
		}

		private class NegativeListener : Java.Lang.Object, IDialogInterfaceOnClickListener
		{
			public void OnClick(IDialogInterface dialog, int which)
			{
				//var activity = mParent.Activity;
				//if (activity != null)
				//{
				//	activity.Finish();
				//}
			}
		}

		public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			//mParent = ParentFragment;
			//return new AlertDialog.Builder(Activity)
			//.SetMessage(Resource.String.request_permission)
			//.SetPositiveButton(Android.Resource.String.Ok, new PositiveListener())
			//.SetNegativeButton(Android.Resource.String.Cancel, new NegativeListener())
			//.Create();

			return null;
		}
	}
}
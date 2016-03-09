using System;
using Foundation;
using UIKit;
using MixpanelBinding;
using CoreFoundation;

namespace HelloMixpanel
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate, IMixpanelDelegate
	{
		const string _mixpanelTokenKey = "mixpanelToken";

		// IMPORTANT!!! replace with you api token from https://mixpanel.com/account/
		// const string MIXPANEL_TOKEN = "YOUR_MIXPANEL_PROJECT_TOKEN";
		const string MIXPANEL_TOKEN = "fd506b4c1c6426c57c0dbea08dca7aee";

		DateTime startTime;

		public Mixpanel mixpanel { get; set; }

		nint bgTask;

		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			NSUserDefaults.StandardUserDefaults.RegisterDefaults (NSDictionary.FromObjectAndKey (new NSString (_mixpanelTokenKey), new NSString (MIXPANEL_TOKEN)));

			var mixpanelToken = NSUserDefaults.StandardUserDefaults.StringForKey (_mixpanelTokenKey);

			Window.MakeKeyAndVisible ();

			if (string.IsNullOrEmpty (mixpanelToken) || mixpanelToken.Equals ("YOUR_MIXPANEL_PROJECT_TOKEN", StringComparison.OrdinalIgnoreCase)) {

#if DEBUG
				var alert = UIAlertController.Create ("Mixpanel Token Required", "Go to Settings > Mixpanel and add your project's token", UIAlertControllerStyle.Alert);

				alert.AddAction (UIAlertAction.Create ("Okay", UIAlertActionStyle.Default, (obj) => UIApplication.SharedApplication.OpenUrl (NSUrl.FromString (UIApplication.OpenSettingsUrlString))));

				Window.RootViewController.PresentViewController (alert, true, null);

				return true;

#endif

			} else {

				// Initialize the MixpanelAPI object
				mixpanel = Mixpanel.SharedInstanceWithToken (mixpanelToken, launchOptions);
				Console.WriteLine (mixpanel);
			}


			mixpanel.CheckForSurveysOnActive = true;
			mixpanel.ShowSurveyOnActive = true; // Change this to false to show your surveys manually.

			mixpanel.CheckForNotificationsOnActive = true;
			mixpanel.ShowNotificationOnActive = true; //Change this to false to show your notifs manually.

			// Set the upload interval to 20 seconds for demonstration purposes. This would be overkill for most applications.
			mixpanel.FlushInterval = 20; // defaults to 60 seconds

			// Set some super properties, which will be added to every tracked event
			mixpanel.RegisterSuperProperties (NSDictionary.FromObjectAndKey (new NSString ("Plan"), new NSString ("Premium")));

			// Name a user in Mixpanel Streams
			mixpanel.NameTag = "Walter Sobchak";


			var userNotificationSettings = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Badge | UIUserNotificationType.Sound | UIUserNotificationType.Alert, null);

			UIApplication.SharedApplication.RegisterUserNotificationSettings (userNotificationSettings);

			return true;
		}


		#region Session timing example

		public override void OnResignActivation (UIApplication application)
		{
			Console.WriteLine ("Will resign active");

			var seconds = DateTime.Now.Subtract (startTime).TotalSeconds;

			Mixpanel.SharedInstance?.Track ("Session", NSDictionary.FromObjectAndKey (new NSString ("Length"), NSNumber.FromDouble (seconds)));
		}

		public override void DidEnterBackground (UIApplication application)
		{
			bgTask = application.BeginBackgroundTask (() => {

				application.EndBackgroundTask (bgTask);
				bgTask = UIApplication.BackgroundTaskInvalid;

			});

			DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default).DispatchAsync (() => {

				Console.WriteLine ("Starting background task {0}", bgTask);

				// track some events and set some people properties
				var mixpanel = Mixpanel.SharedInstance;

				mixpanel?.RegisterSuperProperties (NSDictionary.FromObjectAndKey (new NSString ("Background Super Property"), new NSString ("Hi!")));
				mixpanel?.Track ("Background Event");
				mixpanel?.People.Set ("Background Property", new NSDate ());

				Console.WriteLine ("Ending background task {0}", bgTask);
				application.EndBackgroundTask (bgTask);
				bgTask = UIApplication.BackgroundTaskInvalid;

			});

			Console.WriteLine ("Dispatched background task {0}", bgTask);
		}

		public override void OnActivated (UIApplication application)
		{
			startTime = DateTime.Now;
		}

		#endregion


		#region Push Notifications

		public override void DidRegisterUserNotificationSettings (UIApplication application, UIUserNotificationSettings notificationSettings)
		{
			application.RegisterForRemoteNotifications ();
		}

		public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
		{
			mixpanel.People.AddPushDeviceToken (deviceToken);
		}

		public override void FailedToRegisterForRemoteNotifications (UIApplication application, NSError error)
		{
			Console.WriteLine (error);
		}

		public override void DidReceiveRemoteNotification (UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			var payload = userInfo.ValueForKey (new NSString ("apn")) as NSDictionary;
			var alertString = payload.ValueForKey (new NSString ("alert"))?.ToString ();

			var alert = UIAlertController.Create ("", alertString, UIAlertControllerStyle.Alert);

			alert.AddAction (UIAlertAction.Create ("Okay", UIAlertActionStyle.Default, null));

			Window.RootViewController.PresentViewController (alert, true, null);

			mixpanel.TrackPushNotification (userInfo);
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public class LCGoogleSignEditor : AssetPostprocessor
{
	static LCGoogleSignEditor()
	{
		LoadCLientSpecificPostProcessor();
	}

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath) {
		foreach (string asset in importedAssets) {
			if (asset.Contains("JarResolver") || asset.Contains("IOSResolver") ) {
				LoadCLientSpecificPostProcessor();
				return;
			}
		}
	}

	static void LoadCLientSpecificPostProcessor()
	{
		#if UNITY_IOS
		Type googleiOSResolver = Google.VersionHandler.FindClass("Google.IOSResolver", "Google.IOSResolver");
		if (googleiOSResolver != null) {
			Google.VersionHandler.InvokeStaticMethod(googleiOSResolver, "AddPod", new object[] { "Google/SignIn" }, null);
		}

		#elif UNITY_ANDROID
		Type googlAndPlayServicesResolver = Google.VersionHandler.FindClass("Google.JarResolver", "Google.JarResolver.PlayServicesSupport");
		if (googlAndPlayServicesResolver == null) {
			return;
		}
		object svcSupport = Google.VersionHandler.InvokeStaticMethod(
			googlAndPlayServicesResolver, "CreateInstance", 
			new object[] { "LCGoogleLoginUnity", EditorPrefs.GetString("AndroidSdkRoot"), "ProjectSettings"
			});

		//play-services-auth version: Format: "11.0.4", "11.2.0", "11.4.0", "LATEST"
		//play-services Version : 11.2.0 is the version used by Firebase too so in future firebase changes it and you are using firebase sdk
		//It should be changed to that version. We have tested this till 11.4.0 which is latest at the moment.
		//If this version is incorrect you will never see play services getting resolved properly & it might or might not throw any error
		//If you wanna see what Facebook is using, just browse

		Google.VersionHandler.InvokeInstanceMethod(
			svcSupport, "DependOn",
			new object[] { "com.google.android.gms", "play-services-auth", "11.2.0" },
			namedArgs: new Dictionary<string, object>() {
				{"packageIds", new string[] { "extra-google-m2repository", "extra-android-m2repository"} }
			});

		//design version: Format: "25.3.1", "26.4.0", "LATEST"
		//25.3.1 is the version used by Facebook too so in future facebook changes it and you are using facebook sdk
		//It should be changed to that version. 26.x version are not recommended at the moment
		//If this version is incorrect you will never see play services getting resolved properly & it might or might not throw any error
		//If you wanna see what firebase is using, just search "com.android.support" in your project
		Google.VersionHandler.InvokeInstanceMethod(
			svcSupport, "DependOn",
			new object[] { "com.android.support", "design", "25.3.1" },
			namedArgs: new Dictionary<string, object>() {
				{"packageIds", new string[] { "extra-google-m2repository", "extra-android-m2repository"} }
			});

		/***** IMPORTANT FACEBOOK SDK users ******/
		// If you are using facebook & getting any error related to unavailable resources, uncomment following section.
		// Check our play services video for more details
		// https://github.com/facebook/facebook-sdk-for-unity/blob/master/Facebook.Unity.Editor/android/AndroidSupportLibraryResolver.cs
		/*
		Google.VersionHandler.InvokeInstanceMethod(
			svcSupport, "DependOn",
			new object[] { "com.android.support", "cardview-v7", "25.3.1" },
			namedArgs: new Dictionary<string, object>() {
				{"packageIds", new string[] { "extra-google-m2repository", "extra-android-m2repository"} }
			});

		Google.VersionHandler.InvokeInstanceMethod(
			svcSupport, "DependOn",
			new object[] { "com.android.support", "customtabs", "25.3.1" },
			namedArgs: new Dictionary<string, object>() {
				{"packageIds", new string[] { "extra-google-m2repository", "extra-android-m2repository"} }
			});
		*/

		#endif
	}
}


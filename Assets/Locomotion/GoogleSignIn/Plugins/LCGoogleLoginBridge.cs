using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using LCGoogleLogin;

public class LCGoogleLoginBridge : MonoBehaviour {

	#region Lifecycle

	private static LCGoogleLoginBridge instance;

	/** 
	 * InitWithClientID() basics
	 * Its safe to pass null here if both of following values are getting picked up properly by google services files.
	 * iOS > client ID automatically picked from GoogleService-Info.plist so any value passed won't have any effect
	 * Android > you need to use web client id, it will be picked up from R.string.default_web_client_id if set by any library. Firebase does set default_web_client_id 
	 * in a file located @ Plugins/Android/Firebase/res/values/google-services.xml 
	 **/

	public static void InitWithClientIDDefault(){
		InitWithClientID (null);
	}

	public static void InitWithClientID(string clientID){
		if (instance == null) { 
			instance = FindObjectOfType( typeof(LCGoogleLoginBridge) ) as LCGoogleLoginBridge;
			if(instance == null) {
				instance = new GameObject("LCGoogleLoginBridge").AddComponent<LCGoogleLoginBridge>();

				if (string.IsNullOrEmpty (clientID)) {
					Debug.LogError ("LCGoogleLoginBridge: InitWithClientID: Google Web Client ID is required");
					return;
				}

				if (Application.platform == RuntimePlatform.Android) {
					LCGoogleLoginAndroid.InitiateWithClientID (clientID);
				} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
					LCGoogleLoginiOS.initiateWithClientID (clientID);
				} else {
					if (debugLogs) {
						Debug.Log ("LCGoogleLogin: LoginUserBasic: Unsupported platform");
					}
				}
			}
		}
	}

	//Set Logs to true, if you want to see logs
	static bool debugLogs = false;

	//We need this so that native code has an object to send messages to
	static LCGoogleLoginBridge SharedInstance()
	{
		if(instance == null) {
			InitWithClientIDDefault ();
		}
		return instance;
	}

	void Awake() {
		// Set the name to allow UnitySendMessage to find this object.
		name = "LCGoogleLoginBridge";
		// Make sure this GameObject persists across scenes
		DontDestroyOnLoad(transform.gameObject);
	}
	#endregion




	#region Native library : Login & Logout

	static Action<bool> authCallback;

	public static bool LoginUser(Action<bool> callback, bool isSilent, bool enableServerAuth = false, bool forceCodeForeRefreshToken = false, List<string> requestedScopes = null){
		LCGoogleLoginBridge.SharedInstance ();
		authCallback = callback;
		string[] strScopesArray = null;
		if (requestedScopes == null || requestedScopes.Count <= 0) {
			strScopesArray = new string[0];
		} else {
			strScopesArray = requestedScopes.ToArray();
		}

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidLoginMethod (LCGoogleLoginAndroid.kUserLogin, isSilent, enableServerAuth,
				forceCodeForeRefreshToken, strScopesArray);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			int noOfScopes = (strScopesArray == null) ? 0 : strScopesArray.Length;
			return LCGoogleLoginiOS.userLogin (isSilent, enableServerAuth, forceCodeForeRefreshToken, strScopesArray, noOfScopes);
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: LoginUser: Unsupported platform");
			}
		}
		return false;
	}

	public static bool LogoutUser(){
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidBoolMethod (LCGoogleLoginAndroid.kUserLogout);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userLogout ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: LogoutUser: Unsupported platform");
			}
		}
		return false;
	}
	#endregion




	#region Native library : Access Data like access token etc

	//Profile Access
	public static string GSIUserName()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrUserDisplayName);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userDisplayName ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIUserName: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIUserID()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrUserActualID);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userActualID ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIUserID: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIEmail()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrUserEmail);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userEmail ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIPhotoUrl: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIPhotoUrl()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrUserPhotoUrl);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userPhotoUrl ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIPhotoUrl: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIIDUserToken()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrUserIDToken);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userIDToken ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIIDUserToken: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIAccessToken()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIAccessToken: Always null for android. Check google docs for why so");
			}
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrUserAccessToken);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.userAccessToken ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIIDUserToken: Unsupported platform");
			}
		}

		return null;
	}

	public static string[] GSIGrantedScopes()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringArrayMethod (LCGoogleLoginAndroid.kStrArrScopes);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			IntPtr scopesArray = LCGoogleLoginiOS.avalableScopes ();
			int noScopes = LCGoogleLoginiOS.noOfAvalableScopes ();
			return LCGoogleLoginiOS.GetCListFromiOSNativeSentData (scopesArray, noScopes);
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIGrantedScopes: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIRefreshToken()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIRefreshToken: Always null for android. Check google docs for why so");
			}
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrRefreshToken);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.refreshToken ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIRefreshToken: Unsupported platform");
			}
		}

		return null;
	}

	public static string GSIServerAuthCode()
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidStringMethod (LCGoogleLoginAndroid.kStrServerAuthCode);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return LCGoogleLoginiOS.serverAuthCode ();
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: GSIIDUserToken: Unsupported platform");
			}
		}

		return null;
	}


	public static bool IsLoggedIn(){
		return (GSIUserID () != null) ? true : false;
	}
	#endregion




	#region Native library : Logging Changes

	public static bool ChangeLoggingLevel(bool enabled)
	{
		debugLogs = enabled;

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidInBoolMethod (LCGoogleLoginAndroid.kInBoolChangeLogLevel, enabled);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			LCGoogleLoginiOS.changeLogLevel (enabled);
			return true;
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: ChangeLoggingLevel: Unsupported platform");
			}
		}

		return false;
	}


	static bool ChangeLoggingDevLevel(bool enabled)
	{
		LCGoogleLoginBridge.SharedInstance ();

		if (Application.platform == RuntimePlatform.Android) {
			return LCGoogleLoginAndroid.CallAndroidInBoolMethod (LCGoogleLoginAndroid.kInBoolChangeDevLogLevel, enabled);
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			LCGoogleLoginiOS.changeDevLogLevel (enabled);
			return true;
		} else {
			if (debugLogs) {
				Debug.Log ("LCGoogleLogin: ChangeLoggingDevLevel: Unsupported platform");
			}
		}

		return false;
	}
	#endregion


	// Callback from native client libraries

	#region Callbacks from Native iOS / Android library

	public void LCGoogleSignInSuccess( string args ) {
		if (debugLogs) {
			Debug.Log ("LCGCAuthenticated: Static: Success: " + args);
		}
		if (authCallback != null) {
			authCallback (true);
			authCallback = null;
		}
	}

	public void LCGoogleSignInFailed( string args ) {
		if (debugLogs) {
			Debug.Log ("LCGCAuthenticated: Static: Failed: " + args);
		}
		if (authCallback != null) {
			authCallback (false);
			authCallback = null;
		}
	}

	public void LCGoogleSignedOut( string args ) {
		if (debugLogs) {
			Debug.Log ("LCGoogleSignedOut1: Static: " + args);
		}
		if(authCallback != null){
			if (args == "true") {
				//authCallback (true);
				//authCallback = null;
			} else {
			}
			//authCallback (false);
			//authCallback = null;
		}
	}


	public void LCGoogleSignedOut() {
		if (debugLogs) {
			Debug.Log ("LCGoogleSignedOut2");
		}
	}
	#endregion
}

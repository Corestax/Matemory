using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace LCGoogleLogin
{
	public static class LCGoogleLoginiOS
	{
		// Auth

		[DllImport ("__Internal")] extern static public void initiateWithClientID(string clientID);

		[DllImport ("__Internal")] extern static public bool userLogin(bool isSilent, bool enableServerAuth, 
			bool forceCodeForeRefreshToken, string[] requestedScopes, int noOfScopes);

		[DllImport ("__Internal")] extern static public bool userLogout();

		// Profile Access
		[DllImport ("__Internal")] extern static public string userDisplayName();

		[DllImport ("__Internal")] extern static public string userActualID();

		[DllImport ("__Internal")] extern static public string userEmail();

		[DllImport ("__Internal")] extern static public string userPhotoUrl();

		[DllImport ("__Internal")] extern static public string userIDToken();

		[DllImport ("__Internal")] extern static public string userAccessToken();

		[DllImport ("__Internal")] extern static public string refreshToken();

		[DllImport ("__Internal")] extern static public string serverAuthCode();

		[DllImport ("__Internal")] extern static public IntPtr avalableScopes();
		[DllImport ("__Internal")] extern static public int noOfAvalableScopes();

		//Logging
		[DllImport ("__Internal")] extern static public void changeLogLevel(bool enableLogs);

		[DllImport ("__Internal")] extern static public void changeDevLogLevel(bool enableLogs);



		//Utils
		public static string[] GetCListFromiOSNativeSentData(IntPtr unmanagedArray, int objsCount)
		{
			IntPtr[] intPtrArray = new IntPtr[objsCount];
			string[] retVal = new string[objsCount];
			Marshal.Copy(unmanagedArray, intPtrArray, 0, objsCount);

			for (int i = 0; i < objsCount; i++) {
				retVal[i] = Marshal.PtrToStringAuto(intPtrArray[i]);
				Marshal.FreeHGlobal(intPtrArray[i]);
			}

			Marshal.FreeHGlobal(unmanagedArray);
			return retVal;
		}

	}
}


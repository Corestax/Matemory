# Version 1.3 Upgrade
- Newly built aar for better support
- iOS Build issue fixes for Unity 2017
- play-services-auth hardcoded to "11.2.0" (Change it if Firebase changes it in future)
- design hardcoded to "25.3.1" (Change it if facebook changes it in future)
- New video explaining conflict management with android libraries & unity project

# Version 1.2 Upgrade
- Added server auth and capability to acquire various scopes
- Added design dependency for better compatibility
- Fixed android 4.x compatibility issue

# Version 1.1 Upgrade
- Fixed logout issue in android
- Its cumpulsory to pass web client ID in init method
- Updated console logs
- Added how to video in asset store page

# Unity Package Name
Google SignIn iOS & Android

# Prerequisite
Google Developer account with project setup

# Must Read
You should have done a walk through of official google docs
Android:
https://developers.google.com/identity/sign-in/android/start-integrating

iOS:
https://developers.google.com/identity/sign-in/ios/start-integrating



# Video Walkthrough
A short video walkthrough is available (Older version 1.1) and link could be found in Asset Store page of this package to follow along. Additional Version 1.2 video is coming soon.
Version 1.1: https://www.youtube.com/watch?v=mmLheAYQoO8&t=4s

# Dependencies
1) Play services support package: Verify that you don't already have it via another third party project by looking into folder "PlayServicesResolver/Editor". If you are using firebase libs, its shipped with play services support. If its not the case, you can downlod latest play services support library unity packagae from
https://github.com/googlesamples/unity-jar-resolver

2) Google API Console project: You need google-services.json file for Android & GoogleService-Info.plist file for iOS. Put it in your project Assets folder. You can have only 1 file for iOS & 1 for android at the moment and should place it in Assets folder (no subfolders). You can read about it and get it while setting up your google console project for signin 

Android:
https://developers.google.com/identity/sign-in/android/start-integrating

iOS:
https://developers.google.com/identity/sign-in/ios/start-integrating

For android you also need to setup make sure that web client id is also created (Usually 1 is auto created for you)
https://console.developers.google.com/apis/credentials

3) Initialize
You must configure logging level & initialize librray using web client id. It has no effect on iOS and it will be used in android only. This is to make sure that R.string.default_web_client_id is configured

4) CocoaPods (iOS)
We add 'Google/SignIn' as pod depencies to pull official google signin library for iOS.

5) Make sure that your application is signed & have correct packagaeID as also available in Google API Manager console otherwise you will get 'DEVELOPER_ERROR' in console logs after login callback. 

6) Right click anywhere on your project editor and client Play Services Resolver -> Android Resolver -> Resolve client jar
This will bring all necessary dependencies for android into your project from your SDK installation folder. Advance usage for same is given under #Advance

** If for any reasons you cant see files as listed below under # Advance > 3 after resolving android jar, copy these files manually from your local android sdk installation to a folder Assets / <Any name> / Plugins / Android

# Precaution (Android)
- Always ensure that you have no duplicate aar files throughout your project otherwise it will result in gradle error.
- All support libs must have same versions (25.3.1 at the time of this release, 26.x series in unsupported at the moment)
- All Play services libs must have same version (11.2.0 at the time of this release)


# Usage
You are all set. Ensure that you have correct InitWithClientID setup in LCGoogleSignInExample.cs & checkout LCGoogleSignInExample scene and its script. Run it on device not Editor since it will be ignored.

> Always run on iOS because setup is relatively lot simpler and thr is a high chance that you got it right
> In android, sometimes first run for new project doesnt work and throws an error. We have seen it very frequently but its not documented anywhere so if your first attempt doesnt logs you in, wait for 5 mins, do a fresh install and try again. If that also doesnt work send us to our support email or check error code. If its DEVELOPER_ERROR in console, we can't help u much and u need to figure out project setup on your own.

# Silent login
Its usually used to reestablish session after user comes back but we are not expert on use cases and you must refer to documents given by google for use cases.

# Server auth (V1.2 onwards)
Most of the game don't need it but if you happen to be using google APIs from your server or offline access, pass 'true' in UserLogin method for relevant variable. If user is already logged in without this permission call logout first.

# Scope (V1.2 onwards)
Mostly games don't need this feature but if you happen to be managing any other google service and need special permission as given in following doc, pass it during "UserLogin" call. For changing requests during a session, call logout and thn call login with new permissions
https://developers.google.com/identity/protocols/googlescopes


# Advance
Play services will usually resolve libraries to latest versions but if you want to specify the version your self
1) Check the version of appcompat you want to use @ LCGoogleSignInEditor by replacing relevant "LATEST" text
Path (Mac): /Users/<Your User>/Library/Android/sdk/extras/android/m2repository/com/android/support/appcompat-v7

2) Check play services version you want to use @ LCGoogleSignInEditor by replacing relevant "LATEST" text
Path (Mac): /Users/<Your User>/Library/Android/sdk/extras/google/m2repository/com/google/android/gms

3) Following dependencies are added by play services resolver for sign in as last checked for android. We did not find any necessity for design-25.3.1 and its additional modules to be required for android 5.x onwards however, you must test if you can skip it or not if you are trying to reduce apk size.

animated-vector-drawable-25.3.1.aar
appcompat-v7-25.3.1.aar
design-25.3.1.aar (Usually appcompat-v7 is enough but its recommended by google example probably for Android 4.x compatibility)
play-services-auth-11.2.0.aar
play-services-auth-api-phone-11.2.0.aar (MIght not be compulsory)
play-services-auth-base-11.2.0.aar
play-services-base-11.2.0.aar
play-services-basement-11.2.0.aar
play-services-tasks-11.2.0.aar
recyclerview-v7.aar (If using design additional to appcompat-v7)
support-annotations-25.3.1.jar
support-compat-25.3.1.aar
support-core-ui-25.3.1.aar
support-core-utils-25.3.1.aar
support-fragment-25.3.1.aar
support-media-compat-25.3.1.aar
support-v4-25.3.1.aar
support-vector-drawable-25.3.1.aar
transition-25.3.1.aar (If using design additional to appcompat-v7)





# Setup Validation
## iOS
- Google-Services.plist file is copied and is the file you intnded
- Info.plist contains 2 entries from this lib in URLs (lcgoogle & google). 
    - lcgoogle > contains your bundle identifier
    - google > contains iOS client ID
- Build & verify by running the app

## Android
We have tested both gradle (internal) & gradle (new) for android. Prefer to use workspace for iOS. Its adviced to use gradle (new) because it gives better details on error





# Common Errors & how to solve it
## iOS
- Usually you should never see an error in iOS. If yours is an exception, let us know. Our setup was tested with xcode workspace but anything else should just work fine. 
- If you are using google play games unity library, make sure that you read iOS section properly because it have many incompatibility for new users including the version of google signin it enforces.


## Android
### DEVELOPER_ERROR in logs
It happens for many reason like packagae name, incorrect project configuration, project signing, first time project etc etc. Its adviced to google and understand the specific case for your configuration. 

Try running first on iOS so that your project is set.

Try running google samples android studio project given by google, if you see same issue, you will be able to resolve it faster

Check Advance notes on replacing string "LATEST" with play services & support library version you want to use.

Verify your web client ID

Make sure you have latest Google services file for both iOS & Android

### java.lang.IllegalArgumentException
Uncaught translation error: java.lang.IllegalArgumentException: already added: Lcom/google/android/gms/iid/zzc;
Uncaught translation error: java.lang.IllegalArgumentException: already added: Lcom/google/android/gms/iid/zzd

Its usually caused by multiple aar files with different versions. Verify that your project have only 1 aar file for each android library. It might exist in different folders too.

### com.google.android.gms.auth.api.signin.internal.SignInConfiguration classNotFound
https://stackoverflow.com/questions/33583326/new-google-sign-in-android?noredirect=1&lq=1

### Dex Error 65K limit
gradle (new) seems to have less of this error and you need to optimize your project by either removing few libraries or search on how you can resolve it for your case. Many solutions in stackoverflow totally works.

### Extreme resolution 1
Import firebase auth library and configure sample project. It might just solve the issue but its not a recommended unless you are planning to use it

### Extreme resolution 2
Create fresh unity project and test this module

### Have suggestions for us ?
Let us know and we will update this document to help other developers





# Additional Questions / Trouble Shoot / Reach Out
You can reach us at support@locomotion.co.in 

## Android logs
Use logcat with filters to attach only relevant log
./adb logcat -s Unity ActivityManager PackageManager dalvikvm DEBUG

# iOS Logs
XCode logs will work



//
//  LCGoogleSignIn.h
//  LCUnityiOSPlugins
//
//  Created by Sourabh Verma on 14/06/17.
//  Copyright © 2017 Sourabh Verma. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "AppDelegateListener.h"


//! Project version number for LCGoogleSignIn.
FOUNDATION_EXPORT double LCGoogleSignInVersionNumber;

//! Project version string for LCGoogleSignIn.
FOUNDATION_EXPORT const unsigned char LCGoogleSignInVersionString[];

@interface LCGoogleSignIn : NSObject <AppDelegateListener>

+ (LCGoogleSignIn *)sharedInstance;

extern "C" {
    //static LCGCKit* sharedInstance();
    void initiateWithClientID(const char* webClientID);
    
    bool userLogin(bool isSilent, bool enableServerAuth, bool forceCodeForeRefreshToken, const char* requestedScopes[], int noOfScopes);
    
    bool userLogout();
    
    void changeLogLevel(bool enabled);
    void changeDevLogLevel(bool enabled);
    
    const char* userDisplayName();
    const char* userAccessToken();
    const char* refreshToken();
    const char* userIDToken();
    const char* userActualID();
    const char* serverAuthCode();
    const char* isLoggedIn();
    
    int noOfAvalableScopes();
    const char** avalableScopes();
}

@end

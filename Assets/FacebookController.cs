using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook;

public class FacebookController : MonoBehaviour
{
    
	void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void Init()
    {
        //Facebook.Unity.INIT
    }

    public void OnShareFacebookClicked()
    {
        StartCoroutine(OnShareFacebookClickedCR());        
    }

    private IEnumerator OnShareFacebookClickedCR()
    {
        // Capture screenshot and post to facebook
        CaptureScreenshot();
        while (isCapturingScreenshot)
            yield return null;
        PostToFacebook();
    }

    private bool isCapturingScreenshot;
    private void CaptureScreenshot()
    {
        StartCoroutine(CaptureScreenshotCR());
    }

    private IEnumerator CaptureScreenshotCR()
    {
        isCapturingScreenshot = true;

        yield return null;
        isCapturingScreenshot = false;
    }

    private void PostToFacebook()
    {
        StartCoroutine(PostToFacebookCR());
    }

    private IEnumerator PostToFacebookCR()
    {
        yield return null;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
 
public class LightEstToggleInspector : MaterialEditor
{
	public override void OnInspectorGUI ()
	{
		// Draw the default inspector.
		base.OnInspectorGUI ();
 
		// If we are not visible, return.
		if (!isVisible)
			return;
 
		// Get the current keywords from the material
		Material targetMat = target as Material;
		string[] keyWords = targetMat.shaderKeywords;
 
		// Check to see if the keyword LIGHTEST_ON is set in the material.
		bool lightEstEnabled = keyWords.Contains ("ARCORELIGHT_ON");
		EditorGUI.BeginChangeCheck();
		// Draw a checkbox showing the status of lightEstEnabled
		lightEstEnabled = EditorGUILayout.Toggle ("ARCore light Enabled", lightEstEnabled);
		// If something has changed, update the material.
		if (EditorGUI.EndChangeCheck())
		{
			// If our ARCore light  is enabled, add keyword LIGHTEST_ON, otherwise add LIGHTEST_OFF
			List<string> keywords = new List<string> { lightEstEnabled ? "ARCORELIGHT_ON" : "ARCORELIGHT_OFF"};
			targetMat.shaderKeywords = keywords.ToArray ();
			EditorUtility.SetDirty (targetMat);
		}
	}
}
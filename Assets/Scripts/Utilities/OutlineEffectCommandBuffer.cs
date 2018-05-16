using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Object = System.Object;

public class OutlineEffectCommandBuffer : MonoBehaviour
{
	private Material m_RendMaterail;
	private Material m_Outline;
	private Renderer r_Render;
	private Camera c_Cam;
	
	private Dictionary<Camera, CommandBuffer> c_Cameras = new Dictionary<Camera, CommandBuffer>();

	private void Start()
	{
		r_Render = GetComponent<Renderer>();
		m_Outline = new Material(Shader.Find("Custom/OutlineBuffer"));
		m_Outline.mainTexture = Resources.Load("Dotted_Tile") as Texture;
		//m_RendMaterail = new Material(Shader.Find("Unlit/DrawBuffer"));
	}

	private void Cleanup()
	{
		foreach (var cam in c_Cameras)
		{
			if (cam.Key)
			{
				cam.Key.RemoveCommandBuffer(CameraEvent.AfterEverything, cam.Value);
			}
		}
		c_Cameras.Clear();
		//Object.DestroyImmdiate(rendMaterail);
	}

	public void OnEnable()
	{
		Cleanup();
	}

	public void OnDisable()
	{
		Cleanup();
	}
	// Whenever any camera will render us, add a command buffer to do the work on it
	private void OnWillRenderObject()
	{
		var act = gameObject.activeInHierarchy && enabled;
		if (!act)
		{
			Cleanup();
			return;
		}

		var cam = Camera.current;
		if(!cam)
			return;

		CommandBuffer buffer = null;
		// Did we already add the command buffer on this camera? Nothing to do then.
		if(c_Cameras.ContainsKey(cam))
			return;

		//We Create our own CommandBuffer
		buffer = new CommandBuffer();
		buffer.name = "Grab screen and Outline mesh";
		
		c_Cameras[cam] = buffer;
		
		// Copy screen into temporary RT
		buffer.Clear();
		int screenCopyID = Shader.PropertyToID("_MainTex");
		buffer.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
		buffer.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);
		buffer.ClearRenderTarget(true,true,Color.black);
		
		
		// Get Outline RT
		int OutlineID = Shader.PropertyToID("_OutlineTex");
		buffer.DrawRenderer(r_Render, m_Outline, 0, -1);
		buffer.GetTemporaryRT(OutlineID, -1, -1, 0,FilterMode.Bilinear);
		buffer.Blit(BuiltinRenderTextureType.CurrentActive, OutlineID);
		
		// We return our temp textures back to screen
		buffer.Blit(OutlineID, BuiltinRenderTextureType.CameraTarget, m_RendMaterail);
		
		cam.AddCommandBuffer(CameraEvent.AfterEverything, buffer);
		
		// We reales all temp render textures
		buffer.ReleaseTemporaryRT(screenCopyID);
		buffer.ReleaseTemporaryRT(OutlineID);
	}
}

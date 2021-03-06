﻿using System.Collections.Generic;
using UnityEngine;

public class SnapController : Singleton<SnapController>
{
    [SerializeField]
    private GameObject Prefab_SnapCollider;
    [SerializeField]
    private Transform Container;

    public SnapCollider ActiveSnapCollider { get; set; }

    private AudioManager audioManager;
    private List<SnapCollider> SnapColliders;

	void Start ()
    {
        audioManager = AudioManager.Instance;
        SnapColliders = new List<SnapCollider>();
    }

    private void OnEnable()
    {
        GameController.OnGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameController.OnGameEnded -= OnGameEnded;
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        Clear();
    }

    public void CreateSnapCollider(FruitItem fruitItem, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(Prefab_SnapCollider, pos, rot, Container);
        var snapCollider = go.GetComponent<SnapCollider>();
        SnapColliders.Add(snapCollider);

        // Copy data to collider
        snapCollider.FruitType = fruitItem.Fruit;
        snapCollider.PositionToSnap = fruitItem.PositionToSnap;
        snapCollider.RotiationToSnap = fruitItem.RotationToSnap;
        //snapCollider.Order = fruitItem.Order;
        snapCollider.gameObject.name = "SnapCollider" + SnapColliders.Count;

        // Set snap collider size based on largest side
        Vector3 radius = fruitItem.GetComponent<Collider>().bounds.size;
        float largest = (radius.x < radius.y) ? radius.x : radius.y;
        snapCollider.Size = largest;
    }

    public void Snap(FruitItem fruitItem)
    {
        if (!ActiveSnapCollider || (ActiveSnapCollider && ActiveSnapCollider.IsSnapped))
            return;

        // Mark collider as snapped
        ActiveSnapCollider.Snap(fruitItem.Fruit);

        // Snap to position
        fruitItem.IsSnapped = true;
        fruitItem.GetComponent<Rigidbody>().isKinematic = true;
        fruitItem.transform.localPosition = ActiveSnapCollider.PositionToSnap;
        fruitItem.transform.localRotation = ActiveSnapCollider.RotiationToSnap;

        // Check if game won
        CheckIfComplete();
    }

    public void CheckIfComplete()
    {
        int count = 0;
        foreach(var sc in SnapColliders)
        {
            if (sc.IsSnapped && sc.FruitType == sc.FruitSnapped)
                count++;
        }

        // If game won
        if (SnapColliders.Count == count)
        {
            audioManager.PlaySound(audioManager.audio_modelCompleted);
            GameController.Instance.StopGame(GameController.EndGameTypes.WIN);
        }
    }

    public void ResetAllColliderColorsToIdle()
    {
        foreach(var sc in SnapColliders)
            sc.ShowIdle(false);
    }

    public void EnableColliders()
    {
        foreach (var sc in SnapColliders)
        {
            if(!sc.IsSnapped)
                sc.GetComponent<Collider>().enabled = true;
        }
    }

    public void DisableColliders()
    {
        foreach (var sc in SnapColliders)
            sc.GetComponent<Collider>().enabled = false;
    }

    public void ResetToIdle()
    {
        foreach (var sc in SnapColliders)
        {
            if (!sc.IsSnapped)
                sc.ShowIdle(false);
        }
        ActiveSnapCollider = null;
    }

    public void Clear()
    {
        foreach (var sc in SnapColliders)
            Destroy(sc.gameObject);

        SnapColliders.Clear();
        ActiveSnapCollider = null;
    }
}

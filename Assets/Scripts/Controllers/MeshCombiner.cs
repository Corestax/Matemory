﻿using UnityEngine;

public class MeshCombiner : Singleton<MeshCombiner>
{
    private DynamicOutline dynamicOutline;
    private GameObject combinedObject;

    private void OnEnable()
    {
        GameController.OnGameStarted += Show;
    }

    private void OnDisable()
    {
        GameController.OnGameStarted -= Show;
    }

    public void Show()
    {
        combinedObject.SetActive(true);
    }

    public void Clear()
    {
        Destroy(combinedObject);
        dynamicOutline = null;
    }

    public void CombineMesh(DynamicOutline _dynamicOutline, Material _material = null, Transform _parent = null, bool _enabled = true)
    {
        dynamicOutline = _dynamicOutline;

        combinedObject = new GameObject();
        combinedObject.name = "Combined Mesh of " + _dynamicOutline.gameObject.name;
        combinedObject.transform.position = _dynamicOutline.transform.position;
        combinedObject.transform.eulerAngles = _dynamicOutline.transform.eulerAngles;
        combinedObject.transform.localScale = new Vector3(1, 1, 1);
        combinedObject.SetActive(_enabled);

        if (_parent)
            combinedObject.transform.SetParent(_parent);

        combinedObject.AddComponent<MeshFilter>();
        combinedObject.GetComponent<MeshFilter>().mesh = CombineMeshes(combinedObject, _dynamicOutline.gameObject, false);
        combinedObject.AddComponent<MeshRenderer>();

        combinedObject.GetComponent<MeshRenderer>().material = _material;
        combinedObject.AddComponent<OutlineEffectCommandBuffer>();
    }

    private Mesh CombineMeshes(GameObject copy, GameObject obj, bool outline)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Combined Mesh (" + obj.name + ")";

        Vector3 position = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);

        obj.transform.position = new Vector3(0, 0, 0);

        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i].sharedMesh == null) continue;
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
        }

        mesh.CombineMeshes(combine, true, true);

        //SaveToAssets(mesh, "Assets/DynamicOutline & Mesh Combining/meshes/" + mesh.name + ".asset");

        AddSkins(obj.transform, copy);

        obj.transform.position = position;

        return mesh;
    }

    //private void SaveToAssets(Mesh mesh, string path)
    //{
    //    Debug.Log("outlineeditor: " + path);
    //    Mesh original = AssetDatabase.LoadMainAssetAtPath(path) as Mesh;
    //    if (original == null)
    //    {
    //        AssetDatabase.CreateAsset(mesh, path);
    //        AssetDatabase.SaveAssets();
    //    }
    //    else
    //    {
    //        EditorUtility.CopySerialized(mesh, original);
    //        AssetDatabase.SaveAssets();
    //    }
    //}

    private void AddSkins(Transform transform, GameObject copy)
    {
        foreach (Transform child in transform)
        {
            SkinnedMeshRenderer skin = child.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skin != null)
            {
                GameObject grandchild = new GameObject();
                grandchild.name = "Skinned Mesh (" + child.gameObject.name + ")";
                grandchild.transform.SetParent(copy.transform);

                AddSkin(child.gameObject, grandchild);

                AddSkins(child, grandchild);
            }
            else
            {
                AddSkins(child, copy);
            }
        }
    }

    private bool AddSkin(GameObject original, GameObject copy)
    {
        SkinnedMeshRenderer skin = original.GetComponent<SkinnedMeshRenderer>();
        if (skin != null)
        {
            copy.AddComponent<SkinnedMeshRenderer>();
            //EditorUtility.CopySerialized(skin, copy.GetComponent<SkinnedMeshRenderer>());

            Material[] materials = new Material[copy.GetComponent<SkinnedMeshRenderer>().materials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = dynamicOutline.material;
            }

            copy.GetComponent<SkinnedMeshRenderer>().materials = materials;

            return true;
        }
        else
        {
            return false;
        }
    }
}

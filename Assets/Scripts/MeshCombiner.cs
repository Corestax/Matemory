using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : Singleton<MeshCombiner>
{
    //void Start()
    //{
    //    m_target = GetComponent<DynamicOutline>();

    //}

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.C))
    //        CombineMesh(m_target);
    //}

    //[SerializeField]
    private DynamicOutline dynamicOutline;

    public void CombineMesh(DynamicOutline _dynamicOutline, Material _material = null, Transform _parent = null)
    {
        dynamicOutline = _dynamicOutline;

        GameObject copy = new GameObject();
        copy.name = "Combined Mesh of " + _dynamicOutline.gameObject.name;
        copy.transform.position = _dynamicOutline.transform.position;
        copy.transform.eulerAngles = _dynamicOutline.transform.eulerAngles;
        copy.transform.localScale = new Vector3(1, 1, 1);

        if (_parent)
            copy.transform.SetParent(_parent);

        copy.AddComponent<MeshFilter>();
        copy.GetComponent<MeshFilter>().mesh = CombineMeshes(copy, _dynamicOutline.gameObject, false);
        copy.AddComponent<MeshRenderer>();

        copy.GetComponent<MeshRenderer>().material = _material;
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

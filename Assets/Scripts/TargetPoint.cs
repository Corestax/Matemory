using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    public int Level;

    private Material material;
    private Color color_selected;
    private Color color_unselected_unlocked;
    private Color color_unselected_locked;

    private void Start()
    {
        // Clone material
        material = GetComponentInChildren<MeshRenderer>().material;
        material.CopyPropertiesFromMaterial(new Material(material));

        // Initialize colors
        color_selected = Color.red;
        color_unselected_unlocked = Color.yellow;
        color_unselected_locked = material.color;
    }

    public void SetSelected()
    {
        material.color = color_selected;
    }

    public void SetUnselected()
    {
        if(Level <= LevelsController.Instance.HighestLevel)
            material.color = color_unselected_unlocked;
        else
            material.color = color_unselected_locked;
    }
}

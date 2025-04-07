using UnityEngine;

public class DeleteWallSystem : MonoBehaviour
{
    public WallManagerDebugger wallManagerD;

    private void OnEnable()
    {
        GameEvents.OnCurveDeleted.AddListener(delete_wall);
    }

    private void OnDisable()
    {
        GameEvents.OnCurveDeleted.RemoveListener(delete_wall);
    }

    public void delete_wall(int curve_index)
    {
        //Debug.Log($"🧱 Wall index {curve_index} entry has been removed");

        wallManagerD.wallManager.RemoveWall(curve_index);

        GameObject go = wallManagerD.wallManager.GetSingleWallGameObject(curve_index);
        if (go != null)
        {

            Destroy(go);
        }
        
    }
}

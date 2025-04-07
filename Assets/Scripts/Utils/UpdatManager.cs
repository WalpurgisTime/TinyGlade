using UnityEngine;

public class UpdatManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
        
    void Update()
    {
        if (Input.GetMouseButton(2)) 
        {
            GameEvents.OnMiddleMousePressed.Invoke();
        }

            
        if (Input.GetMouseButtonUp(2)) 
        {
            GameEvents.OnMiddleMouseReleased.Invoke(); 
        }

        
    }

}

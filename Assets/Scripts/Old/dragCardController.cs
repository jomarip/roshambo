using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dragCardController : MonoBehaviour
{
    static dragCardController instance;
    public static dragCardController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<dragCardController>();
            }
            return instance;
        }
    }

	public bool IsActive { get; set; }

    GameObject getTarget;
    bool isMouseDragging;
    Vector3 offsetValue;
    Vector3 positionOfScreen;
   
    void Update()
    {
       
        //Mouse Button Press Down
        if (Input.GetMouseButtonDown(0))
        {
            MouseButtonDownAction();
        }

        //Mouse Button Up
        if (Input.GetMouseButtonUp(0))
        {
            MouseButtonUpAction();
        }

        //Is mouse Moving
        if (isMouseDragging)
        {
            //tracking mouse position.
            Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, positionOfScreen.z);

            //converting screen position to world position with offset changes.
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace);

            getTarget.transform.position = currentPosition;
            //It will update target gameobject's current postion.   
        }
    }

    //Method to Return Clicked Object
    GameObject ReturnClickedObject(out RaycastHit hit)
    {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out hit))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Dragable"))
            {
                target = hit.collider.gameObject;
            }
        }
        return target;
    }

   
    void MouseButtonUpAction()
    {
        if (getTarget != null)
        {
            isMouseDragging = false;
        }
    }

    void MouseButtonDownAction()
    {
		if (IsActive == false)
			return;

        RaycastHit hitInfo;

      
        getTarget = ReturnClickedObject(out hitInfo);

        if (getTarget != null)
        {
            isMouseDragging = true;
            //Converting world position to screen position.
            positionOfScreen = Camera.main.WorldToScreenPoint(getTarget.transform.position);
        }
    }
    public void DropCard()
    {
        isMouseDragging = false;
        MouseButtonUpAction();
    }
}

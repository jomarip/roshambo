using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class returnOriginalPosController : MonoBehaviour
{

    public Vector3 originalPosition;
    public bool isOnPlatform = false;

    public string word;
    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    private void OnMouseUp()
    {
        if(isOnPlatform==false)
        {
            this.gameObject.transform.position = originalPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int flag = 0;
        if (other.gameObject.tag == "PlayerPlatform")
        {
            returnOriginalPosController[] objects = FindObjectsOfType<returnOriginalPosController>();

            for(int i=0;i<objects.Length;i++)
            {
                if(objects[i].isOnPlatform==true)
                {
                    flag = 1;
                    break;
                }
                
            }
            if(flag==0)
            {
                isOnPlatform = true;
                dragCardController.Instance.DropCard();
                this.gameObject.transform.position = other.gameObject.transform.position;
                coundownTimerController.Instance.StopTimer();

				var c = this.GetComponent<CardInfo>();
				c.CardSelected?.Invoke(this, c);
			}
		}
    }

    public void destroyIT()
    {
        Destroy(this.gameObject);
    }

}

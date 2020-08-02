using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aiReturnOriginalPosController : MonoBehaviour
{
    public bool isOnPlatform = false;

    public GameObject[] cardParts;
    public GameObject opositeCard;

    public string word;

    GameObject AiPlatform;

    int flag = 0;

    // Start is called before the first frame update
    void Start()
    { 
        AiPlatform = GameObject.FindGameObjectWithTag("AIPlatform");
    }

    public void MoveTowards()
    {
        if (flag == 0)
        {
            flag = 1;
            StartCoroutine("MoveToPosition");
        }
    }

    IEnumerator MoveToPosition()
    {
        while(true)
        {
            transform.position = Vector3.MoveTowards(transform.position, AiPlatform.gameObject.transform.position, 0.05f);

            if(transform.position== AiPlatform.gameObject.transform.position)
            {
                isOnPlatform = true;

                opositeCard.SetActive(false);
                for(int i=0;i<cardParts.Length;i++)
                {
                    cardParts[i].SetActive(true);
                }

				var c = this.GetComponent<CardInfo>();
				c.CardSelected?.Invoke(this, c);
				break;
            }

            yield return null;
        }
    }

    public void destroyIT()
    {
        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiManagerController : MonoBehaviour
{
    public List<GameObject> aiCards;


    static AiManagerController instance;
    public static AiManagerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AiManagerController>();
            }
            return instance;
        }
    }

    public void SelectRandom()
    {
		aiCards = aiCards.Where(i => i != null).ToList();
        int count = Random.Range(0, aiCards.Count);
        aiCards[count].GetComponent<aiReturnOriginalPosController>().MoveTowards();
        aiCards.Remove(aiCards[count]);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blockchain : MonoBehaviour
{
    public Text addressAndBalance;

    // Start is called before the first frame update
    void Start()
    {
        addressAndBalance.text = "Tron address: "
            + Static.userAddress + "   " + Static.balance.ToString() + " TRX";
    }
}

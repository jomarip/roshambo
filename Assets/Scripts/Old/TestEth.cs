using Nethereum.Contracts;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEth : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
    {
		StartCoroutine("ConnectToEth");
	}

	IEnumerator ConnectToEth() {
		var blockNumberRequest = new EthBlockNumberUnityRequest("https://rinkeby.infura.io");

		yield return blockNumberRequest.SendRequest();

		if (blockNumberRequest.Exception == null) {
			var blockNumber = blockNumberRequest.Result.Value;
			Debug.Log("Block: " + blockNumber.ToString());
		} else {
			Debug.Log(blockNumberRequest.Exception.Message);
			yield break;
		}
		PersonalNewAccountUnityRequest req = new PersonalNewAccountUnityRequest("https://rinkeby.infura.io");
		yield return req.SendRequest("testpass");

		var acc = req.Result;
		//var contract = new Contract(null, ABI, contractAddress);
		Debug.Log("New account: " + acc);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}

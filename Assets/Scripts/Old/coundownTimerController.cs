using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class coundownTimerController : MonoBehaviour
{
	public EventHandler<coundownTimerController> TimeElapsedEvent { get; set; }

    public int flag = 0;

    public Slider slider;

    static coundownTimerController instance;
    public static coundownTimerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<coundownTimerController>();
            }
            return instance;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        slider.maxValue = 5;
        slider.value = 5;
        StartCoroutine("Timer");
    }

    public void RunTimer(float length)
    {
        if (flag == 0)
        {
            slider.gameObject.SetActive(true);
            slider.maxValue = length;
            slider.value = length;
            StartCoroutine("Timer");
        }
    }
    public void StopTimer()
    {
        slider.gameObject.SetActive(false);
        StopCoroutine("Timer");
    }

    IEnumerator Timer()
    {
        while(true)
        {
            slider.value -= 0.01f;
            if(slider.value<=0)
            {
                break;
            }
            yield return new WaitForSeconds(0.01f);
        }
        slider.gameObject.SetActive(false);
		//GameManager.Instance.TimerLoss();
		this.TimeElapsedEvent?.Invoke(this, this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

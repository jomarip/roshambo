using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    private RectTransform spinner;
    public float timeStep;
    public float oneStepAngle;
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        spinner = this.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime >= timeStep)
        {
            Vector3 iconAngle = spinner.localEulerAngles;
            iconAngle.z -= oneStepAngle;
            spinner.localEulerAngles = iconAngle;
            startTime = Time.time;
        }
    }
}

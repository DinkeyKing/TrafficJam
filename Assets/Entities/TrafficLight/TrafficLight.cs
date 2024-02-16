using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{

    public float passTime = 5.0f;
    public float stopTime = 5.0f;

    public bool is_in_pass_state = true;

    private SpriteRenderer spriteRenderer;
    private float remainingTime = 0.0f;



    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        remainingTime = passTime;
        UpdateColor();
    }


    private void ResetTimer()
    {
        if (is_in_pass_state)
        {
            remainingTime = passTime;
        }
        else
        {
            remainingTime = stopTime;
        }
    }


    private void UpdateColor()
    {
        if (is_in_pass_state)
        {
            spriteRenderer.color = Color.green;
        }
        else
        {
            spriteRenderer.color = Color.red;
        }
    }


    void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime < 0.0f)
        {
            is_in_pass_state = !is_in_pass_state;

            UpdateColor();
            ResetTimer();
        }
    }
}

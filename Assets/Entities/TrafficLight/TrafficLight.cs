using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{

    public float passTime = 5.0f;
    public float stopTime = 5.0f;
    public State state;
    public Vector2 boxSize = new(1.0f, 1.0f);

    private SpriteRenderer spriteRenderer;
    private float remainingTime = 0.0f;

    new private Collider2D collider;

    private List<Car> carsToWakeUp = new List<Car>();

    public enum State
    {
        PASS,
        STOP
    }


    // The car will be stopped only when entering the collider, if the lamp turns red and the car is already in the trigger area, it won't be stopped.
    private void OnTriggerEnter2D(Collider2D other_collider)
    {
        if (state == State.STOP)
        {
            Car car = other_collider.GetComponent<Car>();
            if (car != null)
            {
                car.state = Car.AIState.STOP;

                carsToWakeUp.Add(car);
            }
        }
    }


    // Notify stopped cars that were stopped, so they can move again
    private void WakeAllCars()
    {
        foreach (Car car in carsToWakeUp)
        {
            car.state = Car.AIState.DRIVE;
        }
        carsToWakeUp.Clear();
    }

    // Sets the new state and applies changes
    private void SetState(State new_state)
    {
        state = new_state;

        switch (state)
        {
            case State.PASS:
                remainingTime = passTime;
                spriteRenderer.color = Color.green;

                // The cars that were stopped need to be informed here that they can pass
                WakeAllCars();

                break;

            case State.STOP:
                remainingTime = stopTime;
                spriteRenderer.color = Color.red;
                break;
        }
    }


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();

        SetState(State.PASS);
    }


    void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0.0f)
        {
            switch (state)
            {
                case State.PASS:
                    SetState(State.STOP);
                    break;

                case State.STOP:
                    SetState(State.PASS);
                    break;
            }
        }
    }
}

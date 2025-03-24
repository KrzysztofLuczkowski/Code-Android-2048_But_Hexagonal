using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    private Vector2 startTouchPosition, endTouchPosition;
    public TileMover tileMover; // Referencja do skryptu TileMover
    public GameUI gameUI;
    public MoveValidator moveValidator;
    void Update()
    {
#if UNITY_EDITOR
        // Obs³uguje myszkê w edytorze
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            DetectSwipe();
        }
#endif

        // Obs³uguje dotyk na urz¹dzeniu mobilnym
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                DetectSwipe();
            }
        }
    }

    private void DetectSwipe()
    {
        if (tileMover.isMoving) return;
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        if (swipeDelta.magnitude < 50)
        {
            return; // Minimalna d³ugoœæ przesuniêcia
        }

        // Obliczanie k¹ta w stopniach
        float angle = Mathf.Atan2(swipeDelta.y, swipeDelta.x) * Mathf.Rad2Deg;

        // Upewnijmy siê, ¿e k¹t mieœci siê w zakresie 0-360
        if (angle < 0)
        {
            angle += 360;
        }
        ///Debug.Log(angle);
        Vector2Int direction = Vector2Int.zero;

        // Na podstawie k¹ta przypisujemy odpowiedni kierunek
        if (angle >= 0 && angle <= 60)
        {
            direction = new Vector2Int(1, 0); // E
        }
        else if (angle > 60 && angle <= 120)
        {
            direction = new Vector2Int(0, 1); // W
        }
        else if (angle > 120 && angle <= 180)
        {
            direction = new Vector2Int(-1, 1); // Q
        }
        else if (angle > 180 && angle <= 240)
        {
            direction = new Vector2Int(-1, 0); // A
        }
        else if (angle > 240 && angle <= 300)
        {
            direction = new Vector2Int(0, -1); // S
        }
        else if (angle > 300 && angle <= 360)
        {
            direction = new Vector2Int(1, -1); // D
        }

        if (direction != Vector2Int.zero && moveValidator.IsMovePossible(direction))
        {
            StartCoroutine(tileMover.MoveTiles(direction)); // Przesuniêcie kafelków
            gameUI.IncrementMoves();
            gameUI.PlayMoveSound();
            gameUI.isWaitingForFirstMove = false;
        }
    }
}

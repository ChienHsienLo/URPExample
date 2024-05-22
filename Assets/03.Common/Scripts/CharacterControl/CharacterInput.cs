using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public UIInput joystick;
    public UIInput lookStick;
    public bool useUILook = false;

    private void Start()
    {
        UpdateCursorLockState(cursorLocked);
    }

   

    private void Update()
    {
        UpdateLookLock();
        UpdateMove();
        UpdateLook();
        UpdateJump();
        UpdateSprint();

    }

    Vector2 joystickInput = Vector2.zero;
    Vector2 kbInput = Vector2.zero;

    void UpdateMove()
    {
        if(joystick!=null)
        {
            joystickInput = joystick.GetValue();
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        kbInput = new Vector2(horizontal, vertical).normalized;

        if(Vector2.SqrMagnitude(kbInput) > Vector2.SqrMagnitude(joystickInput))
        {
            move = kbInput;
        }
        else
        {
            move = joystickInput;
        }

    }

    public void UpdateMove(Vector2 value)
    {
        joystickInput = value;
    }

    Vector2 lastFrameUILook = Vector2.zero;

    public void UpdateLook()
    {
        if(!cursorInputForLook)
        {
            return;
        }

        if (useUILook)
        {
            if (lookStick)
            {
                Vector2 i = lookStick.GetDelta();
                i.y *= -1;
                look = i;// lookStick.GetDelta();
            }
        }
        else
        {
            float horizontal = Input.GetAxis("Mouse X");
            float vertical = Input.GetAxis("Mouse Y");
            look = new Vector2(horizontal, -vertical);
        }
    }

    void UpdateJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jump = false;
        }

    }


    void UpdateSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprint = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprint = false;
        }

    }

    void UpdateLookLock()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            cursorInputForLook = !cursorInputForLook;
            cursorLocked = cursorInputForLook;

            if(!cursorInputForLook)
            {
                look = Vector2.zero;
            }

            UpdateCursorLockState(cursorLocked);
        }
    }

    void UpdateCursorLockState(bool doLock)
    {
        Cursor.lockState = doLock ? CursorLockMode.Locked : CursorLockMode.None;
    }

}

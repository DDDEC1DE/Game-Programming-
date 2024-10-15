using System;
using System.Collections.Generic;
using UnityEngine;


public class MouseKeyPlayerController : PlayerController
{
    public ThirdPersonCamera ControlCamera { get; private set; }
    public void Init(Player player)
    {
        ControlCamera = Camera.main.GetComponent<ThirdPersonCamera>();
        ControlCamera.SetPlayer(player);
    }

    public MouseKeyPlayerController()
    {

    }

    public void UpdateControls()
    {
        if (ControlCamera == null)
        {
            return;
        }
        UpdateMouseControlToggle();
        Vector3 lookInput = GetLookInput();
        ControlCamera.UpdateRotation(lookInput.y, lookInput.x);
    }

    public void SetFacingDirection(Vector3 direction)
    {
        ControlCamera.SetFacingDirection(direction);
    }

    public void AddLedgeDir(Vector3 ledgeDir)
    {

    }

    public Vector3 GetControlRotation()
    {
        return ControlCamera.ControlRotation;
    }

    public Vector3 GetMoveInput()
    {
        return new Vector3(
            Input.GetAxis("Horizontal"),
            0.0f,
            Input.GetAxis("Vertical")
            );
    }

    public Vector3 GetLookInput()
    {
        //Don't allow looking around if mouse isn't enabled
        if (!m_EnableMouseControl)
        {
            return Vector3.zero;
        }

        return new Vector3(
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"),
            0.0f
            );
    }

    public Vector3 GetAimTarget()
    {
        return Vector3.zero;
    }

    public bool IsJumping()
    {
        return Input.GetButton("Jump");
    }

    public bool IsFiring()
    {
        if (!m_EnableMouseControl)
        {
            return false;
        }

        return Input.GetButton("Fire1");
    }

    public bool IsAiming()
    {
        return Input.GetButton("Aim");
    }

    public bool ToggleCrouch()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.C);
    }

    public bool SwitchToItem1()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha1);
    }

    public bool SwitchToItem2()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha2);
    }

    public bool SwitchToItem3()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha3);
    }

    public bool SwitchToItem4()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha4);
    }

    public bool SwitchToItem5()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha5);
    }

    public bool SwitchToItem6()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha6);
    }

    public bool SwitchToItem7()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha7);
    }

    public bool SwitchToItem8()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha8);
    }

    public bool SwitchToItem9()
    {
        //TODO: Getting input from the keyboard directly is convenient for getting features in quickly for prototyping, etc.
        //      but it isn't usually ideal for final products.  This should be changed to use Unity's regular input system.
        return Input.GetKeyDown(KeyCode.Alpha9);
    }

    void UpdateMouseControlToggle()
    {
        //Check for a mouse click to lock and enable mouse control
        //GUIUtility.hotControl will be non-zero if a UI element was clicked.  If this is the case ignore the input.
        if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Set enable mouse control here.  This can change outside of our control so we'll check it every frame.
        m_EnableMouseControl = Cursor.lockState == CursorLockMode.Locked;

        Cursor.visible = !m_EnableMouseControl;
    }
    public void ShowMouseCursor()
    {
        if(Input.GetKey(KeyCode.LeftControl))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else 
        {  
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    bool m_EnableMouseControl;
}

using System;
using UnityEngine;

public static class ExCursor
{
    public static bool cursorLocked
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set 
        {
            if(value)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
    }
    
}

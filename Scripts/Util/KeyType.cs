using Godot;
using System.Collections.Generic;
public class KeyTypes
{
    public static Dictionary<string, KeyType> List = new Dictionary<string, KeyType> {
        {
            "mouseone"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEBUTTON
                , ButtonValue = ButtonList.Left
            }
        },
        {
            "mousetwo"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEBUTTON
                , ButtonValue = ButtonList.Right
            }
        },
        {
            "mousethree"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEBUTTON
                , ButtonValue = ButtonList.Middle
            }
        },
        {
            "wheelup"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEBUTTON
                , ButtonValue = ButtonList.WheelUp
            }
        },
        {
            "wheeldown"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEBUTTON
                , ButtonValue = ButtonList.WheelDown
            }
        },
        {
            "mouseup"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEAXIS
                , Direction = ButtonInfo.DIRECTION.UP
            }
        },
        {
            "mousedown"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEAXIS
                , Direction = ButtonInfo.DIRECTION.DOWN
            }
        },
        {
            "mouseright"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEAXIS
                , Direction = ButtonInfo.DIRECTION.RIGHT
            }
        },
        {
            "mouseleft"
            , new KeyType {
                Type = ButtonInfo.TYPE.MOUSEAXIS
                , Direction = ButtonInfo.DIRECTION.LEFT
            }
        },
        {
            "leftstickup"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.UP
                , ControllerButtonValue = JoystickList.AnalogLy
            }
        },
        {
            "leftstickdown"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.DOWN
                , ControllerButtonValue = JoystickList.AnalogLy
            }
        },
        {
            "leftstickright"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.RIGHT
                , ControllerButtonValue = JoystickList.AnalogLx
            }
        },
        {
            "leftstickleft"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.LEFT
                , ControllerButtonValue = JoystickList.AnalogLx
            }
        },
        {
            "rightstickup"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.UP
                , ControllerButtonValue = JoystickList.AnalogRy
            }
        },
        {
            "rightstickdown"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.DOWN
                , ControllerButtonValue = JoystickList.AnalogRy
            }
        },
        {
            "rightstickright"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.RIGHT
                , ControllerButtonValue = JoystickList.AnalogRx
            }
        },
        {
            "rightstickleft"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERAXIS
                , Direction = ButtonInfo.DIRECTION.LEFT
                , ControllerButtonValue = JoystickList.AnalogRx
            }
        },
        {
            "xboxa"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.XboxA
            }
        },
        {
            "xboxb"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.XboxB
            }
        },
        {
            "xboxx"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.XboxX
            }
        },
        {
            "xboxy"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.XboxY
            }
        },
        {
            "xboxlb"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.L
            }
        },
        {
            "xboxrb"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.R
            }
        },
        {
            "xboxlt"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.L2
            }
        },
        {
            "xboxrt"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.R2
            }
        },
        {
            "rightstickclick"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.R3
            }
        },
        {
            "leftstickclick"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.L3
            }
        },
        {
            "dpadup"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.DpadUp
            }
        },
        {
            "dpaddown"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.DpadDown
            }
        },
        {
            "dpadleft"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.DpadLeft
            }
        },
        {
            "dpadright"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.DpadRight
            }
        },
        {
            "xboxstart"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.Start
            }
        },
        {
            "xboxselect"
            , new KeyType {
                Type = ButtonInfo.TYPE.CONTROLLERBUTTON
                , ControllerButtonValue = JoystickList.Select
            }
        },
    };
}

public class KeyType
{
    public ButtonInfo.TYPE Type;
    public ButtonList ButtonValue = ButtonList.Left;
    public ButtonInfo.DIRECTION Direction = ButtonInfo.DIRECTION.UP;
    public JoystickList ControllerButtonValue = JoystickList.Axis0;
}
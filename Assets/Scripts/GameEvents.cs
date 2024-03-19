using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DOTSCC.GameOfLife
{
    public static class GameEvents
    {
        public enum Buttons_Types
        {
            GenerateNext,
            Repeat
        }
        public struct UIEvent
        {
            public Buttons_Types Type;
        }

        public static event Action<UIEvent> OnClicked;

        public static void RaiseEvent(UIEvent e)
        {
            OnClicked?.Invoke(e);
        }
    }
}
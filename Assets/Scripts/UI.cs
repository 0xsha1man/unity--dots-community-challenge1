using DOTSCC.GameOfLife;
using UnityEngine;
using UnityEngine.UIElements;
using static DOTSCC.GameOfLife.GameEvents;

public class UI : MonoBehaviour
{
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var generationButton = root.Q<Button>("Generation");
        var repeat = root.Q<Button>("Repeat");

        generationButton.clickable.clicked += Generation_OnClick;
        repeat.clickable.clicked += Repeat_OnClick;
    }

    private void Generation_OnClick()
    {
        GameEvents.RaiseEvent(new UIEvent { Type = Buttons_Types.GenerateNext });
    }

    private void Repeat_OnClick()
    {
        GameEvents.RaiseEvent(new UIEvent { Type = Buttons_Types.Repeat });
    }
}

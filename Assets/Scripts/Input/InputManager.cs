using UnityEngine;
public class InputManager : MonoBehaviour
{
    public static InputMaster INPUT { get; private set; }

    void Awake() => INPUT = new();

    void OnEnable() => INPUT.Enable();
    void OnDisable() => INPUT.Disable();
}


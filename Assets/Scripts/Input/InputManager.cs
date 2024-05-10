using UnityEngine;
public class InputManager : MonoBehaviour
{
    public InputMaster INPUT { get; private set; }

    public static InputManager instance;

    void Awake()
    {
        instance = this;
        INPUT = new();
    }

    void OnEnable() => INPUT.Enable();

    void OnDisable() => INPUT.Disable();
}


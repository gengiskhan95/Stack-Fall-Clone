using UnityEngine;

public class LongCylinderInitializer : MonoBehaviour
{
    private Transform playerTransform;
    private Transform longCylinder;
    private Transform winTransform;

    [SerializeField] private float yIncrement = 50f;
    [SerializeField] private float yOffset = 5f; // Added offset variable

    private void Awake()
    {
        InitializePlayerTransform();
        InitializeWinTransform();
        InitializeLongCylinder();
    }

    private void Start()
    {
        AdjustLongCylinderHeight();
    }

    private void InitializePlayerTransform()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
        else
        {
            Debug.LogError("Player Controller not found! Please ensure the Player object is present in the scene.");
        }
    }

    private void InitializeWinTransform()
    {
        var winObject = GameObject.Find("Win(Clone)");
        if (winObject != null)
        {
            winTransform = winObject.transform;
        }
        else
        {
            Debug.LogError("Win(Clone) object not found! Please ensure the Win(Clone) object is present in the scene.");
        }
    }

    private void InitializeLongCylinder()
    {
        if (winTransform != null)
        {
            longCylinder = winTransform.Find("LongCylinder");
            if (longCylinder == null)
            {
                Debug.LogError("LongCylinder object not found as a child of Win(Clone)! Please ensure the LongCylinder object is a child of Win(Clone) and named correctly.");
            }
        }
    }

    private void AdjustLongCylinderHeight()
    {
        if (longCylinder != null && playerTransform != null)
        {
            Vector3 scale = longCylinder.localScale;
            scale.y = Mathf.Abs(playerTransform.position.y - winTransform.position.y) / 2;
            longCylinder.localScale = scale;

            Vector3 position = longCylinder.position;
            position.y = (playerTransform.position.y + winTransform.position.y) / 2 + yOffset; // Apply offset
            longCylinder.position = position;
        }
    }
}
    
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Offset value for the camera's Y position relative to the player and win objects.")]
    [SerializeField] private float cameraYOffset = 4f;
    [SerializeField] private Vector3 initialCameraPosition = new Vector3(0, 0, -5);
    [SerializeField] private float cameraSmoothTime = 0.3f;

    [Header("Target Transforms")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform winTransform;

    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        SetupCamera();
    }

    private void Update()
    {
        UpdateCameraPosition();
    }

    #region Initialization Methods
    private void InitializeComponents()
    {
        InitializePlayerTransform();
        InitializeWinTransform();
    }

    private void InitializePlayerTransform()
    {
        if (playerTransform == null)
        {
            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
            else
            {
                Debug.LogError("Player Controller not found! Please assign the Player Transform.");
            }
        }
    }

    private void InitializeWinTransform()
    {
        if (winTransform == null)
        {
            var winObject = GameObject.Find("Win(Clone)");
            if (winObject != null)
            {
                winTransform = winObject.transform;
            }
            else
            {
                Debug.LogWarning("Win object not yet instantiated. The camera will follow only the player.");
            }
        }
    }
    #endregion

    #region Camera Methods
    private void SetupCamera()
    {
        SetInitialCameraPosition();
    }

    private void SetInitialCameraPosition()
    {
        transform.position = initialCameraPosition;
    }

    private void UpdateCameraPosition()
    {
        if (playerTransform == null) return;
        EnsureWinTransformInitialized();
        FollowPlayerAndWin();
    }

    private void EnsureWinTransformInitialized()
    {
        if (winTransform == null)
        {
            var winObject = GameObject.Find("Win(Clone)");
            if (winObject != null)
            {
                winTransform = winObject.transform;
            }
            else
            {
                Debug.LogWarning("Win object not yet instantiated. The camera will follow only the player.");
            }
        }
    }

    private void FollowPlayerAndWin()
    {
        if (transform.position.y > playerTransform.position.y && transform.position.y > winTransform.position.y + cameraYOffset)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, playerTransform.position.y, initialCameraPosition.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, cameraSmoothTime);
        }
    }
    #endregion
}

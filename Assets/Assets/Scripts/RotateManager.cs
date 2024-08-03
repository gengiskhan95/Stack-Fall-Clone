using UnityEngine;

public class RotateManager : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The speed at which the object rotates.")]
    [Range(0, 500)]
    [SerializeField] private float rotationSpeed = 100f;

    public float RotationSpeed
    {
        get => rotationSpeed;
        set => rotationSpeed = value;
    }

    private void Update()
    {
        HandleRotation();
    }

    #region Private Methods

    private void HandleRotation()
    {
        PerformRotation();
    }

    private void PerformRotation()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    [ContextMenu("Reset Rotation Speed")]
    private void ResetRotationSpeed()
    {
        rotationSpeed = 100f;
    }

    #endregion
}

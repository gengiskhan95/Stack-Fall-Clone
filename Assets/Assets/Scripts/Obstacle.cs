using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Rigidbody obstacleRigidbody;
    [SerializeField] private MeshRenderer obstacleMeshRenderer;
    [SerializeField] private Collider obstacleCollider;
    [SerializeField] private ObstacleManager obstacleManager;

    [Header("Shatter Force Settings")]
    [Tooltip("Range of force applied when the obstacle shatters.")]
    [SerializeField] private Vector2 shatterForceRange = new Vector2(20f, 35f);

    [Tooltip("Range of torque applied when the obstacle shatters.")]
    [SerializeField] private Vector2 shatterTorqueRange = new Vector2(110f, 180f);

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
    }

    #region Initialization Methods

    private void InitializeComponents()
    {
        obstacleRigidbody = GetComponent<Rigidbody>();
        obstacleMeshRenderer = GetComponent<MeshRenderer>();
        obstacleCollider = GetComponent<Collider>();
        obstacleManager = GetComponentInParent<ObstacleManager>();
    }

    private void ValidateComponents()
    {
        if (obstacleRigidbody == null) Debug.LogError("Rigidbody component is missing.", this);
        if (obstacleMeshRenderer == null) Debug.LogError("MeshRenderer component is missing.", this);
        if (obstacleCollider == null) Debug.LogError("Collider component is missing.", this);
        if (obstacleManager == null) Debug.LogError("ObstacleManager component is missing in parent.", this);
    }

    #endregion

    #region Shatter Methods

    public void Shatter()
    {
        if (AreComponentsMissing()) return;

        DisableObstacleCollider();
        ApplyShatterForces();
    }

    private bool AreComponentsMissing()
    {
        if (obstacleRigidbody == null || obstacleCollider == null || obstacleMeshRenderer == null)
        {
            Debug.LogWarning("Cannot shatter obstacle due to missing components.", this);
            return true;
        }
        return false;
    }

    private void DisableObstacleCollider()
    {
        obstacleRigidbody.isKinematic = false;
        obstacleCollider.enabled = false;
    }

    private void ApplyShatterForces()
    {
        Vector3 forcePoint = transform.parent.position;
        float parentXpos = transform.parent.position.x;
        float xPos = obstacleMeshRenderer.bounds.center.x;

        Vector3 direction = CalculateShatterDirection(parentXpos, xPos);
        float force = Random.Range(shatterForceRange.x, shatterForceRange.y);
        float torque = Random.Range(shatterTorqueRange.x, shatterTorqueRange.y);

        obstacleRigidbody.AddForceAtPosition(direction * force, forcePoint, ForceMode.Impulse);
        obstacleRigidbody.AddTorque(Vector3.left * torque);
        obstacleRigidbody.velocity = Vector3.down;
    }

    private Vector3 CalculateShatterDirection(float parentXpos, float xPos)
    {
        Vector3 subdir = (parentXpos - xPos < 0) ? Vector3.right : Vector3.left;
        return (Vector3.up * 1.5f + subdir).normalized;
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraBehaviour
{
    public float ObstacleCheckRadius = 0.5f;
    public Vector3 PlayerLocalObstructionMovePos = Vector3.zero;
    public CameraBehaviour()
    {

    }

    public virtual void Init(ThirdPersonCamera camera, Player player)
    {
        m_Camera = camera;
        m_Player = player;

        m_RaycastHitMask = ~LayerMask.GetMask("Player", "Ignore Raycast");
    }

    public virtual void Activate()
    {

    }

    public virtual void Deactivate()
    {

    }

    public abstract void UpdateCamera();
    public abstract void UpdateRotation(float yawAmount, float pitchAmount);
    public abstract void SetFacingDirection(Vector3 direction);

    public virtual Vector3 GetControlRotation()
    {
        return m_Camera.transform.rotation.eulerAngles;
    }

    public virtual bool UsesStandardControlRotation()
    {
        return true;
    }

    protected float HandleObstacles()
    {
        Vector3 rayStart = m_Player.transform.TransformPoint(PlayerLocalObstructionMovePos);
        Vector3 rayEnd = m_Camera.transform.position;

        Vector3 rayDir = rayEnd - rayStart;

        float rayDist = rayDir.magnitude;

        if(rayDist <= 0.0f)
        {
            return 0.0f;
        }

        rayDir /= rayDist;

        RaycastHit[] hitInfos = Physics.SphereCastAll(rayStart, ObstacleCheckRadius, rayDir, rayDist, m_RaycastHitMask);
        if(hitInfos.Length <= 0)
        {
            return rayDist;
        }

        float minMoveUpDist = float.MaxValue;
        foreach (RaycastHit hitInfo in hitInfos)
        {
            minMoveUpDist = Mathf.Min(minMoveUpDist, hitInfo.distance);
        }

        if(minMoveUpDist < float.MaxValue)
        {
            m_Camera.transform.position = rayStart + rayDir * minMoveUpDist;
        }

        //Debug.DrawLine(rayStart, rayEnd, Color.red);

        return minMoveUpDist;
    }

    protected ThirdPersonCamera m_Camera;

    protected Player m_Player;

    int m_RaycastHitMask;
}

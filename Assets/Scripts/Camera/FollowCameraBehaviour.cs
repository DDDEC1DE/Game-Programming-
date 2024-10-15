using UnityEngine;

public class FollowCameraBehaviour : CameraBehaviour
{
    public float CameraHorizPosEaseSpeed = 5.0f;
    public float CameraVertPosEaseSpeed = 4.0f;
    public float LookPosEaseSpeed = 5.0f;

    public Vector3 PlayerMaxDistLocalLookPos = Vector3.zero;
    public Vector3 PlayerMinDistLocalLookPos = Vector3.zero;

    public Vector3 PlayerLocalPivotPos = Vector3.zero;

    public float YawRotateSpeed = 1.0f;
    public float PitchRotateSpeed = 1.0f;
    public float MaxVerticalAngle = 70.0f;

    public float MaxDistFromPlayer = 6.0f;
    public float MinHorizDistFromPlayer = 5.0f;
    public float AutoRotateDelayTime = 1.0f;

    public FollowCameraBehaviour()
    {

    }

    public override void Activate()
    {
        base.Activate();

        m_GoalPos = m_Camera.transform.position;
        m_AllowAutoRotate = false;
        m_TimeTillAutoRotate = AutoRotateDelayTime;
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override void UpdateRotation(float yawAmount, float pitchAmount)
    {
        m_YawInput = yawAmount;
        m_PitchInput = pitchAmount;
    }

    public override void SetFacingDirection(Vector3 direction)
    {
      
    }

    public override Vector3 GetControlRotation()
    {
        return base.GetControlRotation();
    }

    public override bool UsesStandardControlRotation()
    {
        return false;
    }

    public override void UpdateCamera()
    {
        m_Player.Controller.ShowMouseCursor();

        Vector3 worldPivotPos = m_Player.transform.TransformPoint(PlayerLocalPivotPos);

        Vector3 offsetFromPlayer = m_GoalPos - worldPivotPos;

        float distFromPlayer = offsetFromPlayer.magnitude;

        //Update Position of the Camera
        Vector3 rotateAmount = new Vector3(m_PitchInput * PitchRotateSpeed, m_YawInput * YawRotateSpeed);

        m_TimeTillAutoRotate -= Time.deltaTime;

        if(!MathUtils.AlmostEquals(rotateAmount.y, 0.0f))
        {
            m_AllowAutoRotate = false;
            m_TimeTillAutoRotate = AutoRotateDelayTime;
        }
        else if( m_TimeTillAutoRotate <= 0.0f)
        {
            m_AllowAutoRotate = true;
        }

        //Horizontal Rotation
        Vector3 pivotRotation = m_Camera.PivotRotation;

        if(m_AllowAutoRotate)
        {
            Vector3 anglesFromPlayer = Quaternion.LookRotation(offsetFromPlayer).eulerAngles;
            pivotRotation.y = anglesFromPlayer.y;
        }
        else
        {
            pivotRotation.y += rotateAmount.y;

           //Debug.Log(rotateAmount.y);
        }


        //pivotRotation.y += m_Player.GroundAngularVelocity.y * Time.deltaTime;

        pivotRotation.x -= rotateAmount.x;

        pivotRotation.x = Mathf.Clamp(pivotRotation.x, -MaxVerticalAngle, MaxVerticalAngle);

        m_Camera.PivotRotation = pivotRotation;

        distFromPlayer = Mathf.Clamp(distFromPlayer, MinHorizDistFromPlayer, MaxDistFromPlayer);

        offsetFromPlayer = Quaternion.Euler(pivotRotation.x, pivotRotation.y, 0.0f) * Vector3.forward;

        offsetFromPlayer *= distFromPlayer;

        m_GoalPos = offsetFromPlayer + worldPivotPos;

        Vector3 newCameraPosition = m_Camera.transform.position;

        newCameraPosition = MathUtils.SlerpToHoriz(
            CameraHorizPosEaseSpeed,
            newCameraPosition,
            m_GoalPos,
            worldPivotPos,
            Time.deltaTime);

        newCameraPosition.y = MathUtils.LerpTo(
            CameraVertPosEaseSpeed,
            newCameraPosition.y,
            m_GoalPos.y,
            Time.deltaTime);

        m_Camera.transform.position = newCameraPosition;

        HandleObstacles();

        Vector3 goalLookPos = m_Player.transform.TransformPoint(Vector3.zero);

        m_Camera.LookPos = MathUtils.LerpTo(
          LookPosEaseSpeed,
          m_Camera.LookPos,
          goalLookPos,
          Time.deltaTime);

        Vector3 lookDir = m_Camera.LookPos - m_Camera.transform.position;
        m_Camera.transform.rotation = Quaternion.LookRotation(lookDir);

    }


    Vector3 m_GoalPos;

    float m_YawInput;
    float m_PitchInput;

    float m_TimeTillAutoRotate;
    bool m_AllowAutoRotate;
}

using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.Infix;
using Leap.Unity.Swizzle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dash {

  using Pose = Leap.Unity.Pose;
  
  public class CameraController : MonoBehaviour {

    public Transform playerTransform;
    public Vector3 targetOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Input")]
    public float lookSpeed = 1f;

    [Header("Rig")]
    
    public float baseLookAhead = 5f;

    public enum CameraMode { Free }
    public CameraMode camMode = CameraMode.Free;

    [Header("Debug")]
    public bool drawDebug = false;

    [SerializeField]
    private float _yAngle = 0f;
    [SerializeField]
    private float _xAngle = 0f;
    [SerializeField]
    private Vector3 _camPos_pivot = new Vector3(0f, 0f, -5f);

    private void OnEnable() {
      if (playerTransform == null) {
        Debug.LogError("Camera requires a target.");
        this.enabled = false;
        return;
      }

      var dirToPlayer = playerTransform.position - this.transform.position;
      var yDir = Vector3.up;
      var groundDirToPlayer = dirToPlayer.ProjectedOnPlane(yDir);
      var xDir = Vector3.Cross(dirToPlayer, yDir).normalized;
      if (xDir == Vector3.zero) xDir = Vector3.right;
      _yAngle = Vector3.SignedAngle(Vector3.forward, dirToPlayer.normalized, Vector3.up);
      _xAngle = Vector3.SignedAngle(groundDirToPlayer, dirToPlayer, xDir);
      _xAngle = _xAngle.Clamped(-89f, 89f);
    }

    private void Update() {
      if (playerTransform == null) return;

      var lookTargetPose = playerTransform.ToPose() * targetOffset;
      var camPose = this.transform.ToPose();

      // mouse movement changes _pivotRot
      var mouseMove = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
      _yAngle += mouseMove.x * lookSpeed;
      if (_yAngle > 360f) _yAngle -= 360f;
      if (_yAngle < -360f) _yAngle += 360f;
      _xAngle += mouseMove.y * lookSpeed * -1;
      _xAngle = _xAngle.Clamped(-89f, 89f);
      var pivotRot = Quaternion.AngleAxis(_yAngle, Vector3.up);
      pivotRot = Quaternion.AngleAxis(_xAngle, pivotRot * Vector3.right) * pivotRot;

      var camPivotPose = lookTargetPose.WithRotation(pivotRot);
      //if (_camPos_pivot == null) {
      //  _camPos_pivot = (camPivotPose.inverse * camPose).position;
      //}
      var targetCamPos = (camPivotPose * _camPos_pivot).position;
      var targetCamRot = Quaternion.LookRotation(lookTargetPose.position - targetCamPos);

      this.transform.SetPose(new Pose(targetCamPos, targetCamRot));

      //this.transform.SetPose(targetPose);

      if (!Application.isPlaying) {

      }

      switch (camMode) {
        case CameraMode.Free:

          break;
      }
    }

  }

  public static class CameraControllerExtensions {

    public static Vector3 RotatedAround(this Vector3 v, Vector3 point, Vector3 axis, float angle) {
      var vFromPoint = v - point;
      var rotated = Quaternion.AngleAxis(angle, axis) * vFromPoint;
      return point + vFromPoint;
    }

    public static Pose RotatedAround(this Pose pose, Vector3 point, Vector3 axis, float angle) {
      var relativePosition = pose.position - point;
      var rotation = Quaternion.AngleAxis(angle, axis);
      var rotatedRelativePosition = rotation * relativePosition;
      return new Pose(point + rotatedRelativePosition, rotation * pose.rotation);
    }

    public static Vector3 ProjectedOnPlane(this Vector3 v, Vector3 planeNormal) {
      return Vector3.ProjectOnPlane(v, planeNormal);
    }

  }

}

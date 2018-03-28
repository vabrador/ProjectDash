using Leap.Unity;
using Leap.Unity.Infix;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Swizzle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dash {

  public class PlayerAController : MonoBehaviour {

    [Header("Camera (optional)")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float speed = 120f;
    public float movementDecay = 14f;
    public float turnSpeed = 12f;

    [Header("Animation")]
    public RigPoseController poser;
    public int poseIdx_stand = 0;
    public int poseIdx_dash = 1;

    [Header("Debug")]
    public bool drawDebug = false;

    private Vector3? _lastPosMem = null;
    private Vector2? _lastFacingIntentMem = null;

    private void Update() {

      // Current facing.
      var curFacing = this.transform.forward.xz();

      // Movement input.
      float moveVecMag;
      var moveVec = getInputMovementVector(out moveVecMag);
      var moveVecDir = (moveVecMag > 0.01f ? moveVec / moveVecMag : curFacing);
      var isMovingIntended = moveVecMag > 0.01f;
      if (cameraTransform != null && isMovingIntended) {
        var camXZToPlayer
          = this.transform.position.ProjectedOnPlane(this.transform.up)
            - cameraTransform.position.ProjectedOnPlane(this.transform.up);
        var camXZAngle = Vector3.SignedAngle(camXZToPlayer, Vector3.forward, Vector3.up);
        var camRotation = Quaternion.AngleAxis(-camXZAngle, Vector3.up);
        moveVec = (camRotation * moveVec.AsXZ()).xz();
        moveVecDir = (camRotation * moveVecDir.AsXZ()).xz();
      }
      if (drawDebug) {
        DebugPing.Line("moveVec", this.transform.position,
          this.transform.position + moveVec.AsXZ(), LeapColor.red);
      }

      // Movement.
      if (!_lastPosMem.HasValue) { _lastPosMem = this.transform.position; }
      var curVelXZ = ((this.transform.position - _lastPosMem.Value) / Time.deltaTime).xz();
      if (isMovingIntended) {
        curVelXZ = curVelXZ.RotatedTowards(moveVecDir, turnSpeed * Time.deltaTime);
        curVelXZ += speed * moveVec * Time.deltaTime;
      }
      curVelXZ *= 1 - (movementDecay * Time.deltaTime).Clamped01();
      _lastPosMem = this.transform.position;
      this.transform.position += curVelXZ.AsXZ() * Time.deltaTime;
      var curSpdXZ = curVelXZ.magnitude;

      // Facing.
      var facingIntent = _lastFacingIntentMem.ValueOr(moveVecDir);
      if (isMovingIntended) {
        var facingErrAngle
          = Vector3.SignedAngle(curFacing.AsXZ(), moveVecDir.AsXZ(), Vector3.up);
        if (facingErrAngle.Abs() > 179.1f) {
          curFacing = (Quaternion.AngleAxis(1f, Vector3.up) * curFacing.AsXZ()).xz();
        }

        facingIntent = moveVecDir;
        _lastFacingIntentMem = moveVecDir;
      }
      curFacing = curFacing.SlerpedTo(facingIntent, turnSpeed * Time.deltaTime);
      this.transform.SetForward(curFacing.AsXZ());

      // Animation.
      if (curSpdXZ > 0.5f) {
        poser.curPoseIdx = poseIdx_dash;
      }
      else {
        poser.curPoseIdx = poseIdx_stand;
      }

    }

    #region Input

    private Vector2 getInputMovementVector(out float moveVecMag) {
      var h = Input.GetAxis("Horizontal");
      var v = Input.GetAxis("Vertical");
      var moveInput = new Vector2(h, v);

      moveVecMag = moveInput.magnitude;
      if (moveVecMag > 1f) {
        moveInput /= moveVecMag;
        moveVecMag = 1f;
      }

      return moveInput;
    }

    #endregion

  }

  public static class PlayerAControllerExtensions {

    public static void SetForward(this Transform t, Vector3 newForward) {
      t.rotation = Quaternion.LookRotation(newForward, t.up);
    }

    /// <summary>
    /// Returns a Vector3 interpreting this Vector2 as its X and Z components, with the
    /// Y component set to zero.
    /// </summary>
    public static Vector3 AsXZ(this Vector2 v) {
      return new Vector3(v.x, 0f, v.y);
    }

    public static Vector2 SlerpedTo(this Vector2 v, Vector2 other, float t) {
      return Vector3.Slerp(v, other, t).xy();
    }

    public static Vector2 RotatedTowards(this Vector2 v, Vector2 targetDir, float maxAngleDelta) {
      return Vector3.RotateTowards(v, targetDir, maxAngleDelta * Mathf.Deg2Rad, 0f).xy();
    }

    public static float Abs(this float f) {
      return Mathf.Abs(f);
    }

    public static T ValueOr<T>(this T? nullable, T otherwiseValue) where T : struct {
      if (nullable.HasValue) {
        return nullable.Value;
      }
      return otherwiseValue;
    }

  }

}
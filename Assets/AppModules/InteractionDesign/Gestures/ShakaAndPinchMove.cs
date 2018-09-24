using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Gestures;

using Pose = Leap.Unity.Pose;
using Leap.Unity;

public class ShakaAndPinchMove : MonoBehaviour {
  
  public SelfShakaGesture shakaGesture;
  public PinchGesture pinchGesture;

  public float rotationMult = 1f;

  private Pose? _lastPose = null;

  public Transform moveTransform = null;

  private void Update() {
    if (shakaGesture.isActive && pinchGesture.isActive) {
      if (_lastPose.HasValue) {
        moveTarget(_lastPose.Value, pinchGesture.pose);
      }
      _lastPose = pinchGesture.pose;
    }
    else {
      _lastPose = null;
    }
  }

  private void moveTarget(Pose oldPose, Pose newPose) {
    if (moveTransform == null) return;

    // Translate on all three axes.
    var translation = newPose.position - oldPose.position;

    // Rotate only around world up.
    var rotation = Quaternion.AngleAxis(
      maxAbs( // Take the max of two in case angle is degenerate for one.
        Vector3.SignedAngle(
          oldPose.rotation * Vector3.forward,
          newPose.rotation * Vector3.forward,
          Vector3.up
        ),
        Vector3.SignedAngle(
          oldPose.rotation * Vector3.up,
          newPose.rotation * Vector3.up,
          Vector3.up
        )
      ) * rotationMult,
      Vector3.up
    );

    var origPose = moveTransform.ToPose();
    var movedPose = origPose + new Pose(translation, rotation);
    moveTransform.position = movedPose.position;
    moveTransform.rotation = movedPose.rotation;
  }

  // Returns the value with larger magnitude, without modifying sign.
  private float maxAbs(float a, float b) {
    if (Mathf.Abs(a) >= Mathf.Abs(b)) {
      return a;
    }
    return b;
  }

}

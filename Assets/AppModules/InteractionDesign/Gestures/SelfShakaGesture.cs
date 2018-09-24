using System.Collections;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using Leap.Unity.Gestures;
using UnityEngine;

public class SelfShakaGesture : OneHandedGesture {

  public Transform selfTarget;
  [Range(0f, 90f)]
  public float maxFacingAngle = 90f;
  
  private float stopHysteresisMult = 1.1f;

  protected override void Reset() {
    base.Reset();

    if (selfTarget == null && provider != null) {
      selfTarget = provider.transform;
    }
  }

  protected override bool ShouldGestureActivate(Hand hand) {
    return isPalmFacingTarget(hand, selfTarget.position, maxFacingAngle) &&
      isShakaHand(hand);
  }

  protected override bool ShouldGestureDeactivate(Hand hand,
                            out DeactivationReason? deactivationReason) {
    deactivationReason = DeactivationReason.FinishedGesture; // never cancels.
    return !(isPalmFacingTarget(hand, selfTarget.position, maxFacingAngle
      * stopHysteresisMult) && isShakaHand(hand));
  }

  private bool isPalmFacingTarget(Hand hand, Vector3 target, float maxAngle) {
    var palmDir = hand.PalmarAxis();
    var dirToTarget = (target - hand.PalmPosition.ToVector3()).normalized;
    return Vector3.Angle(palmDir, dirToTarget) <= maxFacingAngle;
  }

  private bool isShakaHand(Hand hand) {
    var radialAxis = hand.RadialAxis();
    var distalAxis = hand.DistalAxis();

    var thumb = hand.GetThumb();
    var thumbDir = thumb.Direction.ToVector3();
    var isThumbOut = Vector3.Dot(radialAxis, thumbDir) >= 0.3f;

    var pinky = hand.GetPinky();
    var pinkyDir = pinky.Direction.ToVector3();
    pinkyDir = Vector3.ProjectOnPlane(pinkyDir, radialAxis).normalized;
    var isPinkyOut = Vector3.Dot(distalAxis, pinkyDir) >= 0.3f;

    var index = hand.GetIndex();
    var middle = hand.GetMiddle();
    var ring = hand.GetRing();
    var otherFingersClosed = isFingerClosed(hand, index) &&
      isFingerClosed(hand, middle) && isFingerClosed(hand, ring);

    return isThumbOut && isPinkyOut && otherFingersClosed;
  }

  private bool isFingerClosed(Hand hand, Finger finger) {
    var distalAxis = hand.DistalAxis();
    var fingerDir = finger.bones[3].Direction.ToVector3();
    return Vector3.Dot(-distalAxis, fingerDir) >= 0.5f;
  }

}

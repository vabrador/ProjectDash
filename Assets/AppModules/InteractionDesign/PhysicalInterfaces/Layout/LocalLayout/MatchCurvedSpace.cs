using Leap.Unity.Attributes;
using Leap.Unity.Space;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Leap.Unity.Layout {

  [ExecuteInEditMode]
  public class MatchCurvedSpace : MonoBehaviour {

    public LeapSpace leapSpace;

    private void Reset() {
      if (leapSpace == null) {
        leapSpace = FindObjectOfType<LeapSpace>();
      }
    }

    [Header("Manual Specification")]
    public Vector3 localRectangularPosition = Vector3.zero;
    public bool matchRotation = true;
    public Vector3 localRectangularRotation = Vector3.zero;

    [Header("ILocalPositionProvider (overrides manual local position)")]
    [ImplementsInterface(typeof(ILocalPositionProvider))]
    public MonoBehaviour localPositionProvider = null;

    [Header("Use Transform (overrides all above)")]
    public Transform useTransform = null;

    [Tooltip("Instead of using the local position of the transform directly, "
      + "look in its children and use the child whose name includes a matching "
      + "numeric token. A numeric token is defined as a non-negative integer in "
      + "parentheses: (0), (1), (24), (3000), etc.")]
    public bool matchBasedOnNumericTokenInChild = false;

    [SerializeField, Disable]
    #pragma warning disable 0414
    private Transform _matchedChild;
    #pragma warning restore 0414

    private void Start() {
      refreshPosition();
    }

    private void Update() {
      refreshPosition();
    }

    private static Regex tokenMatcher = new Regex(@"\((\d+)\)");
    private static float? maybeGetNumericToken(string str) {
      var results = tokenMatcher.Match(str);

      var firstGroup = results.Groups[0];
      if (!string.IsNullOrEmpty(firstGroup.Value)) {
        var actualFloatStr = firstGroup.Value.Substring(1,
            firstGroup.Value.Length - 2);
        float parseResult;
        if (float.TryParse(actualFloatStr, out parseResult)) {
          return parseResult;
        }
      }
      else {
        Debug.LogError("no numeric token found in string: " + str);
      }

      return null;
    }

    private void refreshPosition() {
      if (leapSpace != null) {

        if (leapSpace.transformer != null) {
          var localPos = Vector3.zero;
          localPos = localRectangularPosition;
          if (localPositionProvider != null) {
            localPos = (localPositionProvider as ILocalPositionProvider)
              .GetLocalPosition(this.transform);
          }
          if (useTransform != null) {
            localPos = leapSpace.transform.worldToLocalMatrix
              .MultiplyPoint3x4(useTransform.position);

            if (matchBasedOnNumericTokenInChild) {
              var ownToken = maybeGetNumericToken(this.name);
              if (ownToken.HasValue) {
                foreach (var child in useTransform.GetChildren()) {
                  var childToken = maybeGetNumericToken(child.name);
                  if (childToken.HasValue &&
                      childToken.Value == ownToken.Value) {
                    localPos = child.localPosition;
                    _matchedChild = child;
                    break;
                  }
                }
              }
            }
          }

          var localRectPos = leapSpace.transform.InverseTransformPoint(
            this.transform.parent.TransformPoint(localPos));

          this.transform.position =
            leapSpace.transform.TransformPoint(
              leapSpace.transformer.TransformPoint(
                localRectPos));

          if (matchRotation) {
            this.transform.rotation =
              leapSpace.transform.TransformRotation(
                leapSpace.transformer.TransformRotation(
                  localRectPos,
                  leapSpace.transform.InverseTransformRotation(
                    this.transform.parent.TransformRotation(
                      Quaternion.Euler(localRectangularRotation)))));
          }
        }
      }
    }

  }
  
}

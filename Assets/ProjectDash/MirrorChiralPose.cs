using Leap.Unity;
using Leap.Unity.Attributes;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dash {

  using Pose = Leap.Unity.Pose;

  [ExecuteInEditMode]
  public class MirrorChiralPose : MonoBehaviour {
    
    public Transform mirrorMatch = null;

    public bool alsoMatchLocalScale = true;

    private void OnValidate() {
      if (mirrorMatch == null) {
        tryFindMirrorMatchSibling();
      }
    }

    [QuickButton("Mirror Now", "MirrorXPose")]
    public bool mirrorX;

    [QuickButton("Mirror Now", "MirrorYPose")]
    public bool mirrorY;

    [QuickButton("Mirror Now", "MirrorZPose")]
    public bool mirrorZ;

    private void Update() {
      if (mirrorX) {
        MirrorXPose();
      }
      if (mirrorY) {
        MirrorYPose();
      }
      if (mirrorZ) {
        MirrorZPose();
      }
    }

    public void MirrorXPose() {
      if (mirrorMatch == null) return;

      this.transform.SetLocalPose(mirrorMatch.ToLocalPose().MirroredX());

      maybeMatchScale();
    }

    public void MirrorYPose() {
      if (mirrorMatch == null) return;

      this.transform.SetLocalPose(mirrorMatch.ToLocalPose().MirroredY());

      maybeMatchScale();
    }

    public void MirrorZPose() {
      if (mirrorMatch == null) return;

      this.transform.SetLocalPose(mirrorMatch.ToLocalPose().MirroredZ());

      maybeMatchScale();
    }

    private void maybeMatchScale() {
      if (mirrorMatch == null) return;

      if (alsoMatchLocalScale) {
        this.transform.localScale = mirrorMatch.localScale;
      }
    }

    private void tryFindMirrorMatchSibling() {
      var thisName = this.name;

      var leftExpr = new Regex("(\\bLeft\\b)");
      var rightExpr = new Regex("(\\bRight\\b)");

      string rightName = null;
      bool thisIsLeft = false;
      var leftMatch = leftExpr.Match(thisName);
      if (leftMatch.Success) {
        rightName = thisName.Replace("Left", "Right");
        thisIsLeft = true;
      }
      string leftName = null;
      bool thisIsRight = false;
      var rightMatch = rightExpr.Match(thisName);
      if (rightMatch.Success) {
        leftName = thisName.Replace("Right", "Left");
        thisIsRight = true;
      }

      foreach (var sibling in this.transform.GetSiblings()) {
        if (thisIsLeft && sibling.name.Equals(rightName)) {
          mirrorMatch = sibling;
        }
        if (thisIsRight && sibling.name.Equals(leftName)) {
          mirrorMatch = sibling;
        }
      }
    }

  }

  public struct SiblingEnumerator {
    private Transform _thisT;
    private Transform _parent;
    private int _thisChildIdx;
    private int _currIdx;
    private bool _includeSelf;

    public SiblingEnumerator(Transform t, bool includeSelf = false) {
      _thisT = t;
      _parent = t.parent;
      _currIdx = -1;
      if (_parent == null) {
        _thisChildIdx = -1;
      }
      else {
        _thisChildIdx = _thisT.GetSiblingIndex();
      }
      _includeSelf = includeSelf;
    }

    public SiblingEnumerator GetEnumerator() { return this; }
    public Transform Current { get { return _parent.GetChild(_currIdx); } }
    public bool MoveNext() {
      _currIdx += 1;
      if (!_includeSelf && _currIdx == _thisChildIdx) _currIdx += 1;
      return _currIdx < _parent.childCount;
    }
  }

  public static class MirrorChiralPoseExtensions {

    /// <summary>
    /// Returns an enumerator that enumerates through the siblings of this transform, in
    /// sibling-index order.
    /// Optionally also include this transform in the returned siblings by passing
    /// includeSelf.
    /// </summary>
    public static SiblingEnumerator GetSiblings(this Transform t,
                                                bool includeSelf = false) {
      return new SiblingEnumerator(t, includeSelf);
    }

    /// <summary>
    /// Returns a Pose that has its position and rotation mirrored on the Y axis.
    /// </summary>
    public static Pose MirroredY(this Pose pose) {
      var v = pose.position;
      var q = pose.rotation;
      return new Pose(new Vector3(v.x, -v.y, v.z),
                      new Quaternion(q.x, -q.y, q.z, -q.w).Flipped());
    }

    /// <summary>
    /// Returns a Pose that has its position and rotation mirrored on the Z axis.
    /// </summary>
    public static Pose MirroredZ(this Pose pose) {
      var v = pose.position;
      var q = pose.rotation;
      return new Pose(new Vector3(v.x, v.y, -v.z),
                      new Quaternion(q.x, q.y, -q.z, -q.w).Flipped());
    }

  }

}

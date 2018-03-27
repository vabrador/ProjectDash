using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dash {

  [ExecuteInEditMode]
  public class RigPoseController : MonoBehaviour {

    public RigPoseAsset[] poseAssets;
    
    [SerializeField]
    [OnEditorChange("curPoseIdx")]
    private int _curPoseIdx = 0;
    public int curPoseIdx {
      get { return _curPoseIdx; }
      set {
        if (poseAssets == null || poseAssets.Length == 0) _curPoseIdx = 0;
        else {
          _curPoseIdx = Mathf.Clamp(value, 0, poseAssets.Length - 1);
        }
      }
    }

    private void Update() {
      if (poseAssets != null && poseAssets.Length > 0) {
        poseAssets[curPoseIdx].rigPose.SetTransforms(this.transform);
      }
    }

  }

}

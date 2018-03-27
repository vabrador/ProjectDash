using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dash {

  using Pose = Leap.Unity.Pose;

  [System.Serializable]
  public struct Bone {
    public string transformPath;
    public Pose localPose;
    public Vector3 localScale;
  }

  [System.Serializable]
  public class RigPose {
    [SerializeField]
    private List<Bone> _backingBoneData;
    public List<Bone> boneData {
      get {
        if (_backingBoneData == null) {
          _backingBoneData = new List<Bone>();
        }
        return _backingBoneData;
      }
    }

    public void Clear() {
      boneData.Clear();
    }

    public void Fill(RigPose otherRigPose) {
      Clear();
      foreach (var bone in otherRigPose.boneData) {
        boneData.Add(bone);
      }
    }

    private int addOrGetBoneIdx(string transformPath) {
      for (int i = 0; i < boneData.Count; i++) {
        var bone = boneData[i];
        if (bone.transformPath != null
            && bone.transformPath.Equals(transformPath)) {
          return i;
        }
      }

      boneData.Add(new Bone() { transformPath = transformPath });
      return boneData.Count - 1;
    }

    public void AddOrSetBone(string transformPath,
                             Pose newLocalPose,
                             Vector3 newLocalScale) {
      int boneIdx = addOrGetBoneIdx(transformPath);
      var bone = boneData[boneIdx];
      bone.localPose = newLocalPose;
      bone.localScale = newLocalScale;
      boneData[boneIdx] = bone;
    }

    public string ToJson(bool prettyPrint =  false) {
      return JsonUtility.ToJson(this, prettyPrint);
    }

    /// <summary>
    /// Traverses the children of rootTransform, setting Transform data whose names match
    /// the bone transform paths to match this pose.
    /// </summary>
    public void SetTransforms(Transform rootTransform) {
      foreach (var bone in boneData) {
        var liveTransform = rootTransform.Traverse(bone.transformPath);

        if (liveTransform != null) {
          liveTransform.SetLocalPose(bone.localPose);
          liveTransform.localScale = bone.localScale;
        }
        else {
          Debug.LogWarning("Couldn't find bone for relative path: " + bone.transformPath
            + " from root: " + rootTransform.name);
        }
      }
    }

  }

}
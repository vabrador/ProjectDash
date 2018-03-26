using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

namespace Dash {
  
  [CreateAssetMenu(fileName = "New RigPose",
                   menuName = "Animation Rig Pose",
                   order = 400)]
  public class RigPoseAsset : ScriptableObject {
    public RigPose rigPose;
  }

}

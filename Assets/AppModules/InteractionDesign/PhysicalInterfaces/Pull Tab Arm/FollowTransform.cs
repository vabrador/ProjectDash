using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  [AddComponentMenu("")]
  public class FollowTransform : MonoBehaviour {

    public Transform target;

    public enum FollowMode { Update, FixedUpdate }
    public FollowMode mode;
    public bool followPosition = true, followRotation = true;

    private void Update() {
      if (mode != FollowMode.Update) return;
      if (target != null && target.gameObject.activeInHierarchy) {
        if(followPosition) this.transform.position = target.transform.position;
        if(followRotation) this.transform.rotation = target.transform.rotation;
      }
    }

    private void FixedUpdate() {
      if (mode != FollowMode.FixedUpdate) return;
      if (target != null && target.gameObject.activeInHierarchy) {
        if(followPosition) this.transform.position = target.transform.position;
        if(followRotation) this.transform.rotation = target.transform.rotation;
      }
    }

  }

}

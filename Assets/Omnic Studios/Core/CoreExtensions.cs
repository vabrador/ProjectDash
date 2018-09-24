using UnityEngine;

namespace Omnic {

  public static class CoreExtensions {

    /// <summary>
    /// Returns a copy of the input Vector3 with a different X component.
    /// </summary>
    public static Vector3 WithX(this Vector3 v, float x) {
      return new Vector3(x, v.y, v.z);
    }

    /// <summary>
    /// Returns a copy of the input Vector3 with a different Y component.
    /// </summary>
    public static Vector3 WithY(this Vector3 v, float y) {
      return new Vector3(v.x, y, v.z);
    }

    /// <summary>
    /// Returns a copy of the input Vector3 with a different Z component.
    /// </summary>
    public static Vector3 WithZ(this Vector3 v, float z) {
      return new Vector3(v.x, v.y, z);
    }

  }

}

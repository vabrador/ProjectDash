using UnityEngine;
using Leap.Unity;
using Leap;
using Leap.Unity.Attributes;
using System.Collections.Generic;

public abstract class LeapPostProcess : MonoBehaviour {
  
  public Image combinedImage;
  
  [Disable]
  public int width = 640;
  [Disable]
  public int height = 240;

  private HashSet<uint> _alreadyRunThisFrame = new HashSet<uint>();

  protected virtual void Update() {
    _alreadyRunThisFrame.Clear();
  }

  protected abstract void processImage(Image image, byte[] imageData);

  public void OnNewImage(Image image, byte[] imageData) {
    if (_alreadyRunThisFrame.Contains(image.DeviceID) || !enabled) return;
    _alreadyRunThisFrame.Add(image.DeviceID);

    width = image.Width;
    height = image.Height;
    combinedImage = image;

    processImage(image, imageData);
  }

  public void drawSphereCameraCoordinate(Vector2 pixelCoordinate, float size, Transform debugTransform = null) {
    Vector2 blob = Vector2.Scale(pixelCoordinate, new Vector2(1f / width, 1f / height)) - (Vector2.one * 0.5f);
    blob.y /= 2f;
    blob.y -= 0.25f;
    Gizmos.DrawSphere((debugTransform == null ? transform : debugTransform).TransformPoint(blob), size);
  }

  public void drawLineCameraCoordinate(Vector2 pixelCoordinateA, Vector2 pixelCoordinateB, Transform debugTransform = null) {
    Vector3 blobA = Vector2.Scale(pixelCoordinateA, new Vector2(1f / width, 1f / height)) - (Vector2.one * 0.5f);
    blobA.y /= 2f;
    blobA.y -= 0.25f;
    blobA.z = -0.01f;

    Vector3 blobB = Vector2.Scale(pixelCoordinateB, new Vector2(1f / width, 1f / height)) - (Vector2.one * 0.5f);
    blobB.y /= 2f;
    blobB.y -= 0.25f;
    blobB.z = -0.01f;

    Gizmos.DrawLine((debugTransform == null ? transform : debugTransform).TransformPoint(blobA),
                    (debugTransform == null ? transform : debugTransform).TransformPoint(blobB));
  }
}

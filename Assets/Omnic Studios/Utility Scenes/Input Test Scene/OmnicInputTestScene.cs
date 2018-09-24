using UnityEngine;

namespace Omnic.UtilityScenes {

  public class OmnicInputTestScene : MonoBehaviour {

    public Transform leftHorizontalVisual;

    private void Update() {
      if (leftHorizontalVisual != null) {
        var leftHorizontalPosition = leftHorizontalVisual.transform
          .localPosition;
        var horizontalAxis = Input.GetAxis("Horizontal");

        leftHorizontalPosition = leftHorizontalPosition.WithX(horizontalAxis);
      }
    }

  } 

}

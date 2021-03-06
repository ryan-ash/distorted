using UnityEngine;

public class Rotator : MonoBehaviour {
    public float rotationSpeed;
    
    void Update() {
        transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }
}

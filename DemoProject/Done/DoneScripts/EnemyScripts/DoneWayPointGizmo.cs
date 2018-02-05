using UnityEngine;
using System.Collections;

public class DoneWayPointGizmo : MonoBehaviour {

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "wayPoint.png", true);
    }
}

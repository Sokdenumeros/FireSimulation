using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movementScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    public GameObject head;
    private Vector3 up;
    void Start()
    {
        up = new Vector3(0f, 1f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 fw = new Vector3(head.transform.forward.x,0f, head.transform.forward.z);
        fw = fw.normalized;

        Vector3 displacement = fw * Input.GetAxis("Vertical");
        displacement += Vector3.Cross(up, fw) * Input.GetAxis("Horizontal");
        displacement += up * (Input.GetAxis("up") - Input.GetAxis("down"));
        transform.position += displacement * Time.deltaTime * speed;
    }
}

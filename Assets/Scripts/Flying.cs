using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bresenham3D;

public class Flying : MonoBehaviour
    {
    public float thrust = 10f;
    public Rigidbody rb;

    public ChunkManager manager;

    void Start()
    {
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Movement code.
        if (Input.GetKey(KeyCode.W))
        {
        rb.AddForce(transform.forward * thrust);
        }
        if (Input.GetKey(KeyCode.S))
        {
        rb.AddForce(transform.forward * -thrust);
        }
        if (Input.GetKey(KeyCode.D))
        {
        rb.AddForce(transform.right* thrust);
        }
        if (Input.GetKey(KeyCode.A))
        {
        rb.AddForce(transform.right* -thrust);
        }
        if (Input.GetKey(KeyCode.Space)) 
        {
            if (rb.velocity.y < 0.01f && rb.velocity.y > -0.01f || true) {
                rb.AddForce(new Vector3(0, thrust / 4.0f, 0), ForceMode.Impulse);
            }
        }

        if (Input.GetMouseButton(0)) {
            var position = transform.position; 
            var direction = transform.forward;
            var normalized_direction = direction.normalized * 5;
            var line_iter = new Bresenham3D(position, position + normalized_direction);

            foreach (Vector3Int cube in line_iter) {
                
                manager.RemoveBlock(cube);
            }
            manager.ForceReload();
            print("---");
            
        }
        // rb.transform.Rotate( Input.GetAxis("Vertical"), 0.0f, -Input.GetAxis("Horizontal") );

    }
}

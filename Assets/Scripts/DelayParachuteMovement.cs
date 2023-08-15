using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayParachuteMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject Mesh;
    public Transform player { get; set; }
    private float speed = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 finalPlayerPos = new Vector3(player.position.x, player.position.y + 2f, player.position.z);
        transform.position = Vector3.MoveTowards(transform.position, finalPlayerPos, speed * Time.deltaTime);
        //transform.rotation = Quaternion.LookRotation((transform.position - player.position).normalized);
        Mesh.transform.LookAt((transform.position - (player.position - Mesh.transform.position)));
        //transform.rotation = Quaternion.Euler(transform.rotation.x, 180f, transform.rotation.z);
        //transform.LookAt(player.transform, Vector3.up);
    }
}

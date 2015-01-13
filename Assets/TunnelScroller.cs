using UnityEngine;
using System.Collections;

public class TunnelScroller : MonoBehaviour
{
    public float velocity = 2;

    float position;

    Kvant.Tunnel tunnel1;
    Kvant.Tunnel tunnel2;
    Kvant.Tunnel tunnel3;

    void Start()
    {
        tunnel2 = GetComponentInChildren<Kvant.Tunnel>();

        var tunnel1GO = Instantiate(tunnel2.gameObject) as GameObject;
        var tunnel3GO = Instantiate(tunnel2.gameObject) as GameObject;

        tunnel1 = tunnel1GO.GetComponent<Kvant.Tunnel>();
        tunnel3 = tunnel3GO.GetComponent<Kvant.Tunnel>();

        tunnel1.offset = +tunnel2.density;
        tunnel3.offset = -tunnel2.density;

        tunnel1GO.transform.parent = transform;
        tunnel3GO.transform.parent = transform;

        tunnel1GO.transform.localPosition = Vector3.forward * +tunnel2.height;
        tunnel3GO.transform.localPosition = Vector3.forward * -tunnel2.height;
    }

    void Update()
    {
        var step = tunnel1.height * 2 / tunnel1.stacks;

        position += velocity * Time.deltaTime;

        transform.localPosition = Vector3.forward * -(position % step);

        tunnel2.offset = Mathf.Floor(position / step) * tunnel2.density * 2 / tunnel2.stacks;
        tunnel1.offset = tunnel2.offset + tunnel2.density;
        tunnel3.offset = tunnel2.offset - tunnel2.density;
    }
}

using UnityEngine;

public class LineConnector : MonoBehaviour
{
    LineRenderer ln;
    [SerializeField]
    private GameObject connector;

    [SerializeField]
    private GameObject weight1;
    [SerializeField]
    private GameObject weight1Parent;

    [SerializeField]
    private GameObject weight2;

    [SerializeField]
    private GameObject weight2Parent;
    [SerializeField]
    private int amountOfConnectors;
    private Transform[] connectorArray;
    private Vector3[] vectors;

    float lerpValue = 0f;
    float distance = 0f;
    Vector3 instantiatePosition;

    // Start is called before the first frame update
    void Start()
    {
        ln = GetComponent<LineRenderer>();
        ln.positionCount = amountOfConnectors + 2;
        InstantiateSegments();
        weight1.transform.SetParent(weight1Parent.transform);
        weight2.transform.SetParent(weight2Parent.transform);

    }

    void InstantiateSegments()
    {
        connectorArray = new Transform[amountOfConnectors];
        vectors = new Vector3[amountOfConnectors + 2];

        distance = 1f / (amountOfConnectors + 1);

        for (int i = 0; i < amountOfConnectors; i++)
        {
            lerpValue += distance;

            instantiatePosition = Vector3.Lerp(weight1.transform.position, weight2.transform.position, lerpValue);

            GameObject g = Instantiate(connector, instantiatePosition, transform.rotation);

            if (i < amountOfConnectors / 2)
                g.GetComponent<ConfigurableJoint>().connectedBody = weight1.GetComponent<Rigidbody>();
            else
                g.GetComponent<ConfigurableJoint>().connectedBody = weight1.GetComponent<Rigidbody>();

            if(i == 0)
            {
                g.GetComponent<SpringJoint>().connectedBody = weight1.GetComponent<Rigidbody>();

            }
            else if(i == amountOfConnectors - 1)
            {
                g.GetComponent<SpringJoint>().connectedBody = weight2.GetComponent<Rigidbody>();

            }
            else
            {
                g.GetComponent<SpringJoint>().connectedBody = connectorArray[i - 1].gameObject.GetComponent<Rigidbody>();

            }

            g.transform.SetParent(this.transform);
            connectorArray[i] = g.transform;
        }

    }

    // Update is called once per frame
    void Update()
    {
        int index = 1;

        vectors[0] = weight1.transform.position;
        foreach (Transform t in connectorArray)
        {
            vectors[index] = t.transform.position;
            index++;
        }
        vectors[amountOfConnectors + 1] = weight2.transform.position;

        //weight1.transform.LookAt(weight2.transform);
        //weight2.transform.LookAt(weight1.transform);

        ln.SetPositions(vectors);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    private const int MARKER_NUM = 11;

    [SerializeField]
    private string UDP_IP = "127.0.0.1";
    [SerializeField]
    private int UDP_PORT = 5000;
    [SerializeField]
    private float CAM_SCALE = 0.01f;
    [SerializeField]
    private int CAM_WIDTH = 1920;
    [SerializeField]
    private int CAM_HEIGHT = 1080;

    private UDPController udpController;
    [SerializeField]
    private List<Transform> markerTF;
    private Vector3[] markerPos = new Vector3[MARKER_NUM];

    [SerializeField]
    private GameObject markerPrefab;

    [SerializeField]
    private Transform headTF;

    [SerializeField]
    private float headRatio = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        udpController = new UDPController(UDP_IP, UDP_PORT);
        udpController.Received += ReseiveUDP;
        udpController.ListenStart();

        for (int i = 0; i < MARKER_NUM; i++)
        {
            GameObject obj = Instantiate(markerPrefab);
            markerTF.Add(obj.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < MARKER_NUM; i++)
        {
            if (markerTF[i] != null && markerPos[i] != Vector3.zero)
            {
                markerTF[i].transform.localPosition = markerPos[i];
            }
        }
        float top = (markerPos[1].y + markerPos[2].y) / 2;
        float bottom = markerPos[10].y;
        float size = Mathf.Abs(top - bottom);
        headTF.localPosition = new Vector3(markerPos[0].x, top + size * (headRatio - 1), 0);
    }

    private void OnDestroy()
    {
        if (udpController != null)
        {
            udpController.Dispose();
        }
    }

    private void ReseiveUDP(string strMsg)
    {
        string[] parseMsg = strMsg.Split(":");
        int objIndex = int.Parse(parseMsg[0]);
        string[] strPos = parseMsg[1].Split(",");
        Vector2 pos = new((float.Parse(strPos[0])- CAM_WIDTH / 2) * CAM_SCALE, (CAM_HEIGHT / 2 - float.Parse(strPos[1])) * CAM_SCALE);

        markerPos[objIndex] = new Vector3(pos.x, pos.y, 0);

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    private const int MARKER_NUM = 17;

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
    private float triggerDist = 0.2f;
    [SerializeField]
    private float releaseDist = 8;
    [SerializeField]
    private float releaseTriggerDist = 6;

    [SerializeField]
    private GameObject kamehame_charge;
    [SerializeField]
    private GameObject kamehame_beam;

    private Vector3 chargeSize;

    private const float DIS_CHARGE_TIME = 1.5f;
    private float disChargeTime = 0;
    private const float CHARGE_TIME = 1.5f;
    private float chargeTime = 0;

    [SerializeField]
    private GameObject beamMusic;
    [SerializeField]
    private GameObject chargeMusic;

    [SerializeField]
    private GameObject unityChan;
    [SerializeField]
    private float tfDist = 6f;

    private float unityChanDisTime = 0;
    private const float UNITY_CHAN_DIS_TIME = 1f;

    [SerializeField]
    private float fusionLRDist = 6f;
    [SerializeField]
    private float fusionUpperDist = 2f;

    [SerializeField]
    private int fusionStatus = 0;
    private bool reftFusion = false;

    private int lastFusinoStatus = 0;
    private const float DIS_FUSION_TIME = 1.5f;
    private float disFusionTime = 0;

    public bool noMove = false;

    // Start is called before the first frame update
    void Start()
    {
        udpController = new UDPController(UDP_IP, UDP_PORT);
        udpController.Received += ReseiveUDP;
        udpController.ListenStart();

        chargeSize = kamehame_charge.transform.localScale;

        /*
        for (int i = 0; i < MARKER_NUM; i++)
        {
            GameObject obj = Instantiate(markerPrefab);
            markerTF.Add(obj.transform);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        noMove = true;
        for (int i = 0; i < MARKER_NUM; i++)
        {
            if (markerTF[i] != null && markerPos[i] != Vector3.zero)
            {
                markerTF[i].transform.localPosition = markerPos[i];
            }
            if (markerPos[i] == Vector3.zero)
            {
                noMove = false;
            }
        }

        disChargeTime += Time.deltaTime;
        if (disChargeTime >= DIS_CHARGE_TIME)
        {
            kamehame_beam.SetActive(false);
            kamehame_charge.SetActive(false);
            beamMusic.SetActive(false);
            chargeMusic.SetActive(false);
            chargeTime = 0;
        }
        float hX = (markerPos[9].x + markerPos[10].x) / 2;
        float sX = (markerPos[5].x + markerPos[6].x) / 2;
        float hY = (markerPos[9].y + markerPos[10].y) / 2;
        float sY = (markerPos[5].y + markerPos[6].y) / 2;
        // フュージョン用
        switch (fusionStatus)
        {
            case 0:
                if (Mathf.Abs(sX - hX) >= fusionLRDist && sY + fusionUpperDist >= hY)
                {
                    fusionStatus++;
                    reftFusion = sX > hX;
                }
                break;
            case 1:
                if(sY + fusionUpperDist <= hY)
                {
                    fusionStatus++;
                }
                break;
            case 2:
                if (Mathf.Abs(sX - hX) >= fusionLRDist && sY + fusionUpperDist >= hY)
                {
                    if ((reftFusion && sX < hX) || ((!reftFusion) && sX > hX))
                    {
                        fusionStatus++;
                    }
                }
                break;
            case 3:
                if (Mathf.Abs(sX - hX) >= fusionLRDist && sY + fusionUpperDist >= hY)
                {
                    if ((reftFusion && sX > hX) || ((!reftFusion) && sX < hX))
                    {
                        fusionStatus = 0;
                        unityChan.SetActive(!unityChan.activeSelf);
                    }
                }
                break;
        }
        disFusionTime += Time.deltaTime;
        if(lastFusinoStatus != fusionStatus)
        {
            lastFusinoStatus = fusionStatus;
            disFusionTime = 0;
        }
        if(disFusionTime >= DIS_CHARGE_TIME && !unityChan.activeSelf)
        {
            fusionStatus = 0;
        }
        // 左右の距離確認
        if (markerPos[9] != Vector3.zero)
        {
            if ((kamehame_beam.activeSelf ? releaseTriggerDist : triggerDist) >= Vector3.Distance(markerPos[9], markerPos[10]))
            {
                kamehame_charge.SetActive(true);
                kamehame_charge.transform.localScale = chargeSize;
                kamehame_charge.transform.position = new Vector3((markerPos[9].x + markerPos[10].x) / 2, (markerPos[9].y + markerPos[10].y) / 2, 0);
                disChargeTime = 0;
                chargeTime += Time.deltaTime;
                if (chargeTime >= CHARGE_TIME)
                {
                    // 正規の処理
                    if (releaseDist <= Mathf.Abs(hX - sX))
                    {
                        kamehame_beam.SetActive(true);
                        kamehame_beam.transform.rotation = Quaternion.Euler(0, hX > sX ? 0 : 180, 0);
                    }
                }
                if (kamehame_beam.activeSelf)
                {
                    beamMusic.SetActive(true);
                    chargeMusic.SetActive(false);
                }
                else
                {
                    beamMusic.SetActive(false);
                    chargeMusic.SetActive(true);
                }
            }
        }
        if (markerPos[5] == Vector3.zero){
            unityChanDisTime += Time.deltaTime;
            if (unityChanDisTime >= UNITY_CHAN_DIS_TIME)
            {
                unityChan.SetActive(false);
            }
        }
        else
        {
            unityChanDisTime = 0;
        }
        kamehame_charge.transform.localScale = chargeSize * (1 - disChargeTime / DIS_CHARGE_TIME);
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
        float x = float.Parse(strPos[0]);
        float y = float.Parse(strPos[1]);
        if(x == 0 && y == 0)
        {
            markerPos[objIndex] = Vector3.zero;
            return;
        }
        Vector2 pos = new((x - CAM_WIDTH / 2) * CAM_SCALE, (CAM_HEIGHT / 2 - y) * CAM_SCALE);

        markerPos[objIndex] = new Vector3(pos.x, pos.y, 0);

    }
}

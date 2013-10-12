using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

public class OneProjectorCalibrate : MonoBehaviour
{

    WebCamTexture plantexture;
    WebCamDevice[] devices ;
    CalibrationSetting setting = new CalibrationSetting();
    private bool isConfiguring = true;
    private bool isDevChanged = false;
    public Transform root;
    public Transform plane;
    public string xmlPath = "c:/setting.xml";
	// Use this for initialization
	void Start () {
        devices = WebCamTexture.devices;
        try
        {
            setting = BobSerializor.Deserialize(xmlPath, typeof(CalibrationSetting)) as CalibrationSetting;
        }
        catch (Exception e) {
            BobSerializor.SerializeXML(xmlPath, setting);
        }
        isDevChanged = true;
	}

    void OnGUI() {
        if (!isConfiguring)
        {
            return;
        }

        GUI.contentColor = Color.black;
        GUILayout.BeginVertical();
        setting.isRootonTop = GUILayout.Toggle(setting.isRootonTop, setting.isRootonTop ? "绕上边缘旋转" : "绕下边缘旋转");
        if (setting.isRootonTop)
        {
            root.localPosition = new Vector3(0, 4.5f, 7.794229f);
            plane.localPosition = new Vector3(0, -4.5f, 0);
        }
        else
        {
            root.localPosition = new Vector3(0, -4.5f, 7.794229f);
            plane.localPosition = new Vector3(0, 4.5f, 0);
        }

        setting.CameraBackgroundColor = GUILayout.Toggle(setting.CameraBackgroundColor, setting.CameraBackgroundColor ? "黑色" : "红色");
        if (setting.CameraBackgroundColor)
        {
            Camera.main.backgroundColor = Color.black;
        }
        else
        {
            Camera.main.backgroundColor = Color.red;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("设备号: ");
        if (GUILayout.Button("<"))
        {
            setting.CurrentDevice -= 1;
            isDevChanged = true;
            if (setting.CurrentDevice < 0)
            {
                setting.CurrentDevice = 0;
                isDevChanged = false;
            }
        }
        GUILayout.Label(setting.CurrentDevice.ToString("000"));
        if (GUILayout.Button(">"))
        {
            setting.CurrentDevice += 1;
            isDevChanged = true;
            if (setting.CurrentDevice > devices.Length)
            {
                setting.CurrentDevice = devices.Length;
                isDevChanged = false;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("旋转角度: ");
        if (GUILayout.Button("^"))
        {
            setting.RotationXRoot += 0.5f;
        }
        GUILayout.Label(setting.RotationXRoot.ToString("000.00"));
        if (GUILayout.Button("v"))
        {
            setting.RotationXRoot -= 0.5f;
        }
        root.localRotation = Quaternion.Euler(setting.RotationXRoot, 0, 0);
        setting.RotationXRoot = root.localRotation.eulerAngles.x;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("宽度值: "+ setting.ScaleXRoot.ToString("0.00"));
        setting.ScaleXRoot = GUILayout.HorizontalSlider(setting.ScaleXRoot, -1f, 1f, GUILayout.Width(150f));
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("高度值: " + setting.ScaleYRoot.ToString("0.00"));
        setting.ScaleYRoot = GUILayout.HorizontalSlider(setting.ScaleYRoot, 0f, 1f, GUILayout.Width(150f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("高度翻转: " + setting.ScaleZPlane.ToString("0.00"));
        setting.ScaleZPlane = GUILayout.HorizontalSlider(setting.ScaleZPlane, -0.9f, 0.9f, GUILayout.Width(150f));
        GUILayout.EndHorizontal();


        GUILayout.EndVertical();

        root.localScale = new Vector3(setting.ScaleXRoot, setting.ScaleYRoot, root.localScale.z);
        plane.localScale = new Vector3(1.6f, 1f, setting.ScaleZPlane);

        if (isDevChanged) {
            if (setting.CurrentDevice == 0) {
                plane.renderer.material.mainTexture = null;
                return;
            }

            if (null != plantexture)
            {
                plantexture.Stop();
            }
            plantexture = new WebCamTexture(WebCamTexture.devices[setting.CurrentDevice-1].name);
            plane.renderer.material.mainTexture = plantexture;
            plantexture.Play();
            isDevChanged = false;
        }
    }
	// Update is called once per frame
	void Update () {

        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.L))
        {
            BobSerializor.SerializeXML(xmlPath, setting);
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            isConfiguring = !isConfiguring;
            if (isConfiguring == false)
            {
                BobSerializor.SerializeXML(xmlPath, setting);
            }
        }
	}
}


[Serializable]
public class CalibrationSetting {
    public float ScaleXRoot = 1f;
    public float ScaleYRoot = 1f;
    public float RotationXRoot = 0f;

    public float ScaleZPlane = 0.9f;

    public bool isRootonTop = false;
    public bool CameraBackgroundColor = false;
    public int CurrentDevice = 0;
}


public class BobSerializor {
    public static void SerializeXML(string path, object o) {
        using (StreamWriter xml = new StreamWriter(path)) {
            XmlSerializer xmlserializer = new XmlSerializer(o.GetType());
            xmlserializer.Serialize(xml, o);
            xml.Close();
        }
    }

    public static object Deserialize(string path, Type type) {
        using (StreamReader xmlreader = new StreamReader(path)) {
                XmlSerializer xmlserializer = new XmlSerializer(type);
                return xmlserializer.Deserialize(xmlreader);
        }
    }
}


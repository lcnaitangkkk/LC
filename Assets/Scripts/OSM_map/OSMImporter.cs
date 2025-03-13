using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class OSMRoadParser
{
    public class OSMNode
    {
        public long Id;
        public double Lat;//γ��
        public double Lon;//����
    }

    public class OSMWay
    {
        public long Id;
        public List<long> NodeIds = new List<long>(); // ������ɵ�·�Ľڵ�ID
    }

    // �������еĽڵ�͵�·��Ϣ
    public List<OSMNode> Nodes = new List<OSMNode>();
    public List<OSMWay> Ways = new List<OSMWay>();

    // ����OSM�ļ�����ȡ��·�;�γ����Ϣ
    public void ParseOSMFile(string filePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        // �������нڵ㣨<node>��ǩ��
        XmlNodeList nodeList = doc.GetElementsByTagName("node");
        foreach (XmlNode node in nodeList)
        {
            OSMNode osmNode = new OSMNode
            {
                Id = long.Parse(node.Attributes["id"].Value),
                Lat = double.Parse(node.Attributes["lat"].Value),
                Lon = double.Parse(node.Attributes["lon"].Value)
            };
            Nodes.Add(osmNode);
        }

        // �������е�·��<way>��ǩ��
        XmlNodeList wayList = doc.GetElementsByTagName("way");
        foreach (XmlNode way in wayList)
        {
            OSMWay osmWay = new OSMWay
            {
                Id = long.Parse(way.Attributes["id"].Value)
            };

            // ��ȡ��·�ڵ��ID
            foreach (XmlNode nd in way.SelectNodes("nd"))
            {
                osmWay.NodeIds.Add(long.Parse(nd.Attributes["ref"].Value));
            }

            Ways.Add(osmWay);
        }
    }

    // ��ȡ��·�ľ�γ����Ϣ�����������ź�ԭ�����
    public List<Vector3> GetRoadCoordinates(OSMWay way, float scaleFactor, Vector3 offset)
    {
        List<Vector3> roadCoordinates = new List<Vector3>();

        foreach (var nodeId in way.NodeIds)
        {
            OSMNode node = Nodes.Find(n => n.Id == nodeId);
            if (node != null)
            {
                // ����γ��ת��Ϊ3D���꣬Ӧ�����ź�ԭ�����
                Vector3 position = new Vector3(
                    (float)(node.Lon * scaleFactor) - offset.x,  // ���Ų���������
                    0,  // ���Ը�����Ҫ�����߶�
                    (float)(node.Lat * scaleFactor) - offset.z   // ���Ų�����γ��
                );
                roadCoordinates.Add(position);
            }
        }

        return roadCoordinates;
    }
}

public class OSMImporter : MonoBehaviour
{
    private string osmFilePath = Path.Combine(Application.dataPath, "OSM/map1.osm");
    public Material lineMaterial;  // ������Ⱦ�����Ĳ���
    public float scaleFactor = 1000f; // ��������
    public Vector3 offset = new Vector3(0, 0, 0);  // ԭ��ƫ����

    // ��������������������е��������Ŀ��
    public float lineWidth = 0.01f;  // �����������

    void Start()
    {
        // ����OSMRoadParser������OSM�ļ�
        OSMRoadParser parser = new OSMRoadParser();
        parser.ParseOSMFile(osmFilePath);

        // ����ÿһ����·�������Ƶ�·������
        foreach (var way in parser.Ways)
        {
            List<Vector3> roadCoordinates = parser.GetRoadCoordinates(way, scaleFactor, offset);

            // ���������ԣ�ȷ������ֵ
            foreach (var coord in roadCoordinates)
            {
                Debug.Log(coord);
            }

            // ����ÿһ����·������
            if (roadCoordinates.Count > 0)
            {
                DrawRoadLine(roadCoordinates);
            }
        }
    }

    // ʹ��LineRenderer���Ƶ�·������
    void DrawRoadLine(List<Vector3> roadCoordinates)
    {
        // ����һ���յ�GameObject
        GameObject roadObject = new GameObject("RoadLine");

        // ���LineRenderer���
        LineRenderer lineRenderer = roadObject.AddComponent<LineRenderer>();

        // ���ò���
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogWarning("Line Material is not assigned in the inspector.");
        }

        // ����LineRenderer�Ļ�������
        lineRenderer.positionCount = roadCoordinates.Count;  // ���ö�������
        lineRenderer.SetPositions(roadCoordinates.ToArray()); // ������������λ��
        lineRenderer.startWidth = lineWidth;  // �����������
        lineRenderer.endWidth = lineWidth;    // �����������
        lineRenderer.useWorldSpace = true;  // ʹ����������

        // ��������������λ��
        Vector3 center = Vector3.zero;
        foreach (var coord in roadCoordinates)
        {
            center += coord;
        }
        center /= roadCoordinates.Count;  // �������е��ƽ��λ��

        // ����GameObject��Transformλ��
        roadObject.transform.position = center;
    }
}

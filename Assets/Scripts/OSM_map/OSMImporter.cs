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
        public double Lat;//纬度
        public double Lon;//经度
    }

    public class OSMWay
    {
        public long Id;
        public List<long> NodeIds = new List<long>(); // 包含组成道路的节点ID
    }

    // 保存所有的节点和道路信息
    public List<OSMNode> Nodes = new List<OSMNode>();
    public List<OSMWay> Ways = new List<OSMWay>();

    // 解析OSM文件，提取道路和经纬度信息
    public void ParseOSMFile(string filePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        // 解析所有节点（<node>标签）
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

        // 解析所有道路（<way>标签）
        XmlNodeList wayList = doc.GetElementsByTagName("way");
        foreach (XmlNode way in wayList)
        {
            OSMWay osmWay = new OSMWay
            {
                Id = long.Parse(way.Attributes["id"].Value)
            };

            // 获取道路节点的ID
            foreach (XmlNode nd in way.SelectNodes("nd"))
            {
                osmWay.NodeIds.Add(long.Parse(nd.Attributes["ref"].Value));
            }

            Ways.Add(osmWay);
        }
    }

    // 获取道路的经纬度信息，并进行缩放和原点调整
    public List<Vector3> GetRoadCoordinates(OSMWay way, float scaleFactor, Vector3 offset)
    {
        List<Vector3> roadCoordinates = new List<Vector3>();

        foreach (var nodeId in way.NodeIds)
        {
            OSMNode node = Nodes.Find(n => n.Id == nodeId);
            if (node != null)
            {
                // 将经纬度转换为3D坐标，应用缩放和原点调整
                Vector3 position = new Vector3(
                    (float)(node.Lon * scaleFactor) - offset.x,  // 缩放并调整经度
                    0,  // 可以根据需要调整高度
                    (float)(node.Lat * scaleFactor) - offset.z   // 缩放并调整纬度
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
    public Material lineMaterial;  // 用于渲染线条的材质
    public float scaleFactor = 1000f; // 缩放因子
    public Vector3 offset = new Vector3(0, 0, 0);  // 原点偏移量

    // 公共变量，允许在面板中调整线条的宽度
    public float lineWidth = 0.01f;  // 设置线条宽度

    void Start()
    {
        // 创建OSMRoadParser并解析OSM文件
        OSMRoadParser parser = new OSMRoadParser();
        parser.ParseOSMFile(osmFilePath);

        // 遍历每一条道路，并绘制道路的线条
        foreach (var way in parser.Ways)
        {
            List<Vector3> roadCoordinates = parser.GetRoadCoordinates(way, scaleFactor, offset);

            // 输出坐标调试，确认坐标值
            foreach (var coord in roadCoordinates)
            {
                Debug.Log(coord);
            }

            // 绘制每一条道路的线条
            if (roadCoordinates.Count > 0)
            {
                DrawRoadLine(roadCoordinates);
            }
        }
    }

    // 使用LineRenderer绘制道路的线条
    void DrawRoadLine(List<Vector3> roadCoordinates)
    {
        // 创建一个空的GameObject
        GameObject roadObject = new GameObject("RoadLine");

        // 添加LineRenderer组件
        LineRenderer lineRenderer = roadObject.AddComponent<LineRenderer>();

        // 设置材质
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogWarning("Line Material is not assigned in the inspector.");
        }

        // 设置LineRenderer的基本属性
        lineRenderer.positionCount = roadCoordinates.Count;  // 设置顶点数量
        lineRenderer.SetPositions(roadCoordinates.ToArray()); // 设置线条顶点位置
        lineRenderer.startWidth = lineWidth;  // 设置线条宽度
        lineRenderer.endWidth = lineWidth;    // 设置线条宽度
        lineRenderer.useWorldSpace = true;  // 使用世界坐标

        // 计算线条的中心位置
        Vector3 center = Vector3.zero;
        foreach (var coord in roadCoordinates)
        {
            center += coord;
        }
        center /= roadCoordinates.Count;  // 计算所有点的平均位置

        // 更新GameObject的Transform位置
        roadObject.transform.position = center;
    }
}

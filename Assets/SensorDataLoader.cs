/*
 * Copyright 2024 Haply Robotics Inc. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// 돼지 피부 계측 데이터를 CSV 파일에서 로드하는 클래스
/// </summary>
public class SensorDataLoader : MonoBehaviour
{
    [System.Serializable]
    public struct SensorDataPoint
    {
        public float elapsedTime;
        public Vector3 force; // X, Y, Z 힘 값
        public int sampleNumber;
        
        public SensorDataPoint(float time, float x, float y, float z, int sample)
        {
            elapsedTime = time;
            force = new Vector3(x, y, z);
            sampleNumber = sample;
        }
    }

    [System.Serializable]
    public class SensorDataSet
    {
        public string fileName;
        public List<SensorDataPoint> dataPoints;
        public float duration;
        public float samplingRate;
        
        public SensorDataSet(string name)
        {
            fileName = name;
            dataPoints = new List<SensorDataPoint>();
        }
    }

    /// <summary>
    /// CSV 파일에서 센서 데이터를 로드합니다.
    /// </summary>
    /// <param name="filePath">CSV 파일 경로 (Assets/Data/SensorData/ 기준)</param>
    /// <returns>로드된 데이터셋</returns>
    public static SensorDataSet LoadCSV(string filePath)
    {
        SensorDataSet dataSet = new SensorDataSet(Path.GetFileName(filePath));
        
        // StreamingAssets 또는 Assets 경로 확인
        string fullPath = GetFullPath(filePath);
        
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"파일을 찾을 수 없습니다: {fullPath}");
            return dataSet;
        }

        try
        {
            string[] lines = File.ReadAllLines(fullPath);
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] values = line.Split(',');
                
                if (values.Length < 6)
                    continue;

                // CSV 형식: timestamp, elapsed_time, sample, X, Y, Z
                if (float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float elapsedTime) &&
                    int.TryParse(values[2], out int sample) &&
                    float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    SensorDataPoint point = new SensorDataPoint(elapsedTime, x, y, z, sample);
                    dataSet.dataPoints.Add(point);
                }
            }

            // 통계 계산
            if (dataSet.dataPoints.Count > 0)
            {
                dataSet.duration = dataSet.dataPoints[dataSet.dataPoints.Count - 1].elapsedTime - 
                                  dataSet.dataPoints[0].elapsedTime;
                dataSet.samplingRate = dataSet.dataPoints.Count / dataSet.duration;
            }

            Debug.Log($"데이터 로드 완료: {dataSet.fileName} - {dataSet.dataPoints.Count}개 샘플, " +
                     $"지속시간: {dataSet.duration:F3}초, 샘플링 레이트: {dataSet.samplingRate:F2}Hz");
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV 파일 로드 오류: {e.Message}");
        }

        return dataSet;
    }

    /// <summary>
    /// 전체 경로를 반환합니다 (StreamingAssets 또는 Assets 기준)
    /// </summary>
    private static string GetFullPath(string relativePath)
    {
        // StreamingAssets 경로 시도
        string streamingPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (File.Exists(streamingPath))
            return streamingPath;

        // Assets 경로 시도
        string assetsPath = Path.Combine(Application.dataPath, relativePath);
        if (File.Exists(assetsPath))
            return assetsPath;

        // 상대 경로 그대로 반환
        return relativePath;
    }

    /// <summary>
    /// 경과 시간에 해당하는 힘 값을 보간하여 반환합니다.
    /// </summary>
    /// <param name="dataSet">데이터셋</param>
    /// <param name="elapsedTime">경과 시간</param>
    /// <returns>보간된 힘 벡터</returns>
    public static Vector3 GetInterpolatedForce(SensorDataSet dataSet, float elapsedTime)
    {
        if (dataSet == null || dataSet.dataPoints.Count == 0)
            return Vector3.zero;

        // 경계 체크
        if (elapsedTime <= dataSet.dataPoints[0].elapsedTime)
            return dataSet.dataPoints[0].force;

        if (elapsedTime >= dataSet.dataPoints[dataSet.dataPoints.Count - 1].elapsedTime)
            return dataSet.dataPoints[dataSet.dataPoints.Count - 1].force;

        // 이진 검색으로 적절한 인덱스 찾기
        int index = FindIndex(dataSet.dataPoints, elapsedTime);
        
        if (index < 0 || index >= dataSet.dataPoints.Count - 1)
            return Vector3.zero;

        // 선형 보간
        SensorDataPoint point1 = dataSet.dataPoints[index];
        SensorDataPoint point2 = dataSet.dataPoints[index + 1];

        float t = (elapsedTime - point1.elapsedTime) / (point2.elapsedTime - point1.elapsedTime);
        return Vector3.Lerp(point1.force, point2.force, t);
    }

    /// <summary>
    /// 이진 검색으로 경과 시간에 해당하는 인덱스를 찾습니다.
    /// </summary>
    private static int FindIndex(List<SensorDataPoint> points, float elapsedTime)
    {
        int left = 0;
        int right = points.Count - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            
            if (points[mid].elapsedTime <= elapsedTime && 
                (mid == points.Count - 1 || points[mid + 1].elapsedTime > elapsedTime))
            {
                return mid;
            }
            
            if (points[mid].elapsedTime > elapsedTime)
                right = mid - 1;
            else
                left = mid + 1;
        }

        return Mathf.Clamp(left, 0, points.Count - 1);
    }

    /// <summary>
    /// 데이터셋의 통계 정보를 반환합니다.
    /// </summary>
    public static void PrintStatistics(SensorDataSet dataSet)
    {
        if (dataSet == null || dataSet.dataPoints.Count == 0)
        {
            Debug.LogWarning("데이터셋이 비어있습니다.");
            return;
        }

        Vector3 minForce = dataSet.dataPoints[0].force;
        Vector3 maxForce = dataSet.dataPoints[0].force;
        Vector3 sumForce = Vector3.zero;
        float maxMagnitude = 0f;

        foreach (var point in dataSet.dataPoints)
        {
            minForce.x = Mathf.Min(minForce.x, point.force.x);
            minForce.y = Mathf.Min(minForce.y, point.force.y);
            minForce.z = Mathf.Min(minForce.z, point.force.z);

            maxForce.x = Mathf.Max(maxForce.x, point.force.x);
            maxForce.y = Mathf.Max(maxForce.y, point.force.y);
            maxForce.z = Mathf.Max(maxForce.z, point.force.z);

            sumForce += point.force;

            float magnitude = point.force.magnitude;
            if (magnitude > maxMagnitude)
                maxMagnitude = magnitude;
        }

        Vector3 avgForce = sumForce / dataSet.dataPoints.Count;

        Debug.Log($"=== {dataSet.fileName} 통계 ===");
        Debug.Log($"샘플 수: {dataSet.dataPoints.Count}");
        Debug.Log($"지속 시간: {dataSet.duration:F3}초");
        Debug.Log($"샘플링 레이트: {dataSet.samplingRate:F2}Hz");
        Debug.Log($"평균 힘: ({avgForce.x:F6}, {avgForce.y:F6}, {avgForce.z:F6})");
        Debug.Log($"최소 힘: ({minForce.x:F6}, {minForce.y:F6}, {minForce.z:F6})");
        Debug.Log($"최대 힘: ({maxForce.x:F6}, {maxForce.y:F6}, {maxForce.z:F6})");
        Debug.Log($"최대 힘 크기: {maxMagnitude:F6}");
    }
}


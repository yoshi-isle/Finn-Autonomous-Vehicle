using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;

public class ArduinoDistanceReader : MonoBehaviour
{
    public Dictionary<Vector3, GameObject> worldMapCreated = new();
    public TextMeshProUGUI distanceText;
    public GameObject worldMap;
    public GameObject wallPrefab;
    [Header("Serial Settings")]
    public string portName = "COM6";
    public int baudRate = 9600;

    [Header("Live Data")]
    public float currentDistance = 0f;
    public bool isConnected = false;

    private SerialPort _serialPort;
    private string _incomingLine = "";

    private void Start()
    {
        TryConnect();
    }

    private void TryConnect()
    {
        try
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.ReadTimeout = 50;
            _serialPort.Open();
            isConnected = true;
            Debug.Log("Connected to Arduino on " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open port " + portName + e.Message);
        }
    }

    private void Update()
    {
        if (!isConnected || _serialPort == null) return;

        try
        {
            while (_serialPort.BytesToRead > 0)
            {
                _incomingLine = _serialPort.ReadLine().Trim();

                if (!_incomingLine.Contains("Distance:")) continue;
                
                // Example input: "12:24:29.929 -> Distance: 15.60 cm"
                var parts = _incomingLine.Split(':');
                if (parts.Length < 2) continue;
                var numberPart = parts[parts.Length - 1].Replace("cm", "").Trim();
                if (float.TryParse(numberPart, out var dist))
                {
                    currentDistance = dist;
                    // Debug.Log("Distance updated: " + currentDistance + " cm");
                }
            }
        }
        catch (TimeoutException) { }
        catch (System.Exception e)
        {
            Debug.LogWarning("Serial read issue: " + e.Message);
        }
        
        
        distanceText.text = "Distance: " + currentDistance + "cm";
        
        
        
        //loop through dictionary
        var toRemove = new List<Vector3>();
        foreach (var entry in worldMapCreated)
        {

            if (entry.Value == null)
            {
                
                toRemove.Add(entry.Key);
            }
            else
            {
                if (worldMapCreated[entry.Key].GetComponent<ObjectIdentifier>()._counter < 300)
                {
                    worldMapCreated[entry.Key].GetComponent<ObjectIdentifier>().IncreaseCounter(-1);
                }

            }
        }

        foreach (var remove in toRemove)
        {
            // Remove the dictionary entry
            worldMapCreated.Remove(remove);
        }
        
        print("Check grid location: " + DistanceToGridLocation(currentDistance));
        if (worldMapCreated.ContainsKey(DistanceToGridLocation(currentDistance)))
        {
            worldMapCreated[DistanceToGridLocation(currentDistance)].GetComponent<ObjectIdentifier>().IncreaseCounter(2);
            return;
        }
        
        if (DistanceToGridLocation(currentDistance) == Vector3.zero)
        {
            return;
        }

        var wall = Instantiate(wallPrefab, DistanceToGridLocation(currentDistance), Quaternion.identity);
        wall.transform.SetParent(worldMap.transform);
        worldMapCreated[DistanceToGridLocation(currentDistance)] = wall;

    }

    private static Vector3 DistanceToGridLocation(float distance)
    {
        // Convert distance to a grid location.
        // For now just assume x 0
        return new Vector3(0, 0, (int)distance);
    }

    private void OnApplicationQuit()
    {
        if (_serialPort != null && _serialPort.IsOpen)
            _serialPort.Close();
    }
}
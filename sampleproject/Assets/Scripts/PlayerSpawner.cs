﻿using System.Collections;
using System.Collections.Generic;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    const byte SPAWN_TAG = 0;

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    GameObject networkPrefab;

    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (controllablePrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (networkPrefab == null)
        {
            Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        client.MessageReceived += SpawnPlayer;
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (message.Tag == Tags.SpawnPlayerTag)
            {
                if (reader.Length % 17 != 0)
                {
                    Debug.LogWarning("Received malformed spawn packet.");
                    return;
                }

                while (reader.Position < reader.Length)
                {
                    ushort id = reader.ReadUInt16();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                    float radius = reader.ReadSingle();
                    Color32 color = new Color32(
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        255
                    );

                    GameObject obj;
                    if (id == client.ID)
                    {
                        obj = Instantiate(controllablePrefab, position, Quaternion.identity) as GameObject;
                    }
                    else
                    {
                        obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                    }

                    AgarObject agarObj = obj.GetComponent<AgarObject>();

                    agarObj.SetRadius(radius);
                    agarObj.SetColor(color);
                }
            }
        }
    }
}
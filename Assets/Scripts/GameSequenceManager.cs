using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameSequenceManager : MonoBehaviour
{
    public UnityEvent OnPlayerEnterRoom;

    [SerializeField]
    PlayerReferencer playerReferencer;

    void Start()
    {
        OnPlayerEnterRoom.AddListener(() => playerReferencer.playerMovement.AutoCrawl(false));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Policy;
using System.Linq.Expressions;

public static class StaticEventHandler 
{
    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }

    // Burayi anlamadim??

    // Room enemies defeated event
    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;

    public static void CallRoomEnemiesDefeatedEvent(Room room)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs() { room = room });
    }

    // Points Scored event. Game Manager subscribe olacak ve Score Changed eventini gonderecek UI icin
    public static event Action<PointsScoredArgs> OnPointsScored;
    // Enemy defeat oldugunda enemy'nin health'i kadar bir point publish edecek.
    public static void CallPointsScoredEvent(int points)
    {
        OnPointsScored?.Invoke(new PointsScoredArgs() {points=points});
    }

    // Score Changed Event. UI Score subscribe olacak.
    public static event Action<ScoreChangedArgs> OnScoreChanged;
    // Game Manager publish edecek. Aktif multiplier ve score'u birlikte gonderecek.
    public static void CallScoreChangedEvent(long score, int multiplier)
    {
        OnScoreChanged?.Invoke(new ScoreChangedArgs(){score=score, multiplier=multiplier});
    }
    // Game manager subscribe olacak.
    public static event Action<MultiplierArgs> OnMultiplier;
    // ammo publish edecek
    public static void CallMultiplierEvent(bool multiplier)
    {
        OnMultiplier?.Invoke(new MultiplierArgs() {multiplier=multiplier});
    }
    
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
}

public class PointsScoredArgs : EventArgs
{
    public int points;
}

public class ScoreChangedArgs : EventArgs
{
    public long score;
    public int multiplier;
}

public class MultiplierArgs : EventArgs
{
    public bool multiplier; 
}


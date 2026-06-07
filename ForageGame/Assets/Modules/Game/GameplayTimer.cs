using TDK.SaveSystem;
using UnityEngine;

public class GameplayTimer : MonoBehaviour, ISaveable
{
    private double? _gameTimerStart;

    void Start() => ResetTimer();

    private void ResetTimer()
    {
        _gameTimerStart = Time.timeAsDouble;
    }

    public void SaveData(ref WorldSaveData data)
    {
        if (_gameTimerStart == null) return;
        double recordedTime = Time.timeAsDouble - (double)_gameTimerStart;
        data.playtimeSeconds += (int)recordedTime;
        ResetTimer();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveGame
{
    void LoadData(SaveGameData data);

    void SaveData(SaveGameData data);
}

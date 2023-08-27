using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleton
{
    private static GameSingleton instance = null;

    private GameSingleton()
    {

    }

    public static GameSingleton Instance
    {
        get
        {
            return instance;
        }
    }
}

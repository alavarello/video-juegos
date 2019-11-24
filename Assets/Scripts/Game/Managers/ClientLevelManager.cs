using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class ClientLevelManager: MonoBehaviour
{
    
    public static int level = 1;

    public static float baseTime;

    public static float showTime = 4f;
    
    
    Text text;


    void Awake ()
    {
        text = GetComponent <Text> ();
        level = 1;
    }

    public static void LevelUp(int level)
    {
        ClientLevelManager.level = level;

        baseTime = Time.time;
    }

    void Update ()
    {

        if (baseTime + showTime > Time.time)
        {
            text.text = "Level: " + level;
        }
        else
        {
            text.text = "";
        }
    }
}
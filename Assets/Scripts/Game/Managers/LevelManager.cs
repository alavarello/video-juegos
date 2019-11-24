using UnityEngine;

public class LevelManager
{
    public static int level = 1;

    public static int amountOfEnemyInLevel = 20;

    public static int enemiesSpawn = 1;

    public static float levelRestartTime = 2f;

    public static float levelUpTime = 0;

    public static void LevelUp()
    {
        level++;
        amountOfEnemyInLevel += 20;
        enemiesSpawn = 1;
        levelUpTime = Time.time;
    }

    public static bool CanSpawn()
    {
        return levelUpTime + levelRestartTime > Time.time && amountOfEnemyInLevel > enemiesSpawn;
    }
    
    
}

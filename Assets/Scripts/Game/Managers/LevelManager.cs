using UnityEngine;

public class LevelManager
{
    public static int level = 1;

    public static int amountOfEnemyInLevel = 5;

    public static int enemiesSpawn = 1;

    public static float levelRestartTime = 2f;

    public static float levelUpTime = 0;
    
    public static int totalEnemies = amountOfEnemyInLevel;

    public static void LevelUp()
    {
        level++;
        amountOfEnemyInLevel += 5;
        totalEnemies += amountOfEnemyInLevel;
        enemiesSpawn = 1;
        levelUpTime = Time.time;
    }

    public static bool CanSpawn()
    {
        return levelUpTime + levelRestartTime < Time.time && amountOfEnemyInLevel >= enemiesSpawn;
    }
    
    
}

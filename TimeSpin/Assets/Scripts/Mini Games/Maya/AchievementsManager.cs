using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsManager : MonoBehaviour
{
    public static AchievementsManager instance;

    // Logros desbloqueados
    private HashSet<string> unlockedAchievements = new HashSet<string>();

    // Progreso para los logros
    private int consecutiveJumps = 0; // Salto consecutivo 
    private int consecutiveFalls = 0; // Caídas consecutivas 
    private bool noObstaclesHit = true; // Sin chocar ni caer para 
    private bool noFalls = true; // Sin caer en agujeros para 
    private float raceTimer = 0f; // Tiempo de la carrera para 
    private bool noJumpsUsed = true; // Sin saltar para 
    private int score = 0; // Puntuación del jugador para 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ResetProgress()
    {
        consecutiveJumps = 0;
        consecutiveFalls = 0;
        noObstaclesHit = true;
        noFalls = true;
        noJumpsUsed = true;
        raceTimer = 0f;
    }

    public void RegisterJump()
    {
        consecutiveJumps++;
        noJumpsUsed = false;
    }

    public void RegisterFall()
    {
        consecutiveFalls++;
        consecutiveJumps = 0; // Reset de saltos consecutivos
        noFalls = false;
    }

    public void RegisterObstacleHit()
    {
        noObstaclesHit = false;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void CompleteRace(float timer)
    {
        raceTimer = timer;
        CheckAchievements();
    }

    private void CheckAchievements()
    {
        if (!unlockedAchievements.Contains("AbandonoDeLasCiudades"))
        {
            AchievementManager.UnlockAchievement("Maya_AbandonoDeLasCiudades");
        }

        if (noObstaclesHit && noFalls)
        {
            AchievementManager.UnlockAchievement("Maya_CalendarioAvanzado");
        }

        if (consecutiveJumps >= 5)
        {
            AchievementManager.UnlockAchievement("Maya_ConocimientosAstronómicos");
        }

        if (raceTimer <= 60f)
        {
            AchievementManager.UnlockAchievement("Maya_EscrituraJeroglifica");
        }

        if (noJumpsUsed)
        {
            AchievementManager.UnlockAchievement("Maya_IngenieríaHidráulica");
        }

        if (consecutiveFalls >= 2)
        {
            AchievementManager.UnlockAchievement("Maya_JuegoDePelota");
        }

        if (noFalls)
        {
            AchievementManager.UnlockAchievement("Maya_PirámidesEscalonadas");
        }

        if (score >= 50)
        {
            AchievementManager.UnlockAchievement("Maya_SacrificiosHumanos");
        }
    }

    private void UnlockAchievement(string achievement)
    {
        if (!unlockedAchievements.Contains(achievement))
        {
            unlockedAchievements.Add(achievement);
            Debug.Log($"Logro desbloqueado: {achievement}");
        }
    }
}


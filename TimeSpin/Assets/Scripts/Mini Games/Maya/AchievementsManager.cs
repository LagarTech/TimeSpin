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

    private bool logr01;
    private bool logr02;
    private bool logr03;
    private bool logr04;
    private bool logr05;
    private bool logr06;
    private bool logr07;
    private bool logr08;

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
        if (!unlockedAchievements.Contains("AbandonoDeLasCiudades") && !logr01)
        {
            AchievementManager.UnlockAchievement("Maya_AbandonoDeLasCiudades");
               logr01 = true;
        }

        if (noObstaclesHit && noFalls && !logr02)
        {
            AchievementManager.UnlockAchievement("Maya_CalendarioAvanzado");
            logr02 = true;
        }

        if (consecutiveJumps >= 5 && !logr03)
        {
            AchievementManager.UnlockAchievement("Maya_ConocimientosAstronómicos");
            logr03 = true;
        }

        if (raceTimer <= 60f && !logr04)
        {
            AchievementManager.UnlockAchievement("Maya_EscrituraJeroglifica");
            logr04 = true;
        }

        if (noJumpsUsed && !logr05)
        {
            AchievementManager.UnlockAchievement("Maya_IngenieríaHidráulica");
            logr05 = true;
        }

        if (consecutiveFalls >= 2 && !logr06)
        {
            AchievementManager.UnlockAchievement("Maya_JuegoDePelota");
            logr06 = true;
        }

        if (noFalls && !logr07)
        {
            AchievementManager.UnlockAchievement("Maya_PirámidesEscalonadas");
            logr07 = true;
        }

        if (score >= 50 && !logr08)
        {
            AchievementManager.UnlockAchievement("Maya_SacrificiosHumanos");
            logr08 = true;
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


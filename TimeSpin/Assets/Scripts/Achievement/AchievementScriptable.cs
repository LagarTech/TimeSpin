using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Achievements/New Achievement")]
public class AchievementScriptable : ScriptableObject
{
    public string Title; // Nombre del logro
    [TextArea] public string Description; // Descripción del logro
    [TextArea] public string Condition; // Condición para desbloquear
    public bool IsUnlocked; // Estado del logro (desbloqueado o no)
}


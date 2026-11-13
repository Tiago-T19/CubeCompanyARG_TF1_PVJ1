using UnityEngine;
using UnityEngine.AI;

public class EnemigoEspecial : EnemyAI
{
    [Header("Enemigo Especial")]
    public float duracionRalentizacion = 3f;
    [Range(0f, 1f)] public float factorRalentizacion = 0.5f;

    protected override void ExecuteAttack()
    {
        // 1. Dano normal. El cooldown lo maneja el padre en AttackCooldownRoutine
        base.ExecuteAttack();

        // 2. Ralentizacion: la maneja el Player, no el enemigo.
        var movPlayer = playerTransform != null ? playerTransform.GetComponent<PlayerMovement>() : null;
        if (movPlayer != null)
        {
            movPlayer.AplicarRalentizacion(factorRalentizacion, duracionRalentizacion);
        }
    }
}

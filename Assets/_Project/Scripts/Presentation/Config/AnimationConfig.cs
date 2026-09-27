using DG.Tweening;
using UnityEngine;

namespace NutSort.Presentation
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "NutSort/Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        [Header("Lift (select)")]
        public float LiftDuration = 0.18f;
        public Ease LiftEase = Ease.OutBack;

        [Header("Drop")]
        public float DropDuration = 0.2f;
        [Tooltip("back or bounce makes the nuts overlap")]
        public Ease DropEase = Ease.InQuad;

        [Header("Travel between bolts")]
        public float TravelDuration = 0.28f;
        public float TravelArcHeight = 1.5f;
        public float NutStagger = 0.05f;

        [Header("Spin")]
        public float SpinTurnsPerSlot = 1f;
        public int MaxSpinTurns = 3;
        public int SpinDirection = 1;

        [Header("Cap")]
        public float CapDropHeight = 2f;
        public float CapDuration = 0.3f;
        public Ease CapEase = Ease.OutBounce;
        public float CapPunch = 0.08f;

        [Header("Invalid move")]
        public float ShakeDuration = 0.25f;
        public float ShakeStrength = 0.3f;
        public int ShakeVibrato = 20;

        [Header("Win")]
        public float WinBoltStagger = 0.06f;
        public float WinPunch = 0.15f;
        public float WinPunchDuration = 0.35f;
        public float WinNutHop = 0.6f;
        public float WinPanelDelay = 0.3f;
    }
}

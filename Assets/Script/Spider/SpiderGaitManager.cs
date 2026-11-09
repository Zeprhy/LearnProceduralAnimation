using Unity.VisualScripting;
using UnityEngine;

public class SpiderGaitManager : MonoBehaviour
{
    [Header("Grup 1 (Silang A)")]
    public SpiderLeg legFL; // Depan-Kiri
    public SpiderLeg legBR; // Belakang-Kanan

    [Header("Grup 2 (Silang B)")]
    public SpiderLeg legFR; // Depan-Kanan
    public SpiderLeg legBL; // Belakang-Kiri

    [Header("Sensitivitas Langkah")]
    public float stepDistance = 0.75f;

    // 0 = Giliran Grup 1, 1 = Giliran Grup 2
    private int activeGroup = 0;

    void Update()
    {
        bool group1Stepping =(legFL != null && legFL.IsStepping) || (legBR != null && legBR.IsStepping);
        bool group2Stepping =(legFR != null && legFR.IsStepping) || (legBL != null && legBL.IsStepping);

        if (group1Stepping || group2Stepping) return;

        if (activeGroup == 0)
        {
            if (legFL.GetHorizontalDistance() > stepDistance || legBR.GetHorizontalDistance() > stepDistance)
            {
                legFL.ExecuteStep();
                legBR.ExecuteStep();
                activeGroup = 1;
            }
        }
        else if (activeGroup == 1)
        {
            if (legFR.GetHorizontalDistance() > stepDistance || legBL.GetHorizontalDistance() > stepDistance)
            {
                legFR.ExecuteStep();
                legBL.ExecuteStep();
                activeGroup = 0;
            }
        }
    }
}
using UnityEngine;

public class SpiderGaitManager8 : MonoBehaviour
{
    [Header("Grup A (Tetrapod Silang 1)")]
    public SpiderLeg[] groupA; // L_First, R_Second, L_Third, R_Fourth

    [Header("Grup B (Tetrapod Silang 2)")]
    public SpiderLeg[] groupB; // R_First, L_Second, R_Third, L_Fourth

    [Header("Sensitivitas Langkah")]
    public float stepDistance = 0.75f;

    private int activeGroup = 0;
    private Vector3 lastBodyPos;
    private Vector3 currentVelocity;

    void Start()
    {
        lastBodyPos = transform.position;
    }

    void Update()
    {
        // Hitung arah gerak badan nyata per frame
        Vector3 frameMove = transform.position - lastBodyPos;
        frameMove.y = 0f;
        if (Time.deltaTime > 0f)
            currentVelocity = frameMove / Time.deltaTime;
        lastBodyPos = transform.position;

        Vector3 moveDirection = currentVelocity.normalized;

        // Cek apakah ada kaki yang masih melayang
        bool isGroupAStepping = false;
        if (groupA != null)
        {
            for (int i = 0; i < groupA.Length; i++)
            {
                if (groupA[i] != null && groupA[i].IsStepping)
                {
                    isGroupAStepping = true;
                    break;
                }
            }
        }

        bool isGroupBStepping = false;
        if (groupB != null)
        {
            for (int i = 0; i < groupB.Length; i++)
            {
                if (groupB[i] != null && groupB[i].IsStepping)
                {
                    isGroupBStepping = true;
                    break;
                }
            }
        }

        if (isGroupAStepping || isGroupBStepping) return;

        
        if (activeGroup == 0)
        {
            if (ShouldGroupStep(groupA))
            {
                TriggerGroupStep(groupA, moveDirection);
                activeGroup = 1;
            }
        }
        
        else if (activeGroup == 1)
        {
            if (ShouldGroupStep(groupB))
            {
                TriggerGroupStep(groupB, moveDirection);
                activeGroup = 0;
            }
        }
    }

    bool ShouldGroupStep(SpiderLeg[] group)
    {
        if (group == null) return false;
        for (int i = 0; i < group.Length; i++)
        {
            if (group[i] != null && group[i].GetHorizontalDistance() > stepDistance)
                return true;
        }
        return false;
    }

    void TriggerGroupStep(SpiderLeg[] group, Vector3 moveDir)
    {
        if (group == null) return;
        for (int i = 0; i < group.Length; i++)
        {
            if (group[i] != null)
                group[i].ExecuteStep(moveDir);
        }
    }
}
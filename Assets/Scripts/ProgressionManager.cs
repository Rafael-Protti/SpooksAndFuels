using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public Transform wagons;
    public static ProgressionManager pg;
    int step = 1;

    public void NextStep()
    {
        if (step == 1) FirstStep();
        
        step++;
    }

    void FirstStep()
    {
        wagons.transform.gameObject.SetActive(true);
    }
}

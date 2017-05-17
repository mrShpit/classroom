using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class StudentData : MonoBehaviour
{
    public int currentStress;
    public int maxStress;
    public int[] discipLevels;
    public int unspentSkillPoints
    {
        get
        {
            int usedSkillPoint = 0;
            foreach (int discLevel in discipLevels)
                usedSkillPoint += discLevel;
            return this.gameObject.GetComponent<CharacterData>().level - usedSkillPoint;
        }
    }

    void Awake()
    {
        discipLevels = new int[FindObjectOfType<SkillController>().allDisciplines.Count]; //Сменить загрузку навыков
    }

}

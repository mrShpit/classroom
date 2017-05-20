using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class StudentData : MonoBehaviour
{
    public int currentStress;
    public Answer currentAnswer;
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

    public float AverageSubjectSkill(List<Discipline.subject> neededSubjects) //Out of ten
    {
        List<Discipline> allDisciplines = FindObjectOfType<SkillController>().allDisciplines;
        float average = 0;
        foreach(Discipline.subject subject in neededSubjects)
        {
            for(int i = 0; i < discipLevels.Length; i++)
            {
                if(allDisciplines[i].Subject.ToString() == subject.ToString())
                {
                    average += discipLevels[i] * 10 / allDisciplines[i].maxLevel;
                }
            }
        }

        average /= neededSubjects.Count;
        return average;
    }

}

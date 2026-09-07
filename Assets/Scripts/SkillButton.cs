using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public int skillNumber = 1;
    public Slider skillSlider;
    private void Update()
    {
        if(skillNumber ==1)
        {
            if(!Player.Instance.canSkill1)
            {
                skillSlider.value = Player.Instance.skill1Time/Player.Instance.skill1CD;
            }
        }
        else if(skillNumber == 2)
        {
            if (!Player.Instance.canSkill2)
            {
                skillSlider.value = Player.Instance.skill2Time / Player.Instance.skill2CD;
            }
        }
        else if(skillNumber == 3)
        {
            if (!Player.Instance.canSkill3)
            {
                skillSlider.value = Player.Instance.skill3Time / Player.Instance.skill3CD;
            }
        }
        else if(skillNumber == 4)
        {
            if (!Player.Instance.canSkill4)
            {
                skillSlider.value = Player.Instance.skill4Time / Player.Instance.skill4CD;
            }
        }
    }
}

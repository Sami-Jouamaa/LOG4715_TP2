using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlotUI : MonoBehaviour
{
    [SerializeField] Image Icon;
    [SerializeField] Image CooldownOverlay;
    [SerializeField] TMP_Text ManaCost;
    [SerializeField] TMP_Text InputKey;


    PlayerControler player;
    ISpell spell;

    public void Initialize(ISpell spell, PlayerControler player)
    {
        this.spell = spell;
        this.player = player;

        Icon.sprite = spell.Icon;
        ManaCost.text = spell.ManaCostValue.ToString();
        InputKey.text = spell.Key.ToString();

        CooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        CooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
        CooldownOverlay.fillClockwise = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (spell == null || player == null) return;

        CooldownOverlay.fillAmount = spell.CooldownProgress;
        CooldownOverlay.gameObject.SetActive(spell.CooldownProgress > 0f);

        Icon.color = player.Current_Mana >= spell.ManaCostValue ? Color.white : Color.grey;
    }
}

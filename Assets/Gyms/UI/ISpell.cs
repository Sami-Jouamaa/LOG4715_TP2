using UnityEngine;

public interface ISpell
{
    Sprite Icon { get; }
    int ManaCostValue { get; }
    KeyCode Key { get; }
    float CooldownProgress { get; }
}
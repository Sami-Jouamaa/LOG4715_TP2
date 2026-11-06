using System.Linq;
using UnityEngine;

public class SpellBarUI : MonoBehaviour
{
    [SerializeField] PlayerControler player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ISpell[] spells = player.GetComponents<MonoBehaviour>().OfType<ISpell>().ToArray();
        SpellSlotUI[] slots = GetComponentsInChildren<SpellSlotUI>();

        for (int i = 0; i < Mathf.Min(spells.Length, slots.Length); i++)
        {
            slots[i].Initialize(spells[i], player);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

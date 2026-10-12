using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManaController : MonoBehaviour
{
    [SerializeField] private PlayerControler player;
    [SerializeField] private Image chunkPrefab;
    [SerializeField] private Transform chunkContainer;
    [SerializeField] private Color fullColour = Color.cyan;
    [SerializeField] private Color emptyColour = Color.grey;

    private readonly List<Image> chunks = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateManaBar();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateManaBar();
    }

    void UpdateManaBar()
    {
        int maxMana = player.Max_Mana;
        int currentMana = player.Current_Mana;

        while (chunks.Count < maxMana)
        {
            chunks.Add(Instantiate(chunkPrefab, chunkContainer));
        }

        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].color = i < currentMana ? fullColour : emptyColour;
        }
    }
}

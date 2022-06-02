using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUpdater : MonoBehaviour
{
    public List<TextMeshProUGUI> texts;

    void Update()
    {
        if (texts == null) return;
        int maxLives = 8;
        texts[0].text = MapManager.Instance.pathCost.ToString();
        texts[1].text = MapManager.Instance.totalCost.ToString();
        texts[2].text = MapManager.Instance.currentFightCost.ToString();
        texts[3].text = MapManager.Instance.geneticCost.ToString();
        texts[4].text = (maxLives - MapManager.Instance.charactersEnergy[0]).ToString();
        texts[5].text = (maxLives - MapManager.Instance.charactersEnergy[1]).ToString();
        texts[6].text = (maxLives - MapManager.Instance.charactersEnergy[2]).ToString();
        texts[7].text = (maxLives - MapManager.Instance.charactersEnergy[3]).ToString();
        texts[8].text = (maxLives - MapManager.Instance.charactersEnergy[4]).ToString();
        texts[9].text = (maxLives - MapManager.Instance.charactersEnergy[5]).ToString();
        texts[10].text = (maxLives - MapManager.Instance.charactersEnergy[6]).ToString();
    }
}

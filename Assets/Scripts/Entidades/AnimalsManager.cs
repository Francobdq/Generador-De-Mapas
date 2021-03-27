using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalsManager : MonoBehaviour
{
    [SerializeField] GameObject lobo;
    [SerializeField] GameObject conejo;

    [SerializeField] int cant;

    public void InstanciarAnimales(ref List<Vector2> posHabitables)
    {
        for (int i = 0; i < cant; i++)
        {
            if (posHabitables.Count > 0)
            {
                Vector2 aux = posHabitables[Random.Range(0, posHabitables.Count)];
                aux.x += Random.Range(-0.49f, 0.49f);
                aux.y += Random.Range(-0.49f, 0.49f);
                Instantiate(conejo, aux, Quaternion.identity);
            }
            else
                Debug.Log("Es 0");
        }
        for (int i = 0; i < cant / 6; i++)
        {
            if (posHabitables.Count > 0)
            {
                Vector2 aux = posHabitables[Random.Range(0, posHabitables.Count)];
                aux.x += Random.Range(-0.49f, 0.49f);
                aux.y += Random.Range(-0.49f, 0.49f);
                Instantiate(lobo, aux, Quaternion.identity);
            }
            else
                Debug.Log("Es 0");
        }
    }
}

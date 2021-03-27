using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public bool hervivoro; // si es hervivoro o no


    Carnivoro carn; // si es carnivoro agarra esta clase 
    Hervivoro herb; // si es hervivoro agarra esta clase 

    [SerializeField]List<Carnivoro> posiblePareja;
    [SerializeField]List<Hervivoro> posibleParejaHerb;

    List<Hervivoro> hervivorosCercanos;
    List<Carnivoro> carnivorosCercanos;

    private void Start()
    {
        if (hervivoro)
        {
            herb = GetComponentInParent<Hervivoro>();
           
        }
        else
        {
            carn = GetComponentInParent<Carnivoro>();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Vision"))
            return;


        if (hervivoro)
        {
            if (collision.CompareTag("Carnivoro"))
            {
                carnivorosCercanos.Add(collision.GetComponent<Carnivoro>());
            }
            else
            {
                Hervivoro otro = collision.GetComponent<Hervivoro>();
                if (otro != null)
                {
                    if(herb == null)
                        herb = GetComponentInParent<Hervivoro>();

                    if (otro.nombreCriatura.Equals(herb.nombreCriatura))
                    {
                        posibleParejaHerb.Add(otro);
                    }
                        
                }
            }
        }
        else
        {
            if (collision.CompareTag("Hervivoro"))
            {
                hervivorosCercanos.Add(collision.GetComponent<Hervivoro>());
            }
            else
            {
                Carnivoro otro = collision.GetComponent<Carnivoro>();
                if (otro != null)
                {
                    if(carn == null)
                        carn = GetComponentInParent<Carnivoro>();

                    if (otro.nombreCriatura == carn.nombreCriatura)
                        posiblePareja.Add(otro);
                }
            }
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (hervivoro)
        {
            if (collision.CompareTag("Carnivoro"))
            {
                carnivorosCercanos.Remove(collision.GetComponent<Carnivoro>());
            }
            else
            {
                Hervivoro otro = collision.GetComponent<Hervivoro>();

                if (otro != null)
                {
                    if (herb == null)
                        herb = GetComponentInParent<Hervivoro>();

                    if (otro.nombreCriatura == herb.nombreCriatura)
                        posibleParejaHerb.Remove(otro);
                }
            }
        }
        else
        {
            if (collision.CompareTag("Hervivoro"))
            {
                hervivorosCercanos.Remove(collision.GetComponent<Hervivoro>());
            }
            else
            {
                Carnivoro otro = collision.GetComponent<Carnivoro>();
                if (otro != null)
                {
                    if (carn == null)
                        carn = GetComponentInParent<Carnivoro>();

                    if (otro.nombreCriatura == carn.nombreCriatura)
                        posiblePareja.Remove(otro);
                }
            }
        }
        
    }


    public void InicializarListas(ref List<Hervivoro> hervivorosCercanos, ref List<Carnivoro> posiblePareja)
    {
        this.hervivorosCercanos = hervivorosCercanos;
        this.posiblePareja = posiblePareja;
    }

    public void InicializarListas(ref List<Carnivoro> carnivorosCercanos, ref List<Hervivoro> posibleParejaHerb)
    {
        this.carnivorosCercanos = carnivorosCercanos;
        this.posibleParejaHerb = posibleParejaHerb;
    }
}

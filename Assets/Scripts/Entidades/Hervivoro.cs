using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hervivoro : Animal
{
    [SerializeField] Vision vision;
    public string nombreCriatura; // visto desde vision

    List<Hervivoro> posiblePareja;
    List<Carnivoro> carnivorosCercanos;

    protected override void Awake()
    {
        base.Awake();
        carnivorosCercanos = new List<Carnivoro>();
        posiblePareja = new List<Hervivoro>();
        vision.InicializarListas(ref carnivorosCercanos, ref posiblePareja);
    }

    protected override void Comer()
    {
        base.Comer();   
    }



    // devuelve la pareja mas cercana
    Hervivoro ParejaMasCercana()
    {
        Hervivoro salida = null;
        float distMin;
        if (posiblePareja.Count > 0)
        {
            distMin = Vector3.Distance(transform.position, posiblePareja[0].transform.position);
            salida = posiblePareja[0];
            for (int i = 0; i < posiblePareja.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, posiblePareja[i].transform.position);
                if (dist < distMin)
                {
                    salida = posiblePareja[0];
                }
            }
        }
        return salida;
    }


    protected override void Reproducirse()
    {
        base.Reproducirse();
        Hervivoro masCercano = ParejaMasCercana();
        if (masCercano == null)
        {
            MovimientoAleatorio();
            return;
        }
            
        
        posAMoverse = masCercano.transform.position;
        if (Vector3.Distance(transform.position, masCercano.transform.position) < 0.25f)
        {
            if (masCercano.puedeReproducirse)
            {
                SeReprodujo();
                masCercano.SeReprodujo();
                Instantiate(gameObject, transform.position, transform.rotation);
            }
        }
    }

    protected override void BuscarComida()
    {
        base.BuscarComida();

        if (posActual.normal == 1 && posActual.Pasto()) // si estoy donde hay comida
        {
            Comer();
        }
        else
        {
            BuscarPasto();
        }
    }


}

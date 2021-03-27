using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carnivoro : Animal
{
    Collider2D cuerpo;
    [SerializeField] Vision vision;
    public string nombreCriatura; // visto desde vision

    List<Carnivoro> posiblePareja;
    List<Hervivoro> hervivorosCercanos; // lo lee la vision 

    protected override void Awake()
    {
        base.Awake();
        cuerpo = GetComponent<Collider2D>();
        hervivorosCercanos = new List<Hervivoro>();
        posiblePareja = new List<Carnivoro>();
        vision.InicializarListas(ref hervivorosCercanos, ref posiblePareja);
    }

    protected override void Comer()
    {
        base.Comer();

    }


    protected override void Reproducirse()
    {
        base.Reproducirse();
        Carnivoro masCercano = ParejaMasCercana();
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

    // Devuelve el hervivoro mas cercano y nulo si no hay
    Hervivoro MasCercano()
    {
        Hervivoro salida = null;
        float distMin;
        if (hervivorosCercanos.Count > 0)
        {
            distMin = Vector3.Distance(transform.position, hervivorosCercanos[0].transform.position);
            salida = hervivorosCercanos[0];
            for (int i = 0; i < hervivorosCercanos.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, hervivorosCercanos[i].transform.position);
                if(dist < distMin)
                {
                    salida = hervivorosCercanos[0];
                }
            }
        }
            
        

        return salida;
    }

    // devuelve la pareja mas cercana
    Carnivoro ParejaMasCercana()
    {
        Carnivoro salida = null;
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


    protected override void BuscarComida()
    {
        base.BuscarComida();

        float x = Mathf.Round(transform.position.x);
        float y = Mathf.Round(transform.position.y);

        Hervivoro masCercano = MasCercano();
        
        if (masCercano != null)
        {
            if (Vector2.Distance(transform.position, masCercano.transform.position) < 0.25) // si estoy donde hay comida
            {
                masCercano.Morir();
                Comer();
            }
            else
            {
                posAMoverse = MasCercano().transform.position;
            }

            
        }
        else
        {
            BuscarPasto(); // mas probabilidades de encontrar animales
        }
            
        
    }
}

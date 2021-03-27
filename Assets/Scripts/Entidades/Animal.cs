using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Animal : MonoBehaviour
{
    public float velocidad;
    public float velFaltaComida;
    public int iteracionesParaReproducirse;
    public Sprite imagen;

    [SerializeField] protected float comida = 100;
    protected float velActual;


    protected GameController gameController;
    protected BoardManager board;


    protected bool seEstaMoviendo = false;
    [SerializeField] protected Vector2 posAMoverse;
    [SerializeField] protected Tile posActual;



    // cuando se alcanza este numero de comida, se comerá de nuevo (se aleatoriza vez que se come
    protected int comerRandom;
    [SerializeField]protected int contIteracionesParaReproducirse;
    [HideInInspector] public bool puedeReproducirse = false; // si puede o no reproducirse

    protected virtual void Awake()
    {
        GameObject Game_Manager = GameObject.FindGameObjectWithTag("GameController");
        gameController = Game_Manager.GetComponent<GameController>();
        board = Game_Manager.GetComponent<BoardManager>();
        velActual = velocidad;
        contIteracionesParaReproducirse = iteracionesParaReproducirse;
        StartCoroutine(falsoUpdate());
    }

    public void Morir()
    {
        Debug.Log("muerto");
        SpriteRenderer spriteRen = GetComponent<SpriteRenderer>();
        spriteRen.color = Color.red;
        StopAllCoroutines();
        GetComponent<Collider2D>().enabled = false;
    }

    protected virtual void Comer()
    {
        comerRandom = Random.Range(50, 80); // se aleatoriza para que todos los animales no coman al mismo tiempo
        comida = 100;
    }

    protected virtual void Reproducirse()
    {
        velActual = velocidad;
    }

    // lo activa al reproducirse para reiniciar
    public void SeReprodujo()
    {
        contIteracionesParaReproducirse = iteracionesParaReproducirse; // reinicia contador
        puedeReproducirse = false;
    }

    protected virtual void BuscarComida()
    {
        velActual = velFaltaComida;
        float x = Mathf.Round(transform.position.x);
        float y = Mathf.Round(transform.position.y);
        posActual = gameController.getTile((int)Mathf.Round(x), (int)Mathf.Round(y));

    }


    // Devuelve los tiles de alrededor
    protected Tile[] Alrededor()
    {
        float x = Mathf.Round(transform.position.x);
        float y = Mathf.Round(transform.position.y);
        posActual = gameController.getTile((int)Mathf.Round(x), (int)Mathf.Round(y));

        Tile posArriba = gameController.getTile((int)Mathf.Round(x), (int)Mathf.Round(y + 1));
        Tile posAbajo = gameController.getTile((int)Mathf.Round(x), (int)Mathf.Round(y - 1));
        Tile posIzq = gameController.getTile((int)Mathf.Round(x - 1), (int)Mathf.Round(y));
        Tile posDer = gameController.getTile((int)Mathf.Round(x + 1), (int)Mathf.Round(y));

        Tile[] tiles = { posArriba, posAbajo, posIzq, posDer };

        return tiles;
    }

    protected void MovimientoAleatorio()
    {
        velActual = velocidad;
        Tile[] tiles = Alrededor();
        int[] orden = new int[4];
        for(int i = 0; i < 4; i++)
        {
            orden[i] = Random.Range(0, 4);
        }
        

        if (tiles[orden[0]].normal == 1)
            posAMoverse = tiles[orden[0]].transform.position;
        else
            if(tiles[orden[1]].normal == 1)
                posAMoverse = tiles[orden[1]].transform.position;
            else
                if (tiles[orden[2]].normal == 1)
                    posAMoverse = tiles[orden[2]].transform.position;
                else
                    if (tiles[orden[3]].normal == 1)
                        posAMoverse = tiles[orden[3]].transform.position;

        // Asi el mov no es tan lineal
        posAMoverse.x += Random.Range(-0.49f, 0.49f);
        posAMoverse.y += Random.Range(-0.49f, 0.49f);
    }

    protected IEnumerator Moverse()
    {
        seEstaMoviendo = true;
        while (Vector3.Distance(posAMoverse, transform.position) > 0.001f)
        {
            float dist = Vector2.Distance(transform.position, posAMoverse);
            
            transform.position = Vector2.Lerp(transform.position, posAMoverse, velActual / dist); // velocidad / distancia me da el porcentaje de mov constante
            yield return new WaitForSeconds(velActual * Time.deltaTime);
        }
        transform.position = posAMoverse; // para dejarlo fijo
        seEstaMoviendo = false;
    }

    protected void BuscarTempEstable(Tile[] tiles)
    {
        Tile posArriba = tiles[0];
        Tile posAbajo = tiles[1];
        Tile posIzq = tiles[2];
        Tile posDer = tiles[3];


        // Si estoy donde hace frio, busco calor (para intentar encontrar estabilidad)
        if (posActual.temp < board.nieveHasta)
        {
            if (posArriba.temp > posActual.temp && posArriba.normal == 1)
                posAMoverse = posArriba.transform.position;
            else
                if (posAbajo.temp > posActual.temp && posAbajo.normal == 1)
                    posAMoverse = posAbajo.transform.position;
                else
                    if (posIzq.temp > posActual.temp && posIzq.normal == 1)
                        posAMoverse = posIzq.transform.position;
                    else
                        if (posDer.temp > posActual.temp && posDer.normal == 1)
                            posAMoverse = posDer.transform.position;
                        else
                            MovimientoAleatorio();
        }
        else
        {
            // Si estoy donde hace calor, busco el frio (para intentar encontrar estabilidad)
            if (posActual.temp > board.desiertoCalienteApartir)
            {
                if (posArriba.temp < posActual.temp && posArriba.normal == 1)
                    posAMoverse = posArriba.transform.position;
                else
                    if (posAbajo.temp < posActual.temp && posAbajo.normal == 1)
                        posAMoverse = posAbajo.transform.position;
                    else
                        if (posIzq.temp < posActual.temp && posIzq.normal == 1)
                            posAMoverse = posIzq.transform.position;
                        else
                            if (posDer.temp < posActual.temp && posDer.normal == 1)
                                posAMoverse = posDer.transform.position;
                            else
                                MovimientoAleatorio();
            }
            else // Si estoy estable, me muevo aleatorio (en teoria nunca llegaría aca porque esta intstruccion es buscada desde BuscarPasto y ya esto sería pasto. (al menos en temp)
            {
                MovimientoAleatorio();
            }
        }
        
    }
    protected void BuscarPasto()
    {
        Tile[] tiles = Alrededor();

        Tile posArriba = tiles[0];
        Tile posAbajo = tiles[1];
        Tile posIzq = tiles[2];
        Tile posDer = tiles[3];

        if (posArriba.Pasto() && posArriba.normal == 1)
            posAMoverse = posArriba.transform.position;
        else
            if (posAbajo.Pasto() && posAbajo.normal == 1)
                posAMoverse = posAbajo.transform.position;
            else
                if (posIzq.Pasto() && posIzq.normal == 1)
                    posAMoverse = posIzq.transform.position;
                else
                    if (posDer.Pasto() && posDer.normal == 1)
                        posAMoverse = posDer.transform.position;
                    else
                        BuscarTempEstable(tiles);

        posAMoverse.x += Random.Range(-0.5f, 0.5f);
        posAMoverse.y += Random.Range(-0.5f, 0.5f);
    }

    protected IEnumerator falsoUpdate()
    {
        while (true)
        {
            if (!seEstaMoviendo)
            {
                contIteracionesParaReproducirse--;
                comida--;
                if (comida < comerRandom)
                {
                    if (comida <= 0)
                        Morir();
                    else
                        BuscarComida();
                }
                else
                {
                    if(contIteracionesParaReproducirse <= 0)
                    {
                        puedeReproducirse = true;
                        Reproducirse();
                    }
                    else
                        MovimientoAleatorio();
                }


                StartCoroutine(Moverse());
            }
            yield return new WaitForSeconds(comida/100); // cuanta menos comida tenga m�s rapido deberia actualizar su movimiento
        }
    }
}

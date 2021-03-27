using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    
    // Dimensiones
    [SerializeField] int tamanioMapa;

    [Range(0, 1)] public float aguaMap; // la cantidad de agua que posee el mapa (apartir de que altura se detecta como agua)
    [Range(0, 1)] public float montaniaMap;// la cantidad de montania que posee el mapa (apartir de que altura se detecta como montania)
    [Range(1, 10)] public float zoom;// zoom del terreno (mas distribucion o menos)
    public Vector2 offset; // para aleatorizar el terreno
    [Range(0, 2)] public float polos; // que tanto afectan los polos a la temperatura global
                  public bool poloAlNorte; // a donde está el polo
    [Range(0, 1)] public float calor; // max calor que hay en el mapa
    [Range(0, 1)] public float frio; // min frio que hay en el mapa
    [Range(0, 1)] public float indiceDeAumento; // que tanto afectan los valores de frio y calor
    [Range(1, 10)] public float zoomTemp; // distribucion de las temperaturas
    public Vector2 offsetTemp; // para aleatorizar las temperaturas
    [Range(0, 1)] public float minArbol; // la temperatura minima para que haya bosques
    [Range(0, 1)] public float maxArbol; // la temperatura max para que haya bosques
    [Range(0, 1)] public float minMinerales; // la altura min dentro del "noise" de recursos naturales para generar minerales
    [Range(0, 1)] public float maxMinerales; // la altura max dentro del "noise" de recursos naturales para generar minerales
    [Range(1, 100)] public float zoomRecursosNaturales; // distribucion de los recursos naturales
    public Vector2 offsetRecursosNaturales; // para aleatorizar los recursos naturales

    
    // estos valores son los acomodados de aguaMap y montaniaMap (ya que estos ultimos son valores entre 0 y 1 y se necesitan otros valores)
    float agua;
    float montania;


    // el mapa
    Tile[,] mapa;

    
    private Transform boardHolder; // donde se contiene el mapa
    public List<Vector2> posHabitables = new List<Vector2>(); // las posiciones libres, construibles y habitables (para generar las primeras ciudades
    private List<Vector2> positionesLibres = new List<Vector2>(); // las posiciones libres y construibles
    private List<Vector2> montaniasYRiosList = new List<Vector2>(); // las posiciones donde hay montania


    public GameObject Tile; 


    //---------------------------------
    // Constantes
    readonly float aguaBorde = 0.4f; // para regular los bordes
    readonly public float bordeX = 0.45f; // que tanto afecta el borde en x
    readonly public float bordeY = 0.6f; // que tanto afecta el borde en y (estos 2 valores se usan para que no quede un continente circular y parecer mas organico)


    public readonly float desiertoFrioHasta = 0.1f; // hasta que temperatura es un desierto frio
    public readonly float desiertoCalienteApartir = 0.75f; // apartir de que temperatura es un desierto caliente
    public readonly float nieveHasta = 0.3f; // hasta que temperatura hay nieve

    
    //-----------------------------------------------------



    private void Start()
    {
        NORMAL = true;
        StartCoroutine(FalsoUpdate());
    }

    //-----------------------------------------------------

    // Inicializa las listas y agrega las posiciones correspondientes a cada valor
    void InitializeList()
    {
        positionesLibres.Clear();
        posHabitables.Clear();
        montaniasYRiosList.Clear();
        for (int x = 1; x < tamanioMapa - 1; x++)
        {
            for (int y = 1; y < tamanioMapa - 1; y++)
            {
                if (mapa[x, y].normal == 1) // si se puede construir
                {
                    positionesLibres.Add(new Vector2(x, y));
                    if(mapa[x,y].temp > desiertoFrioHasta && mapa[x,y].temp < desiertoCalienteApartir)
                        posHabitables.Add(new Vector2(x, y));
                }
                else
                    if (mapa[x, y].normal == 2)
                        montaniasYRiosList.Add(new Vector2(x, y));
            }
        }
    }

    //-----------------------------------------------------

    // Devuelve una posicion random
    Vector2 RandomPosition(List<Vector2> lista)
    { 
        int randomIndex = Random.Range(0, lista.Count); // aleatorio entre 0 y el total de elementos de gridPositions
        Vector2 randomPosition = lista[randomIndex];  // obtenemos el valor dentro de la posicion
        lista.RemoveAt(randomIndex); // remueve el elemento de la lista y entre parentesis la posicion que se quiere eliminar
        return randomPosition;
    }

    //-----------------------------------------------------


    // crea e inicializa el tablero
    void BoardSetup(ref Tile[,] mapa)
    {
        boardHolder = new GameObject("Mapa").transform;
        
        // se recorre todo el mapa (mas el borde)
        for (int x = -1; x < tamanioMapa + 1; x++)
        {
            for (int y = -1; y < tamanioMapa+ 1; y++)
            {
                GameObject instancia = Instantiate(Tile, new Vector2(x, y), Quaternion.identity);

                if ((x > -1 && y > -1) && (x < tamanioMapa && y < tamanioMapa)) // si no es un borde
                {
                    mapa[x, y] = instancia.GetComponent<Tile>();
                    mapa[x, y].altura = Mathf.PerlinNoise((float)x / tamanioMapa * zoom + offset.x, (float)y / tamanioMapa * zoom + offset.y); // crea la altura en base  a un mapa de ruido
                    mapa[x, y].altura = SuavizarBordes(x, y); // suaviza bordes para crear el efecto de continente (isla)
                    mapa[x, y].normal = RedondearConComa(mapa[x,y].altura);  // decide si es agua, montaña o "normal"

                    // Acciones para cada caso
                    switch (mapa[x, y].normal)
                    {
                        case 0:
                            mapa[x, y].colorNormal = Color.blue;
                            break;
                        case 1:
                            mapa[x, y].colorNormal = Color.green;
                            break;
                        case 2:
                            mapa[x, y].colorNormal = new Color(0.5f, 0.3f, 0.07f);
                            break;
                        default:
                            mapa[x, y].colorNormal = Color.cyan;
                            break;
                    }
                }
                else
                { // si es un borde
                    instancia.GetComponent<SpriteRenderer>().color = Color.black;
                }
                
                

                instancia.transform.SetParent(boardHolder);
            }
        }

        RecorrerTodoElMapa();
    }

    //-----------------------------------------------------

    // Segun el valor dado decide si es agua(0), "normal"(1) o montaña(2)
    float RedondearConComa(float valor)
    { 
        if (valor > agua)
        {
            if (valor > montania)
                return 2f;
            else
                return 1f;
        }
        else
            return 0;
            
    }

    // Ajusta la temperatura al rango deseado (entre el valor puesto en la variable "frio" y en la variable "calor"
    float AjustarTemperatura(float valorTemp, float y)
    {
        // dato01 = (X - min)/ (max-min)
        //dato01 * (max-min) = X-min
        //dato01*(max-min)+min = X

        float tamanioMapa = (float)this.tamanioMapa;
        if (!poloAlNorte)
        {
            // primero se normaliza el dato, se acomoda el rango de frioy y calor y luego se le resta la mitad
            valorTemp -= ((((tamanioMapa - y) / tamanioMapa) * (calor - frio) + frio) - (calor-frio)/2)* polos; // normalizo entre 0 y 1 y lo acomodo dentro del rango
        }
        else
        {
            valorTemp -= (((y / tamanioMapa) * (calor - frio) + frio) - (calor - frio) / 2) * polos; // normalizo entre 0 y 1 y lo acomodo dentro del rango
        }


        valorTemp = valorTemp * (calor - frio) + frio;  // convierte el valor normalizado (entre 0 y 1) al rango puesto





        return Mathf.Clamp(valorTemp, frio, calor); // por si se pasa tiene un clamp.
    }


    //-----------------------------------------------------

    // Calcula el color de la temperatura 

    Color ColorTemp(float valorTemp)
    {
        Color salida = Color.black;

        salida.r = valorTemp;
        salida.b = 1- valorTemp;

        

        return salida;
    }

    //-----------------------------------------------------

    // Crea el mapa de temperatura
    void CrearMapaTemp(int x, int y)
    {
        float constante = 1f;

        // se resta la mitad de lo que multiplica para que tenga numeros negativos (hace que el centro sea 0 y luego lo dezplaza)
        mapa[x, y].temp = Mathf.PerlinNoise((float)x / tamanioMapa * zoomTemp + offsetTemp.x, (float)y / tamanioMapa * zoomTemp + offsetTemp.y) * constante - constante/2 + frio + (calor-frio)/2; // lo multiplico por una constante para bajar la intencidad y acomodo su centro a la mitad requerida
                                  
        mapa[x, y].temp = AjustarTemperatura(Mathf.Clamp01(mapa[x, y].temp - mapa[x, y].altura * 0.2f), y);
        mapa[x, y].colorTemp = ColorTemp(mapa[x, y].temp);

    }

    //-----------------------------------------------------

    // Devuelve la posicion con menor altura a su alrededor exceptuando la posicion anterior
    Vector2 menorAltura(int x, int y, int xAnterior, int yAnterior)
    {
        // nunca va a ser cerca de un borde
        float[] valor = { mapa[x - 1, y].altura, mapa[x + 1, y].altura, mapa[x, y + 1].altura, mapa[x, y - 1].altura };
        Vector2[] posiblesSalidas = { new Vector2(x - 1, y), new Vector2(x + 1, y), new Vector2(x, y+1), new Vector2(x, y-1) };
        int min = 0;

        for (int i = 1; i < 4; i++)
        {
            
            if (valor[min] > valor[i] && !((int)posiblesSalidas[i].x == xAnterior || posiblesSalidas[i].y == yAnterior))
            {
                min = (int)i;
            }
        }
        return posiblesSalidas[min];
    }

    //-----------------------------------------------------

    // Busca la posicion objetivo del rio (el punto mas bajo hacia donde hay agua)
    Vector2 buscarPosObjetivo(Vector2 posInicial)
    {
        Vector2 posActual = posInicial; // primero sacar� la posicion de una monta�a aleatoriamente
        int xAnterior = (int)posActual.x;
        int yAnterior = (int)posActual.y;
        do
        {
            int x = (int)posActual.x;
            int y = (int)posActual.y;

            posActual = menorAltura(x, y, xAnterior, yAnterior);

            xAnterior = x;
            yAnterior = y;

        } while (mapa[(int)posActual.x, (int)posActual.y].normal != 0); // mientras que la posicion siguiente no sea agua

        return posActual;
    }

    //---------------------------------------------

    // Devuelve verdadero si ambos son iguales y falso en caso contrario
    bool VectoresIguales(Vector2 a, Vector2 b)
    {
        return (a.x == b.x && a.y == b.y);
    }

    //-----------------------------------------------------

    // calcula la posicion en la que seguirá el río (se utiliza para crear los serpenteos y que el rio no sea recto hacia su objetivo)
    Vector2 CalcularPosSiguiente(int x, int y, int xAnterior, int yAnterior, Vector2 posObjetivo)
    {
        float[] probabilidadDireccion = { -1, -1, -1, -1 }; // arriba, abajo, derecha, izquierda
        Vector2 dist = new Vector2(posObjetivo.x - x, posObjetivo.y - y);
        Vector2[] posiblesSalidas = { new Vector2(x, y+1), new Vector2(x, y-1), new Vector2(x+1, y), new Vector2(x-1, y) };


        if (dist.x > 0)
        {
            probabilidadDireccion[2] = 1f; // si es menor a uno va a pasar pero si es menor a 0.1 pasaria por alguno de los otros
        }
        else
        {
            probabilidadDireccion[3] = 1f;
        }

        if (dist.y > 0)
        {
            probabilidadDireccion[0] = 1f; // si es menor a uno va a pasar pero si es menor a 0.1 pasaria por alguno de los otros
        }
        else
        {
            probabilidadDireccion[1] = 1f;
        }



        if (dist.x == 0)
        {
            probabilidadDireccion[2] = 0.3f;
            probabilidadDireccion[3] = 0.3f;
        }
        else
        {
            if (dist.y == 0)
            {
                probabilidadDireccion[0] = 0.3f;
                probabilidadDireccion[1] = 0.3f;

            }
            else
            {
                if(Mathf.Abs(dist.x) > Mathf.Abs(dist.y))
                {
                    probabilidadDireccion[0] /= 2;
                    probabilidadDireccion[1] /= 2;
                }
                else
                {
                    probabilidadDireccion[2] /= 2;
                    probabilidadDireccion[3] /= 2;
                }
            }
        }
        Vector2 anterior = new Vector2(xAnterior, yAnterior);
        Vector2 menorAlturaPos = menorAltura(x, y, xAnterior, yAnterior);
        for (int i = 0; i < 4; i++)
        {
            if(VectoresIguales(menorAlturaPos, posiblesSalidas[i]))
            {
                probabilidadDireccion[i] = 1f;
            }

            if (VectoresIguales(anterior, posiblesSalidas[i]))
            {
                probabilidadDireccion[i] = -1;
            }
        }

        

        


        float numRandom = Random.Range(0.0001f, 0.99999f);
        List<int> posCumplida = new List<int>();
        for(int i = 0; i < 4; i++)
        {
            if(probabilidadDireccion[i] >= numRandom)
            {
                posCumplida.Add(i);
            }
        }

        return posiblesSalidas[posCumplida[Random.Range(0, posCumplida.Count)]];




    }

    
    //---------------------------------------------

    // Crea los rios del mapa
    
    IEnumerator CrearRios(int cantidad)
    {
        Debug.Log("COMIENZA A CREAR RIOS");
        // recorro la cantidad de rios que quiero crear
        for(int i = 0; i < cantidad; i++)
        {
            // si hay montañas y rios
            if (montaniasYRiosList.Count > 0)
            {
                List<Vector2> posicionesTotales = new List<Vector2>();
                Vector2 posActual = RandomPosition(montaniasYRiosList); // primero sacar� la posicion de una monta�a aleatoriamente

                Vector2 posObjetivo = buscarPosObjetivo(posActual);
                int xAnterior = (int)posActual.x;
                int yAnterior = (int)posActual.y;
                do
                {
                    int x = (int)posActual.x;
                    int y = (int)posActual.y;

                    mapa[x, y].normal = 0;
                    mapa[x, y].colorNormal = new Color(0, 1 - mapa[x, y].altura, 1f, 1f);

                    posicionesTotales.Add(new Vector2(x, y));
                    montaniasYRiosList.Add(new Vector2(x, y));
                    posActual = CalcularPosSiguiente(x, y, xAnterior, yAnterior, posObjetivo);


                    xAnterior = x;
                    yAnterior = y;
                    MostrarNormal();
                    if (mostrarCrearRios)
                        yield return new WaitForSeconds(0.1f);

                } while (mapa[(int)posActual.x, (int)posActual.y].normal != 0); // mientras que la posicion siguiente no sea agua

                for (int j = 0; j < posicionesTotales.Count; j++)
                {
                    int x = (int)posicionesTotales[j].x;
                    int y = (int)posicionesTotales[j].y;
                    mapa[x, y].altura -= 0.01f;
                    if (mapa[x, y].altura < 0)
                        mapa[x, y].altura = 0;
                }
            }
            
        }

        Debug.Log("Finalizo la creacion de rios.");
        InstanciarAnimales();
        GenerarCiudadesIniciales(3);
    }


    //-----------------------------------------------------

    // Que tanto se tiene que disminuir la altura al suavizar los bordes
    float CalcularDisminucionDeAltura(float distancia, float valorAControlar, float altura)
    {


        float disminuirAltura = distancia * aguaBorde;
        if (distancia > valorAControlar)
        {
            disminuirAltura *= distancia + altura; // para eliminar montañas
        }

        return disminuirAltura;
    }


    // Hace que los bordes sean agua creando la sensacion de isla
    float SuavizarBordes(int x, int y) {

        float disminuirAltura;
        Vector2 centro = new Vector2((float)tamanioMapa / 2, (float)tamanioMapa / 2);
        Vector2 pos = new Vector2(x, y);
        float distancia = Vector2.Distance(centro, pos) / ((float)tamanioMapa/2f); // normalizo entre 0 y 1
        float distanciaX = Mathf.Abs(centro.x - pos.x) / ((float)tamanioMapa / 2f); ;
        float distanciaY = Mathf.Abs(centro.y - pos.y) / ((float)tamanioMapa / 2f); ;

        float valorAControlarX = bordeX;
        float valorAControlarY = bordeY;

        disminuirAltura = CalcularDisminucionDeAltura(distanciaX, valorAControlarX, mapa[x,y].altura);
        disminuirAltura += CalcularDisminucionDeAltura(distanciaY, valorAControlarY, mapa[x, y].altura);
        

        float salida = mapa[x, y].altura - disminuirAltura;


        if (salida > 1)
            salida = 1;
        else
            if(salida < 0)
             salida = 0;
        return salida;
    }

    //-----------------------------------------------------


    // Crea los recursos naturales 
   void CrearRecursosNaturales(int x, int y)
    {
        float probabilidadGenerar = Mathf.PerlinNoise((float)x / tamanioMapa * zoomRecursosNaturales + offsetRecursosNaturales.x, (float)y / tamanioMapa * zoomRecursosNaturales + offsetRecursosNaturales.y);
        mapa[x, y].probabilidadBosque = probabilidadGenerar;
        if(mapa[x, y].normal == 1)
        {


            if (Random.Range(0, 1f) < 0.5f)
            {
                // Genera Minerales
                if (probabilidadGenerar > minMinerales && probabilidadGenerar < maxMinerales && Random.Range(0f,1f) < 0.5f)
                {
                    mapa[x, y].colorNormal = Color.gray;
                }
                else
                {
                    // genera bosque
                    if (probabilidadGenerar > 0.50f)
                    {
                        float temp = mapa[x, y].temp;
                        if (temp > minArbol && temp < maxArbol) // si la temperatura está dentro del rango
                        {
                            //Debug.Log("Coordenadas: " + x + " " + y + " temp: " + temp);
                            mapa[x, y].colorNormal = Color.green * 0.6f;
                        }
                        else
                        {
                            float probabilidad = 0;
                            if (temp < minArbol)
                            {
                                if(temp > desiertoFrioHasta)
                                {
                                    // si es menor al minimo
                                    probabilidad = (temp) / minArbol; // 1 - el dato normalizado 
                                    probabilidad *= 0.5f; // probabilidad multiplicado por la maxima probabilidad posible bajo del minimo
                                }
                                        
                            }
                            else
                            {
                                // si no lo es, por ende, es mayor al maximo (por la condicion previa al else)
                                probabilidad = 1 - (temp - maxArbol); // normalizo el dato (se divide por uno entonces no lo pongo)
                                probabilidad *= 0.1f; // probabilidad multiplicado por la maxima probabilidad posible sobre el max
                            }
                                    

                            if (Random.Range(0f, 1f) < probabilidad)
                            {
                                mapa[x, y].colorNormal = Color.green * 0.44f;
                            }
                        }
                    }
                }  
            } 
        }
    }



    //-----------------------------------------------------

    // crea los distintos tipos de biomas
    void CrearBiomas(int x, int y)
    {

        if(mapa[x,y].normal == 1)
        {
            if (mapa[x, y].temp >= desiertoCalienteApartir)
            {
                mapa[x, y].colorNormal = Color.yellow;
            }
            else
            {
                if (mapa[x, y].temp <= desiertoFrioHasta)
                {
                    mapa[x, y].colorNormal = Color.white;
                }
                else
                {
                    if (mapa[x, y].temp <= nieveHasta)
                    {
                        mapa[x, y].colorNormal = Color.white * 0.95f;
                        mapa[x, y].colorNormal.a = 1;
                    }
                }
            }
        }

    }



    //-----------------------------------------------------

    // se fija si esa posicion está a una cierta distancia de otra ciudad
    bool CumpleDistanciaMin(Vector2 pos, List<Vector2> ciudadesCreadas)
    {
        for (int i = 0; i < ciudadesCreadas.Count; i++)
        {

            if (Vector2.Distance(pos, ciudadesCreadas[i]) <= 10f)
                return false;

        }
        return true;
    }

    // Asigna los colores a cada ciudad
    void AsignarColores(ref Color[] colores)
    {
        for(int i = 0; i < colores.Length; i++)
        {
            switch (i)
            {
                case 0:
                    colores[i] = Color.red;
                    break;
                case 1:
                    colores[i] = Color.blue * 0.7f;
                    colores[i].a = 1;
                    break;
                case 2:
                    colores[i] = Color.cyan ;
                    colores[i].a = 1;
                    break;
                case 3:
                    colores[i] = Color.black;
                    break;
                case 4:
                    colores[i] = Color.cyan * 0.4f;
                    break;
                case 5:
                    colores[i] = Color.magenta;
                    break;
            }
        }
    }

    // Crea las primeras ciudades 
    void GenerarCiudadesIniciales(int cant)
    {
        List<Vector2> ciudadesCreadas = new List<Vector2>();
        Color[] siguienteColor = new Color[cant];
        AsignarColores(ref siguienteColor);
        Debug.Log("Creando ciudades...");
        if (posHabitables.Count >= cant)
        {
            int i = 0;
            while( i < cant && posHabitables.Count > 0)
            {
                int index = Random.Range(0, posHabitables.Count);
                Vector2 pos = posHabitables[index];
                posHabitables.RemoveAt(index);
                if(CumpleDistanciaMin(pos, ciudadesCreadas))
                {
                    Debug.Log("Ciudad  " + (i + 1) + " creada en pos x:" + pos.x + " y:" + pos.y);
                    mapa[(int)pos.x, (int)pos.y].colorNormal = siguienteColor[i];
                    ciudadesCreadas.Add(pos);
                    i++;
                }
            }
        }
        MostrarNormal();
        Debug.Log("Se terminaron de crear las ciudades");
    }

    //-----------------------------------------------------
    void InstanciarAnimales()
    {
        GetComponent<AnimalsManager>().InstanciarAnimales(ref posHabitables);
    }

    //-----------------------------------------------------

    // Recorre el mapa y ejecuta acciones en cada tile
    void RecorrerTodoElMapa()
    {
        for(int x = 0; x < tamanioMapa; x++)
        {
            for (int y = 0; y < tamanioMapa; y++)
            {
                CrearMapaTemp(x, y);
                CrearBiomas(x,y);
                CrearRecursosNaturales(x, y);
            }
        }
        
        MostrarNormal();
    }



    //-----------------------------------------------------
    // Comienza a crear el mapa
    public void SetupScene(ref Tile[,] mapa, int bordesAgua)
    {
        mapa = new Tile[tamanioMapa, tamanioMapa];
        agua = (aguaMap)/(0.1f+1)+0.1f; // de un valor entre 0 y 1 a un valor entre 0.1 y 1
        montania = montaniaMap * 0.75f; // pasa el valor entre 0 y 1 a un valor entre 0 y 0.75f
        this.mapa = mapa;
        //Random.InitState(seed);
        BoardSetup(ref mapa);
        InitializeList(); // OJO QUE NO QUEDE DESPUES DE LAS CIUDADES 

        //LayoutObjectAtRandom(inWallsTiles, 5, 9); // arreglo wileTiles declarado arriba, entre 5 y 9 muros
        //LayoutObjectAtRandom(foodTiles, 1, 5);
        //LayoutObjectAtRandom(watherTiles, 3, 5);
        //int enemyCount = (int)Mathf.Log(level, 2); // usa el logartimo base 2 con el nivel para la cantidad de enemigos 
        //LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount); // esto instancia varias veces, aparte de ser aleatorio (funcion creada arriba)
        //Instantiate(exit, new Vector2(tamanioMapa - 1, tamanioMapa - 1), Quaternion.identity);

    }

    //-----------------------------------------------------
    // Muestra en pantalla de cada valor


    void MostrarNormal()
    {
        for (int x = 0; x < tamanioMapa; x++)
        {
            for (int y = 0; y < tamanioMapa; y++)
            {
                mapa[x, y].mostrarNormal();
            }
        }
        

    }

    void MostrarTemperatura()
    {
        for (int x = 0; x < tamanioMapa; x++)
        {
            for (int y = 0; y < tamanioMapa; y++)
            {
                mapa[x, y].mostrarTemp();
            }
        }
        
    }

    void MostrarInfluencia()
    {
        for (int x = 0; x < tamanioMapa; x++)
        {
            for (int y = 0; y < tamanioMapa; y++)
            {
                mapa[x, y].mostrarInfluencia();
                mapa[x, y].mostrarAltura();
            }
        }
    }

    void MostrarAltura()
    {
        for (int x = 0; x < tamanioMapa; x++)
        {
            for (int y = 0; y < tamanioMapa; y++)
            {
                mapa[x, y].mostrarAltura();
            }
        }
    }


    void MostrarBosque()
    {
        for (int x = 0; x < tamanioMapa; x++)
        {
            for (int y = 0; y < tamanioMapa; y++)
            {
                mapa[x, y].mostrarBosque();
            }
        }
    }

    //-----------------------------------------------------

    // Devuelve el mapa
    public Tile[,] getMapa()
    {
        return mapa;
    }

    public bool UPDATE = false; // actualiza el mapa
    public bool autoUPDATE;
    public bool NORMAL;
    public bool TEMP;
    public bool ALTURA;
    public bool CREARRIOS;
    public bool mostrarCrearRios;
    public bool BOSQUE;
    
    void ActualizaImagen()
    {
        if (NORMAL)
            MostrarNormal();
        else
            if (TEMP)
                MostrarTemperatura();
            else
                if (ALTURA)
                    MostrarAltura();
                else
                    if (CREARRIOS)
                    {
                        CREARRIOS = false;
                        StartCoroutine(CrearRios(1));
                    }
                    else
                        if(BOSQUE)
                            MostrarBosque();
    }

    IEnumerator FalsoUpdate()
    {
        while (true)
        {
            if ((tamanioMapa <= 200 || UPDATE) && autoUPDATE)
            {
                UPDATE = false;
                if (boardHolder != null)
                {
                    GameObject.Destroy(boardHolder.gameObject);
                    SetupScene(ref mapa, 0);
                }
                ActualizaImagen();


                yield return new WaitForSeconds(0.1f);
            }
            else
                yield return new WaitForSeconds(2f);
        }

       
    }

    private void FixedUpdate()
    {
        if (!autoUPDATE)
        {
            if (UPDATE)
            {
                UPDATE = false;
                if (boardHolder != null)
                {
                    GameObject.Destroy(boardHolder.gameObject);
                    SetupScene(ref mapa, 0);
                }
                MostrarNormal();
            }
            if (NORMAL)
            {
                NORMAL = false;
                MostrarNormal();
            }
            else
            {
                if (TEMP)
                {
                    TEMP = false;
                    MostrarTemperatura();
                }
                else
                {
                    if (ALTURA)
                    {
                        ALTURA = false;
                        MostrarAltura();
                    }
                    else
                    {
                        if (CREARRIOS)
                        {
                            CREARRIOS = false;
                            StartCoroutine(CrearRios(3));
                        }
                        else
                        if (BOSQUE)
                        {
                            BOSQUE = false;
                            MostrarBosque();
                        }
                            
                    }

                }
            }
        }        
           
    }

}

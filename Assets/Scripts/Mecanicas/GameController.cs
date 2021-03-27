using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Valores del mapa
    



    Tile[,] mapa;
    BoardManager boardManager;


    // Start is called before the first frame update
    void Start()
    {
        boardManager = GetComponent<BoardManager>();
        boardManager.SetupScene(ref mapa, 2);
    }



    public Tile getTile(int x, int y)
    {
        return mapa[x, y];
    }

    public Tile getTile(Vector2 pos)
    {
        return mapa[(int)pos.x, (int)pos.y];
    }
}

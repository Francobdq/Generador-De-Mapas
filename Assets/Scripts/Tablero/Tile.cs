using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public float normal;
    public float altura;
    public float temp;
    public float probabilidadBosque;
    public float influencia;





    public Color colorNormal;
    public Color colorTemp;
    public Color colorInfluencia;

    SpriteRenderer spriteRen;
    BoardManager board;


    private void Awake()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        board = GameObject.FindGameObjectWithTag("GameController").GetComponent<BoardManager>();
        mostrarNormal();
    }



    public void mostrarNormal()
    {
        spriteRen.color = colorNormal;
    }

    public void mostrarTemp()
    {
        spriteRen.color = colorTemp;
    }

    public void mostrarInfluencia()
    {
        spriteRen.color = colorInfluencia;
    }
    
    public void mostrarAltura()
    {
        Color colorEscalaGrises = Color.white;
        colorEscalaGrises.r = 1-altura;
        colorEscalaGrises.g = 1-altura;
        colorEscalaGrises.b = 1-altura;
        spriteRen.color = colorEscalaGrises;
    }

    public void mostrarBosque()
    {
        Color colorEscalaGrises = Color.white;
        colorEscalaGrises.r = 1 - probabilidadBosque;
        colorEscalaGrises.g = 1 - probabilidadBosque;
        colorEscalaGrises.b = 1 - probabilidadBosque;
        spriteRen.color = colorEscalaGrises;
    }

    public bool Pasto()
    {
        return (temp > board.nieveHasta && temp < board.desiertoCalienteApartir);
    }

}

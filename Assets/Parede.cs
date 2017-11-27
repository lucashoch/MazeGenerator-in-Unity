using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Parede
{
    public int direcao { get; }//0: cima, 1: direita, 2: baixo, 3: esquerda
    public GameObject gameObject;
    Celula pai { get; }
    Celula vizinho;

    public Parede(GameObject g, Celula p, Celula v = null)
    {
        gameObject = g;
        pai = p;

        switch (g.name) {
            case "top":
                direcao = 0;
                break;
            case "rgt":
                direcao = 1;
                break;
            case "btm":
                direcao = 2;
                break;
            case "lft":
                direcao = 3;
                break;
        }

        vizinho = v;
    }


}
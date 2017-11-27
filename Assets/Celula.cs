using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Celula {

    public int id { get; } // de 0 a comprimento * altura - 1
    public GameObject gameObject { get; }
    public bool visitada;
    public List<Celula> vizinhos { get; }
    public List<Parede> paredes { get; }
    public Celula pai;
    public List<Celula> filhos { get; }

    public Celula(int id, GameObject g) {
        this.id = id;
        visitada = false;
        gameObject = g;

        vizinhos = new List<Celula>();
        filhos = new List<Celula>();

        paredes = new List<Parede>();
        foreach (Transform child in gameObject.transform.GetChild(0)) {
            Parede p = new Parede(child.gameObject, this);
            paredes.Add(p);
        }
        this.pai = pai;
        this.filhos = filhos;
    }

    public void addVizinho(Celula vizinho) {
        //relação de vizinhança é mútua

        vizinhos.Add(vizinho);
        vizinho.vizinhos.Add(this);


        if (vizinhos.Count > 4)
            System.Console.WriteLine("Deu ruim aqui, tio");
    }

    public List<Celula> getVizinhosVisitados() {
        List<Celula> ret = new List<Celula>();
        foreach (Celula v in vizinhos) {
            if (v.visitada)
                ret.Add(v);
        }
        return ret;
    }

    public List<Celula> getVizinhosNaoVisitados() {
        List<Celula> ret = new List<Celula>();
        foreach (Celula v in vizinhos) {
            if (!v.visitada)
                ret.Add(v);
        }
        return ret;
    }
    public bool TemVizinhosNaoVisitados() {
        List<Celula> ret = new List<Celula>();
        foreach (Celula v in vizinhos) {
            if (!v.visitada)
                return true;
        }
        return false;
    }

    public void removeParedesEntre(Celula celFilho) {
        int direcao = id - celFilho.id;

        //filhos.Add(celFilho);
        //celFilho.pai = this;

        switch (direcao) {
            case 1: //o filho está à esquerda
                retiraPareceEsquerda(this, celFilho);
                break;
            case -1: //o filho está à direita
                retiraPareceEsquerda(celFilho, this);
                break;
            default:
                if (direcao > 0) {//o filho está acima
                    retiraPareceCima(this, celFilho);
                    break;
                } else {//o filho está abaixo
                    retiraPareceCima(celFilho, this);
                    break;
                }


        }
    }

    private void retiraPareceEsquerda(Celula celCentral, Celula celEsquerda) {
        Parede esquerda = null;
        Parede direita = null;

        foreach(Parede p in celCentral.paredes) {
            if (p.direcao == 3) {
                esquerda = p;
                break;
            }                
        }
        foreach (Parede p in celEsquerda.paredes) {
            if (p.direcao == 1) {
                direita = p;
                break;
            }
        }

        Object.Destroy(esquerda.gameObject);
        Object.Destroy(direita.gameObject);
        celCentral.paredes.Remove(esquerda);
        celEsquerda.paredes.Remove(direita);
    }

    private void retiraPareceCima(Celula celCentral, Celula celCima) {
        Parede cima = null;
        Parede baixo = null;

        foreach (Parede p in celCentral.paredes) {
            if (p.direcao == 0) {
                cima = p;
                break;
            }
        }
        foreach (Parede p in celCima.paredes) {
            if (p.direcao == 2) {
                baixo = p;
                break;
            }
        }

        Object.Destroy(cima.gameObject);
        Object.Destroy(baixo.gameObject);
        celCentral.paredes.Remove(cima);
        celCima.paredes.Remove(baixo);
    }
}
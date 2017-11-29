using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class mainScript2 : MonoBehaviour {


    public static mainScript2 instance;

    private static int altura;
    private static int comprimento;

    float velocidade = 0f;
    bool instantaneo = false;

    Button btnGera;
    Button btnPausa;
    Button btnLimpa;

    GameObject gridBase;

    public Transform celulaPrefab;

    List<Celula> matriz;

    Celula raiz;

    public void setAltura(int v) {
        altura = v;
        StopCoroutine("DesenhaGrid");
        StartCoroutine("DesenhaGrid");
    }
    public void setComprimento(int v) {
        comprimento = v;
        StopCoroutine("DesenhaGrid");
        StartCoroutine("DesenhaGrid");
    }
    public void setVel(float v) {
        velocidade = v;
    }
    public int getAltura() {
        return altura;
    }
    public void setInstantaneo(bool v) {
        instantaneo = v;
    }
    public int getComprimento() {
        return comprimento;
    }
    public void IniciaGeraMaze(int t) {
        switch (t) {
            case 0:
                StartCoroutine("GeraBuscaEmProfundidade");
                break;
            case 1:
                StartCoroutine("GeraPorPrim");
                break;
            default:
                break;
        }
        
    }

    public void PausaGeraMaze(int t) {
        switch (t) {
            case 0:
                StopCoroutine("GeraBuscaEmProfundidade");
                break;
            case 1:
                StopCoroutine("GeraPorPrim");
                break;
            default:
                break;
        }

    }

    public void Limpa() {
        StopCoroutine("DesenhaGrid");
        StartCoroutine("DesenhaGrid");
    }


    void Start() {
        instance = this;
        altura = 20;
        comprimento = 20;
        btnGera = GameObject.Find("BtnGera").GetComponent<Button>();
        btnPausa = GameObject.Find("BtnPausa").GetComponent<Button>();
        btnLimpa = GameObject.Find("BtnLimpa").GetComponent<Button>();
        gridBase = GameObject.Find("GridBase");



        StartCoroutine("DesenhaGrid");
    }


    void Update() {

    }


    Vector3 instanciaCelula(Vector3 pos, int id, GameObject grid, float tam, float esp) {
        GameObject cel = Instantiate(celulaPrefab.gameObject, grid.transform);
        cel.transform.localPosition = pos;
        cel.transform.localScale = new Vector3(tam, tam, tam);
        cel.name = "Cell " + id;

        matriz.Add(new Celula(id, cel));

        if (id >= comprimento) {
            matriz[id].addVizinho(matriz[id - comprimento]);
        }

        if (id > 0 && id % comprimento != 0) {
            matriz[id].addVizinho(matriz[id - 1]);
        }

        return cel.transform.localPosition;
    }

    public void AtribuiVizinhos() {
        foreach (Celula c in matriz) {
            print(c.id);
        }
    }

    IEnumerator DesenhaGrid() {
        btnGera.interactable = false;
        btnPausa.interactable = false;

        float esp, tam;
        esp = 450f / Mathf.Max(altura, comprimento);
        tam = 45f / Mathf.Max(altura, comprimento);


        GameObject grid = GameObject.Find("Grid");
        Destroy(grid);
        grid = new GameObject("Grid");
        grid.transform.parent = gridBase.transform;
        grid.transform.localPosition = new Vector3(0, 0, 0);
        grid.transform.localScale = new Vector3(1, 1, 1);

        Vector3 pos = new Vector3(0, 0, 0);

        matriz = new List<Celula>();
        int index = 0;

        for (int i = 1; i <= altura; i++) {
            pos = instanciaCelula(pos, index++, grid, tam, esp);
            for (int f = 2; f <= comprimento; f++) {
                pos = instanciaCelula(pos + new Vector3(esp, 0, 0), index++, grid, tam, esp);
            }
            pos = pos + new Vector3(-esp * (comprimento - 1), -esp, 0);

            if (i % 3 == 0)
                yield return null;
        }
        btnGera.interactable = true;
        yield return null;
        raiz = matriz[0];
        matriz = null;
    }




    void setAtualDesceDFS(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;


        atual = antiga;

        atual.visitada = true;


        c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    void setAtualSobeDFS(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        atual = antiga;

        atual.visitada = true;


        c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }


    IEnumerator GeraBuscaEmProfundidade() {

        /*
        Make the initial cell the current cell and mark it as visited
        While there are unvisited cells
            If the current cell has any neighbours which have not been visited
                Choose randomly one of the unvisited neighbours
                Push the current cell to the stack
                Remove the wall between the current cell and the chosen cell
                Make the chosen cell the current cell and mark it as visited
            Else if stack is not empty
                Pop a cell from the stack
                Make it the current cell
        */

        btnPausa.interactable = true;
        btnLimpa.interactable = false;

        raiz.visitada = true;

        Celula atual = raiz;
        int c = 0;
        System.Random r = new System.Random();

        setAtualDesceDFS(ref atual, raiz);
        while (atual.pai != null || atual.TemVizinhosNaoVisitados()) {

            if (atual.TemVizinhosNaoVisitados()) {

                List<Celula> naoVisitados = atual.getVizinhosNaoVisitados();
                Celula vizinhoAleatorio = naoVisitados[r.Next(naoVisitados.Count)];

                vizinhoAleatorio.pai = atual;
                atual.filhos.Add(vizinhoAleatorio);
                atual.removeParedesEntre(vizinhoAleatorio);
                setAtualDesceDFS(ref atual, vizinhoAleatorio); // desce na construção da árvore. A única diferença com Sobe é a cor que pinta


            } else if (atual.pai != null) {
                setAtualSobeDFS(ref atual, atual.pai); //sempre que sobe na árvore, pinta o antigo de Branco, e ele nunca mais é visitado
            }

            if (!instantaneo)
                yield return new WaitForSeconds(velocidade);

            c++;
        }


        Transform fundo = atual.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        btnPausa.interactable = false;
        btnLimpa.interactable = true;
        yield return null;
    }


    List<Parede> setAtualPrim(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        atual = antiga;

        atual.visitada = true;


        c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        List<Parede> ret = new List<Parede>();
        foreach (Parede p in atual.paredes) {
            if (p.vizinho != null)
                ret.Add(p);
        }
        return ret;

    }

    List<Celula> setAtualPrim2(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        atual = antiga;

        atual.visitada = true;


        c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        return atual.vizinhos;

    }
    /*
    IEnumerator GeraPorPrim() {
        /*
            Start with a grid full of walls.
            Pick a cell, mark it as part of the maze. Add the walls of the cell to the wall list.
            While there are walls in the list:
                Pick a random wall from the list. If only one of the two cells that the wall divides is visited, then:
                    Make the wall a passage and mark the unvisited cell as part of the maze.
                    Add the neighboring walls of the cell to the wall list.
                Remove the wall from the list.
         */
    /*

   List<Parede> paredesNaoVisitadas = new List<Parede>();
   Celula atual = raiz;

   paredesNaoVisitadas.AddRange(setAtualPrim(ref atual, raiz));

   System.Random r = new System.Random();
   int c = 0;
   //while (c < 200) {
   while (paredesNaoVisitadas.Count > 0) {
       Parede paredeAleatoria = paredesNaoVisitadas[r.Next(paredesNaoVisitadas.Count)];
       bool condicao;
       condicao = paredeAleatoria.pai.visitada && !paredeAleatoria.vizinho.visitada;
       condicao |= !paredeAleatoria.pai.visitada && paredeAleatoria.vizinho.visitada;
       if (condicao) {

           Celula visitado;
           Celula naoVisitado;
           if (paredeAleatoria.pai.visitada) {
               visitado = paredeAleatoria.pai;
               naoVisitado = paredeAleatoria.vizinho;
           } else {
               visitado = paredeAleatoria.vizinho;
               naoVisitado = paredeAleatoria.pai;
               print("FOI O VIZINHO COM O CASTIÇAL");
           }

           setAtualPrim(ref atual, visitado);
           atual.removeParedesEntre(naoVisitado);                

           foreach(Parede candidata in setAtualPrim(ref atual, naoVisitado)) {
               if (!paredesNaoVisitadas.Contains(candidata)) {
                   paredesNaoVisitadas.Add(candidata);
               }
           }
           if (!instantaneo)
               yield return new WaitForSeconds(velocidade);
       }
       paredesNaoVisitadas.Remove(paredeAleatoria);
       c++;

   }
   Transform fundo = atual.gameObject.transform.GetChild(1);
   fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


   btnPausa.interactable = false;
   btnLimpa.interactable = true;
   yield return null;
}
*/
    IEnumerator GeraPorPrim() {
        /*
            Start with a grid full of walls.
            Pick a cell, mark it as part of the maze. Add the walls of the cell to the wall list.
            While there are walls in the list:
                Pick a random wall from the list. If only one of the two cells that the wall divides is visited, then:
                    Make the wall a passage and mark the unvisited cell as part of the maze.
                    Add the neighboring walls of the cell to the wall list.
                Remove the wall from the list.
         */

        btnPausa.interactable = true;
        btnLimpa.interactable = false;

        List<Celula> vizinhosNaoVisitados = new List<Celula>();
        Celula atual = raiz;

        vizinhosNaoVisitados.AddRange(setAtualPrim2(ref atual, raiz));

        System.Random r = new System.Random();
        int c = 0;
        //while (c < 50) {
        while (vizinhosNaoVisitados.Count > 0) {
            Celula vizinhoAleatorio = vizinhosNaoVisitados[r.Next(vizinhosNaoVisitados.Count)];


            List<Celula> visitados = new List<Celula>();
            foreach (Celula v in vizinhoAleatorio.vizinhos) {
                if (v.visitada)
                    visitados.Add(v);
            }


            if (visitados.Count > 0 && !vizinhoAleatorio.visitada) {

                Celula paiAleatorio = visitados[r.Next(visitados.Count)];

                setAtualPrim(ref atual, paiAleatorio);
                atual.removeParedesEntre(vizinhoAleatorio);

                foreach (Celula candidata in setAtualPrim2(ref atual, vizinhoAleatorio)) {
                    if (!vizinhosNaoVisitados.Contains(candidata)) {
                        vizinhosNaoVisitados.Add(candidata);
                    }
                }
                if (!instantaneo)
                    yield return new WaitForSeconds(velocidade);
            }
            vizinhosNaoVisitados.Remove(vizinhoAleatorio);
            c++;

        }
        Transform fundo = atual.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        btnPausa.interactable = false;
        btnLimpa.interactable = true;
        yield return null;
    }
}



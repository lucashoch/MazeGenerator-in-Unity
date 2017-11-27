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
    public int getComprimento() {
        return comprimento;
    }
    public void IniciaGeraBuscaEmProfundidade() {
        StartCoroutine("GeraBuscaEmProfundidade");
    }
    public void PausaGeraBuscaEmProfundidade() {
        StopCoroutine("GeraBuscaEmProfundidade");
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
        print(raiz.paredes.Count);
        foreach(Parede p in matriz[21].paredes) {
            if(p.vizinho != null)
                print("direcao: " + p.direcao + ", id pai: " + p.pai.id + ", id vizinho: " + p.vizinho.id);
        }
        matriz = null;
    }




    void setAtualDesce(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        //c.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0f, 255f, 130f, 15f);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;


        atual = antiga;

        atual.visitada = true;


        c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    void setAtualSobe(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        //c.gameObject.GetComponent<MeshRenderer>().material.color = new Color(255, 255, 255);
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

        setAtualDesce(ref atual, raiz);
        while (atual.pai != null || atual.TemVizinhosNaoVisitados()) {

            if (atual.TemVizinhosNaoVisitados()) {

                List<Celula> naoVisitados = atual.getVizinhosNaoVisitados();
                Celula vizinhoAleatorio = naoVisitados[r.Next(naoVisitados.Count)];

                vizinhoAleatorio.pai = atual;
                atual.filhos.Add(vizinhoAleatorio);
                atual.removeParedesEntre(vizinhoAleatorio);
                setAtualDesce(ref atual, vizinhoAleatorio); // desce na construção da árvore. A única diferença com Sobe é a cor que pinta


            } else if (atual.pai != null) {
                setAtualSobe(ref atual, atual.pai); //sempre que sobe na árvore, pinta o antigo de Branco, e ele nunca mais é visitado
            }

            
            yield return new WaitForSeconds(velocidade);

            c++;
        }




        btnPausa.interactable = false;
        btnLimpa.interactable = true;
        yield return null;
    }
}

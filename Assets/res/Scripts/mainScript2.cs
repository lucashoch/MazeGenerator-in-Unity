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
    bool paralelo = true;
    bool limpo = false;
    List<int> coroutinesIds = new List<int>();
    int coroutineId = 0;

    Button btnGera;
    Button btnPausa;
    Button btnResolve;

    GameObject gridBase;

    public Transform celulaPrefab;

    List<Celula> matriz;

    Celula raiz;
    Celula fim;

    System.Random r;

    public void setAltura(int v) {
        altura = v;
        StopAllCoroutines();
        StartCoroutine("DesenhaGrid");
    }
    public void setComprimento(int v) {
        comprimento = v;
        StopAllCoroutines();
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
    public void setParalelo(bool v) {
        paralelo = v;
    }
    public int getComprimento() {
        return comprimento;
    }
    public void PausaGeraMaze(int t) {
        StopAllCoroutines();
    }

    public void Limpa() {
        StopCoroutine("DesenhaGrid");
        StartCoroutine("DesenhaGrid");
    }

    public void Resolve() {
        btnResolve.interactable = false;
        StartCoroutine("ResolvePorAEstrela");
    }
    public void IniciaGeraMaze(int i) {

        StopAllCoroutines();
        StartCoroutine(EnumIniciaGeraMaze(i));
    }
    IEnumerator EnumIniciaGeraMaze(int t) {

        if (!limpo) {
            StopCoroutine("DesenhaGrid");
            yield return StartCoroutine("DesenhaGrid");
        }
        limpo = false;
        bool variavel = false;
        switch (t) {
            case 0:
                yield return StartCoroutine("GeraBuscaEmProfundidade");
                break;
            case 1:
                yield return StartCoroutine("GeraPorPrim");
                break;
            case 2:
                yield return StartCoroutine("PreparaDivisaoEConquista", true);
                break;
            case 3:
                variavel = true;
                yield return StartCoroutine("PreparaDivisaoEConquista", false);
                break;
            default:
                break;
        }

        if (variavel) {
            while (coroutinesIds.Count > 0)
                yield return null;
        }

        yield return StartCoroutine("InicioEFim");

    }

    IEnumerator InicioEFim() {
        Transform fundo = raiz.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;

        fundo = fim.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        btnResolve.interactable = true;
        yield return null;
    }

    void Start() {
        instance = this;
        altura = 20;
        comprimento = 20;
        btnGera = GameObject.Find("BtnGera").GetComponent<Button>();
        btnPausa = GameObject.Find("BtnPausa").GetComponent<Button>();
        btnResolve = GameObject.Find("BtnResolve").GetComponent<Button>();
        gridBase = GameObject.Find("GridBase");
        r = new System.Random();



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

    IEnumerator DesenhaGrid() {
        btnPausa.interactable = false;
        btnResolve.interactable = false;

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

        raiz = matriz[0];
        fim = matriz[altura * comprimento - 1];
        matriz = null;
        limpo = true;
        yield return null;
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
        //btnLimpa.interactable = false;

        raiz.visitada = true;

        Celula atual = raiz;

        //System.Random r = new System.Random();

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

        }


        Transform fundo = atual.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        btnPausa.interactable = false;
        //btnLimpa.interactable = true;
        yield return null;
    }

    List<Celula> setAtualPrim(ref Celula atual, Celula antiga) {
        Transform c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;


        atual = antiga;

        atual.visitada = true;


        c = atual.gameObject.transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        return atual.vizinhos;

    }

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
        // btnLimpa.interactable = false;

        List<Celula> vizinhosNaoVisitados = new List<Celula>();
        Celula atual = raiz;

        vizinhosNaoVisitados.AddRange(setAtualPrim(ref atual, raiz));
        foreach (Celula v in vizinhosNaoVisitados) {
            Transform fundoVizinho = v.gameObject.transform.GetChild(1);
            fundoVizinho.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
        if (!instantaneo)
            yield return new WaitForSeconds(velocidade);

        //System.Random r = new System.Random();

        while (vizinhosNaoVisitados.Count > 0) {
            Celula vizinhoAleatorio = vizinhosNaoVisitados[r.Next(vizinhosNaoVisitados.Count)];


            List<Celula> visitados = new List<Celula>();
            foreach (Celula v in vizinhoAleatorio.vizinhos) {
                if (v.visitada)
                    visitados.Add(v);
            }


            if (visitados.Count > 0 && !vizinhoAleatorio.visitada) {

                Celula paiAleatorio = visitados[r.Next(visitados.Count)];

                Transform fundoVizinho = paiAleatorio.gameObject.transform.GetChild(1);
                fundoVizinho.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                if (!instantaneo)
                    yield return new WaitForSeconds(velocidade);
                fundoVizinho = paiAleatorio.gameObject.transform.GetChild(1);
                fundoVizinho.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                setAtualPrim(ref atual, paiAleatorio);
                atual.removeParedesEntre(vizinhoAleatorio);

                foreach (Celula candidata in setAtualPrim(ref atual, vizinhoAleatorio)) {
                    if (!vizinhosNaoVisitados.Contains(candidata) && !candidata.visitada) {
                        fundoVizinho = candidata.gameObject.transform.GetChild(1);
                        fundoVizinho.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                        vizinhosNaoVisitados.Add(candidata);
                    }
                }
                if (!instantaneo)
                    yield return new WaitForSeconds(velocidade);
            }
            Transform fundoAleatorio = vizinhoAleatorio.gameObject.transform.GetChild(1);
            fundoAleatorio.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            vizinhosNaoVisitados.Remove(vizinhoAleatorio);
        }
        Transform fundo = atual.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

        btnPausa.interactable = false;
        //btnLimpa.interactable = true;
        yield return null;
    }

    IEnumerator PreparaDivisaoEConquista(bool constante) {

        //primeiro pinta as paredes de branco
        Celula atualPintura = raiz;
        Celula atualAltura = raiz;
        while (atualPintura != null) {

            foreach (Parede p in atualPintura.paredes) {
                if (p.vizinho != null) {
                    p.gameObject.SetActive(false);
                }
            }
            Celula aux = atualPintura.vizinhoDireito();
            if (aux == null) {
                atualPintura = atualAltura.vizinhoAbaixo();
                atualAltura = atualPintura;
                //yield return null;
                continue;
            }
            atualPintura = aux;
        }
        //fim da pintura das paredes de branco
        if (constante) {
            btnPausa.interactable = true;
            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(raiz, comprimento - 1, altura - 1, comprimento > altura));
            btnPausa.interactable = false;
        } else {
            btnPausa.interactable = true;
            coroutinesIds = new List<int>();
            coroutineId = 0;
            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(raiz, comprimento - 1, altura - 1, comprimento > altura));
            btnPausa.interactable = false;
        }
        yield return null;
    }

    IEnumerator GeraPorDivisaoEConquistaConstante(Celula pivot, int maximoComprimento, int maximoAltura, bool vertical) {

        int myId = coroutineId++;
        coroutinesIds.Add(myId);

        Transform fundoPivot = pivot.gameObject.transform.GetChild(1);
        fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

        Celula pivotAux = pivot;
        Transform fundo;
        int novoMaximoComprimento = maximoComprimento;
        int novoMaximoAltura = maximoAltura;
        vertical = maximoComprimento > maximoAltura;
        //Posicionamento central do pivot
        if (vertical) { //se o corte for vertical
            if (maximoComprimento > 0) { // se o pivot puder andar pra direita 
                novoMaximoComprimento = maximoComprimento / 2;
                for (int i = 1; i <= maximoComprimento / 2; i++) { //anda pra direita até a metade
                    pivotAux = pivotAux.vizinhoDireito();
                }
                if (maximoComprimento > 0 && maximoComprimento % 2 == 0 && r.NextDouble() > 0.5) {//se for par, tem que decidir qual faixa do meio
                    novoMaximoComprimento -= 1;
                    pivotAux = pivotAux.vizinhoEsquerdo();
                }

                for (int j = 0; j <= maximoAltura; j++) {
                    foreach (Parede p in pivotAux.paredes) {
                        if (p.direcao == 1) {
                            p.gameObject.SetActive(true);
                            foreach (Parede p2 in pivotAux.vizinhoDireito().paredes) {
                                if (p2.direcao == 3) {
                                    p2.gameObject.SetActive(true);
                                    Celula x = pivotAux.vizinhoAbaixo();
                                    if (j == maximoAltura)
                                        break;

                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    fundo = pivotAux.vizinhoDireito().gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    pivotAux = x;
                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }


                int abertura = r.Next(0, maximoAltura + 1);
                fundo = pivotAux.vizinhoDireito().gameObject.transform.GetChild(1);
                fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                for (int j = 0; j < maximoAltura; j++) {
                    if (j == abertura)
                        pivotAux.removeParedesEntre(pivotAux.vizinhoDireito());
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                    pivotAux = pivotAux.vizinhoAcima();
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }
                if (abertura == maximoAltura)
                    pivotAux.removeParedesEntre(pivotAux.vizinhoDireito());


                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                pivotAux = pivotAux.vizinhoDireito();
                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                if (maximoComprimento > 0) {
                    int par1;
                    int par2;
                    int par3;
                    int par4;

                    if (maximoComprimento % 2 != 0) {//se for ímpar,manda recursivo simplesmente pela metade
                        par1 = novoMaximoComprimento;
                        par2 = novoMaximoAltura;
                        par3 = novoMaximoComprimento;
                        par4 = novoMaximoAltura;
                    } else { // se for par, ver se ele foi pra esquerda
                        if (novoMaximoComprimento == maximoComprimento / 2) {
                            par1 = novoMaximoComprimento;
                            par2 = novoMaximoAltura;
                            par3 = novoMaximoComprimento - 1;
                            par4 = novoMaximoAltura;
                        } else {
                            par1 = novoMaximoComprimento;
                            par2 = novoMaximoAltura;
                            par3 = novoMaximoComprimento + 1;
                            par4 = novoMaximoAltura;
                        }
                    }
                    if (!instantaneo) {
                        if (!paralelo) {
                            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(pivotAux, par3, par4, !vertical));
                        } else {
                            StartCoroutine(GeraPorDivisaoEConquistaConstante(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(pivotAux, par3, par4, !vertical));
                        }
                    } else {
                        StartCoroutine(GeraPorDivisaoEConquistaConstante(pivot, par1, par2, !vertical));
                        StartCoroutine(GeraPorDivisaoEConquistaConstante(pivotAux, par3, par4, !vertical));
                    }
                }

            } else {
                fundoPivot = pivot.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }

        } else {//se o corte for horizontal
            if (maximoAltura > 0) { // se o pivot puder andar pra baixo 
                novoMaximoAltura = maximoAltura / 2;
                for (int i = 1; i <= maximoAltura / 2; i++) { //anda pra baixo até a metade
                    pivotAux = pivotAux.vizinhoAbaixo();
                }
                if (maximoAltura > 0 && maximoAltura % 2 == 0 && r.NextDouble() > 0.5) {//se for par, tem que decidir qual faixa do meio
                    pivotAux = pivotAux.vizinhoAcima();
                    novoMaximoAltura -= 1;
                }

                for (int j = 0; j <= maximoComprimento; j++) {
                    foreach (Parede p in pivotAux.paredes) {
                        if (p.direcao == 2) {
                            p.gameObject.SetActive(true);
                            foreach (Parede p2 in pivotAux.vizinhoAbaixo().paredes) {
                                if (p2.direcao == 0) {
                                    p2.gameObject.SetActive(true);
                                    Celula x = pivotAux.vizinhoDireito();
                                    if (j == maximoComprimento) {
                                        break;
                                    }

                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    fundo = pivotAux.vizinhoAbaixo().gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    pivotAux = x;
                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }


                int abertura = r.Next(0, maximoComprimento + 1);
                fundo = pivotAux.vizinhoAbaixo().gameObject.transform.GetChild(1);
                fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                for (int j = 0; j < maximoComprimento; j++) {
                    if (j == abertura)
                        pivotAux.removeParedesEntre(pivotAux.vizinhoAbaixo());
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                    pivotAux = pivotAux.vizinhoEsquerdo();
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }
                if (abertura == maximoComprimento)
                    pivotAux.removeParedesEntre(pivotAux.vizinhoAbaixo());


                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                pivotAux = pivotAux.vizinhoAbaixo();
                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                if (maximoAltura > 0) {
                    int par1;
                    int par2;
                    int par3;
                    int par4;

                    if (maximoAltura % 2 != 0) {//se for ímpar,manda recursivo simplesmente pela metade
                        par1 = novoMaximoComprimento;
                        par2 = novoMaximoAltura;
                        par3 = novoMaximoComprimento;
                        par4 = novoMaximoAltura;
                    } else { // se for par, ver se ele foi pra esquerda
                        if (novoMaximoAltura == maximoAltura / 2) {
                            par1 = novoMaximoComprimento;
                            par2 = novoMaximoAltura;
                            par3 = novoMaximoComprimento;
                            par4 = novoMaximoAltura - 1;
                        } else {
                            par1 = novoMaximoComprimento;
                            par2 = novoMaximoAltura;
                            par3 = novoMaximoComprimento;
                            par4 = novoMaximoAltura + 1;
                        }
                    }
                    if (!instantaneo) {
                        if (!paralelo) {
                            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(pivotAux, par3, par4, !vertical));
                        } else {
                            StartCoroutine(GeraPorDivisaoEConquistaConstante(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaConstante(pivotAux, par3, par4, !vertical));
                        }
                    } else {
                        StartCoroutine(GeraPorDivisaoEConquistaConstante(pivot, par1, par2, !vertical));
                        StartCoroutine(GeraPorDivisaoEConquistaConstante(pivotAux, par3, par4, !vertical));
                    }
                }
            } else {
                fundoPivot = pivot.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }


        coroutinesIds.Remove(myId);
        yield return null;
    }

    IEnumerator GeraPorDivisaoEConquistaVariavel(Celula pivot, int maximoComprimento, int maximoAltura, bool vertical) {

        int myId = coroutineId++;
        coroutinesIds.Add(myId);

        Transform fundoPivot = pivot.gameObject.transform.GetChild(1);
        fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

        Celula pivotAux = pivot;

        int novoMaximoComprimento = maximoComprimento;
        int novoMaximoAltura = maximoAltura;
        vertical = maximoComprimento > maximoAltura;
        Transform fundo;
        //Posicionamento central do pivot
        if (vertical) { //se o corte for vertical
            if (maximoComprimento > 0) { // se o pivot puder andar pra direita 
                novoMaximoComprimento = r.Next(maximoComprimento - 1); //escolhe um numero aleatório
                for (int i = 1; i <= novoMaximoComprimento; i++) { //anda pra direita até o numero aleatorio
                    pivotAux = pivotAux.vizinhoDireito();
                }

                for (int j = 0; j <= maximoAltura; j++) {
                    foreach (Parede p in pivotAux.paredes) {
                        if (p.direcao == 1) {
                            p.gameObject.SetActive(true);
                            foreach (Parede p2 in pivotAux.vizinhoDireito().paredes) {
                                if (p2.direcao == 3) {
                                    p2.gameObject.SetActive(true);
                                    Celula x = pivotAux.vizinhoAbaixo();
                                    if (j == maximoAltura)
                                        break;

                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    fundo = pivotAux.vizinhoDireito().gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    pivotAux = x;
                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }


                int abertura = r.Next(0, maximoAltura + 1);
                Transform fundox = pivotAux.vizinhoDireito().gameObject.transform.GetChild(1);
                fundox.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                for (int j = 0; j < maximoAltura; j++) {
                    if (j == abertura)
                        pivotAux.removeParedesEntre(pivotAux.vizinhoDireito());
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                    pivotAux = pivotAux.vizinhoAcima();
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }
                if (abertura == maximoAltura)
                    pivotAux.removeParedesEntre(pivotAux.vizinhoDireito());


                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                pivotAux = pivotAux.vizinhoDireito();
                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                if (maximoComprimento > 0) {
                    int par1;
                    int par2;
                    int par3;
                    int par4;

                    par1 = novoMaximoComprimento;
                    par2 = maximoAltura;
                    par3 = maximoComprimento - 1 - novoMaximoComprimento;
                    par4 = maximoAltura;

                    if (!instantaneo) {
                        if (!paralelo) {
                            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivotAux, par3, par4, !vertical));
                        } else {
                            StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivotAux, par3, par4, !vertical));
                        }
                    } else {
                        StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivot, par1, par2, !vertical));
                        StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivotAux, par3, par4, !vertical));
                    }
                }

            } else {
                fundoPivot = pivot.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }

        } else {//se o corte for horizontal
            if (maximoAltura > 0) { // se o pivot puder andar pra baixo 
                novoMaximoAltura = r.Next(maximoAltura - 1);
                for (int i = 1; i <= novoMaximoAltura; i++) { //anda pra baixo até a metade
                    pivotAux = pivotAux.vizinhoAbaixo();
                }

                for (int j = 0; j <= maximoComprimento; j++) {
                    foreach (Parede p in pivotAux.paredes) {
                        if (p.direcao == 2) {
                            p.gameObject.SetActive(true);
                            foreach (Parede p2 in pivotAux.vizinhoAbaixo().paredes) {
                                if (p2.direcao == 0) {
                                    p2.gameObject.SetActive(true);
                                    Celula x = pivotAux.vizinhoDireito();
                                    if (j == maximoComprimento) {
                                        break;
                                    }

                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    fundo = pivotAux.vizinhoAbaixo().gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;

                                    pivotAux = x;
                                    fundo = pivotAux.gameObject.transform.GetChild(1);
                                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }


                int abertura = r.Next(0, maximoComprimento + 1);
                Transform fundox = pivotAux.vizinhoAbaixo().gameObject.transform.GetChild(1);
                fundox.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                for (int j = 0; j < maximoComprimento; j++) {
                    if (j == abertura)
                        pivotAux.removeParedesEntre(pivotAux.vizinhoAbaixo());
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                    pivotAux = pivotAux.vizinhoEsquerdo();
                    fundo = pivotAux.gameObject.transform.GetChild(1);
                    fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                    if (!instantaneo)
                        yield return new WaitForSeconds(velocidade);
                }
                if (abertura == maximoComprimento)
                    pivotAux.removeParedesEntre(pivotAux.vizinhoAbaixo());


                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                pivotAux = pivotAux.vizinhoAbaixo();
                fundoPivot = pivotAux.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                if (maximoAltura > 0) {
                    int par1;
                    int par2;
                    int par3;
                    int par4;


                    par1 = maximoComprimento;
                    par2 = novoMaximoAltura;
                    par3 = maximoComprimento;
                    par4 = maximoAltura - 1 - novoMaximoAltura;

                    if (!instantaneo) {
                        if (!paralelo) {
                            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivotAux, par3, par4, !vertical));
                        } else {
                            StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivot, par1, par2, !vertical));
                            yield return StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivotAux, par3, par4, !vertical));
                        }
                    } else {
                        StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivot, par1, par2, !vertical));
                        StartCoroutine(GeraPorDivisaoEConquistaVariavel(pivotAux, par3, par4, !vertical));
                    }
                }
            } else {
                fundoPivot = pivot.gameObject.transform.GetChild(1);
                fundoPivot.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                yield return null;
            }
        }

        coroutinesIds.Remove(myId);

        yield return null;
    }

    IEnumerator ResolvePorAEstrela() {

        List<Celula> fila = new List<Celula>();
        List<Celula> visitados = new List<Celula>();

        Celula atual = raiz;
        Transform fundo = atual.gameObject.transform.GetChild(1);
        fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;

        atual.GScore = 0;
        atual.HScore = (int)((atual.gameObject.transform.position - fim.gameObject.transform.position).magnitude * 1000);
        atual.FScore = atual.HScore;


        while (atual != fim) {// && c<15) {
            List<Celula> filhos = atual.getFilhos();
            print(string.Format("atual: {0} H: {1} G: {2} F: {3}", atual.id, atual.HScore, atual.GScore, atual.FScore));

            foreach (Celula v in filhos) {


                v.HScore = (int)((v.gameObject.transform.position - fim.gameObject.transform.position).magnitude * 1000);


                v.GScore = v.pai.GScore + 10;
                v.FScore = v.GScore + v.HScore;

                fila.Add(v);

                fundo = v.gameObject.transform.GetChild(1);
                fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.gray;

                


            }
            yield return new WaitForSeconds(velocidade); ;

            fila.Sort((x, y) => x.FScore.CompareTo(y.FScore));
            
            atual = fila[0];
            visitados.Add(atual);
            fila.Remove(atual);



            fundo = atual.gameObject.transform.GetChild(1);
            fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            yield return new WaitForSeconds(velocidade);
        }

        //achou o caminho
        atual = fim;
        while (atual.pai != null) {
            atual = atual.pai;
            fundo = atual.gameObject.transform.GetChild(1);
            fundo.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            
            yield return new WaitForSeconds(velocidade);
        }

        btnGera.interactable = true;
        yield return null;
    }

}



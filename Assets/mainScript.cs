using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class mainScript : MonoBehaviour {

    private static int altura;
    private static int comprimento;

    float velocidade = 0f;


    public Transform celula;
    GameObject gridBase;

    public static mainScript instance;

    List<GameObject> matriz;

    int atual;
    List<int> NaoVisitados;
    Stack<int> pilha;


    public void setAltura(int v) {
        altura = v;
        StopCoroutine("DesenhaGrid");
        StartCoroutine("DesenhaGrid");
    }

    public void setComprimento(int v)
    {
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

    void Start()
    {        
        pilha = new Stack<int>();
        instance = this;
        altura = 20;
        comprimento = 20;
        NaoVisitados = Enumerable.Range(0, (altura * comprimento)).ToList();
        gridBase = GameObject.Find("GridBase");
        StartCoroutine("DesenhaGrid");
    }

    void Update () {
		
	}

    Vector3 instanciaCelula(Vector3 pos, string nome, GameObject grid, float tam, float esp) {
        GameObject cel = Instantiate(celula.gameObject, grid.transform);
        cel.transform.localPosition = pos;
        cel.transform.localScale = new Vector3(tam, tam, tam);
        cel.name = nome;
        matriz.Add(cel);
        return cel.transform.localPosition;
    }

    IEnumerator DesenhaGrid()
    {
        GameObject.Find("BtnGera").GetComponent<Button>().interactable = false;
        GameObject.Find("BtnPausa").GetComponent<Button>().interactable = false;

        float esp, tam;
        esp = 450f / Mathf.Max(altura, comprimento);
        tam = 45f / Mathf.Max(altura, comprimento);

        Vector3 pos = new Vector3(0, 0, 0);

        NaoVisitados = Enumerable.Range(0, (altura * comprimento)).ToList();
        pilha = new Stack<int>();

        GameObject grid = GameObject.Find("Grid");
        Destroy(grid);
        grid = new GameObject("Grid");
        grid.transform.parent = gridBase.transform;
        grid.transform.localPosition = pos;
        grid.transform.localScale = new Vector3(1, 1, 1);

        matriz = new List<GameObject>();

        

        for (int i = 1; i <= altura; i++)
        {
            pos = instanciaCelula(pos, "Cell 1" + i, grid, tam, esp);
            for (float f = 2f; f <= comprimento; f++)
            {
                pos = instanciaCelula(pos + new Vector3(esp, 0, 0), "Cell " + f + i, grid, tam, esp);
            }
            pos = pos + new Vector3(-esp * (comprimento-1), -esp, 0);
            
            if(i%2 == 0)
                yield return null;
        }
        GameObject.Find("BtnGera").GetComponent<Button>().interactable = true;
        yield return null;
        
    }

    public void IniciaGeraBuscaEmProfundidade() {
        StartCoroutine("GeraBuscaEmProfundidade");
    }
    public void PausaGeraBuscaEmProfundidade()
    {
        StopCoroutine("GeraBuscaEmProfundidade");
    }
    public void Limpa() {
        StopCoroutine("DesenhaGrid");
        StartCoroutine("DesenhaGrid");
    }

    void setAtual(int g, int tipo) {
        Transform c = matriz[atual].transform.GetChild(1);
        if (tipo == 1)
            c.gameObject.GetComponent<MeshRenderer>().material.color = new Color(255, 255, 255);
        else
            c.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0f, 255f, 130f, 15f);
        atual = g;

        NaoVisitados.Remove(g);
        c = matriz[atual].transform.GetChild(1);
        //if (tipo == 0)
            c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        //else
            //c.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;

    }
    void setAtual(int g) {
        atual = g;
        NaoVisitados.Remove(g);
        Transform c = matriz[atual].transform.GetChild(1);
        c.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    List<int> getVizinhos(int a) {

        List<int> ret = new List<int>();
        int indice;

        indice = a - comprimento; //de cima
        if (indice >= 0 && NaoVisitados.Contains(indice))
            ret.Add(indice);

        indice = a + comprimento; //de baixo
        if (indice < comprimento * altura && NaoVisitados.Contains(indice))
            ret.Add(indice);

        indice = a - 1; //da esquerda
        if (a % comprimento != 0 && NaoVisitados.Contains(indice))
            ret.Add(indice);

        indice = a + 1; //da direita
        if ((a + 1) % comprimento != 0 && NaoVisitados.Contains(indice))
            ret.Add(indice);

        return ret;
    }

    void adicionaNaPilha(int a)
    {
        pilha.Push(a);
    }

    int retirarDaPilha()
    {
        int a = pilha.Pop();
        return a;
    }

    void retiraParedes(int a, int b) {
        if (b - a == comprimento) //vizinho de baixo
        {
            Destroy(matriz[a].transform.GetChild(0).Find("btm").gameObject);
            Destroy(matriz[b].transform.GetChild(0).Find("top").gameObject);
        }
        else if (a - b == comprimento) //vizinho de cima
        {
            Destroy(matriz[b].transform.GetChild(0).Find("btm").gameObject);
            Destroy(matriz[a].transform.GetChild(0).Find("top").gameObject);
        }
        else if (b == a + 1) //vizinho da direita
        {
            Destroy(matriz[a].transform.GetChild(0).Find("rgt").gameObject);
            Destroy(matriz[b].transform.GetChild(0).Find("lft").gameObject);
        }
        else //vizinho da esquerda
        {
            Destroy(matriz[b].transform.GetChild(0).Find("rgt").gameObject);
            Destroy(matriz[a].transform.GetChild(0).Find("lft").gameObject);
        }
    }

    IEnumerator GeraBuscaEmProfundidade() {
        GameObject.Find("BtnPausa").GetComponent<Button>().interactable = true;
        GameObject.Find("BtnLimpa").GetComponent<Button>().interactable = false;

        setAtual(0);

        List<int> vizinhos = new List<int>();
        vizinhos = getVizinhos(atual);

        while (NaoVisitados.Count > 0 || pilha.Count > 0) {
            vizinhos = getVizinhos(atual);
            if (vizinhos.Count > 0) {
                System.Random r = new System.Random();
                int vizinhoAleatorio = vizinhos[r.Next(vizinhos.Count)];

                adicionaNaPilha(atual);
                
                retiraParedes(atual, vizinhoAleatorio);

                setAtual(vizinhoAleatorio, 0);
                
            } else if (pilha.Count > 0)
            {
                int r = retirarDaPilha();
                setAtual(r, 1);
            }
            yield return new WaitForSeconds(velocidade);
        }
        GameObject.Find("BtnPausa").GetComponent<Button>().interactable = false;
        GameObject.Find("BtnLimpa").GetComponent<Button>().interactable = true;
        yield return null;
    }
}

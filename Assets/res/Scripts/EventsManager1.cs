using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventsManager1 : MonoBehaviour {

    int tipoDeGeracao = 0;
    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AlterCompVal(float val) {
        this.GetComponent<Text>().text = val.ToString();
        mainScript2.instance.setComprimento((int)val);
    }

    public void AlterAltVal(float val){
        this.GetComponent<Text>().text = val.ToString();
        mainScript2.instance.setAltura((int) val);
    }

    public void AlterVel(float val)
    {
        mainScript2.instance.setVel(-val);
    }

    public void AlterTipo(int i) {
        tipoDeGeracao = i;
    }

    public void GeraLabirinto() {
        this.GetComponent<Button>().interactable = false;
        if (tipoDeGeracao == 0)
        {            
            mainScript2.instance.IniciaGeraBuscaEmProfundidade();
        }
            
        else if (tipoDeGeracao == 1)
        {
            ;// Prim
        }

        else
        {
            ; // Divisao
        }
            
    }

    public void Pausa() {
        this.GetComponent<Button>().interactable = false;
        GameObject.Find("BtnLimpa").GetComponent<Button>().interactable = true;
        GameObject.Find("BtnGera").GetComponent<Button>().interactable = false;
        if (tipoDeGeracao == 0)
        {
            mainScript2.instance.PausaGeraBuscaEmProfundidade();
        }

        else if (tipoDeGeracao == 1)
        {
            ;// Prim
        }

        else
        {
            ; // Divisao
        }
    }

    public void Limpa() {
        mainScript2.instance.Limpa();
        //GameObject.Find("BtnGera").GetComponent<Button>().interactable = true;
    }
}

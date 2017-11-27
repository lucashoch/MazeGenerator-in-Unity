using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventsManager : MonoBehaviour {

    int tipoDeGeracao = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AlterCompVal(float val) {
        this.GetComponent<Text>().text = val.ToString();
        mainScript.instance.setComprimento((int)val);
    }

    public void AlterAltVal(float val){
        this.GetComponent<Text>().text = val.ToString();
        mainScript.instance.setAltura((int) val);
    }

    public void AlterVel(float val)
    {
        mainScript.instance.setVel(-val);
    }

    public void AlterTipo(int i) {
        tipoDeGeracao = i;
    }

    public void GeraLabirinto() {
        this.GetComponent<Button>().interactable = false;
        if (tipoDeGeracao == 0)
        {            
            mainScript.instance.IniciaGeraBuscaEmProfundidade();
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
            mainScript.instance.PausaGeraBuscaEmProfundidade();
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
        mainScript.instance.Limpa();
        //GameObject.Find("BtnGera").GetComponent<Button>().interactable = true;
    }
}

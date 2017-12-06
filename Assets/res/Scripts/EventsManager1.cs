using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventsManager1 : MonoBehaviour {

    int tipoDeGeracao = 0;


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void AlterCompVal(float val) {
        this.GetComponent<Text>().text = val.ToString();
        mainScript2.instance.setComprimento((int)val);
    }

    public void AlterAltVal(float val) {
        this.GetComponent<Text>().text = val.ToString();
        mainScript2.instance.setAltura((int)val);
    }

    public void AlterVel(float val) {
        mainScript2.instance.setVel(-val);
    }

    public void AlterTipo(int i) {
        tipoDeGeracao = i;
        if(i == 2 || i == 3)
            GameObject.Find("TglParalelo").GetComponent<Toggle>().interactable = true;
        else
            GameObject.Find("TglParalelo").GetComponent<Toggle>().interactable = false;
    }

    public void GeraLabirinto() {
        mainScript2.instance.IniciaGeraMaze(tipoDeGeracao);
    }

    public void Pausa() {
        GameObject.Find("BtnPausa").GetComponent<Button>().interactable = false;
        //GameObject.Find("BtnLimpa").GetComponent<Button>().interactable = true;
        mainScript2.instance.PausaGeraMaze(tipoDeGeracao);

    }

    public void Resolve() {
        this.GetComponent<Button>().interactable = false;
        mainScript2.instance.Resolve();
    }

    public void ToggleInstantaneo(bool v) {
        mainScript2.instance.setInstantaneo(v);
    }

    public void AlterParalelo(bool v) {
        mainScript2.instance.setParalelo(v);
    }

    public void Limpa() {
        mainScript2.instance.Limpa();
        //GameObject.Find("BtnGera").GetComponent<Button>().interactable = true;
    }
}

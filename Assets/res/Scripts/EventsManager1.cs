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
    }

    public void GeraLabirinto() {
        GameObject.Find("BtnGera").GetComponent<Button>().interactable = false;
        mainScript2.instance.IniciaGeraMaze(tipoDeGeracao);
    }

    public void Pausa() {
        GameObject.Find("BtnPausa").GetComponent<Button>().interactable = false;
        GameObject.Find("BtnLimpa").GetComponent<Button>().interactable = true;
        GameObject.Find("BtnGera").GetComponent<Button>().interactable = false;
        mainScript2.instance.PausaGeraMaze(tipoDeGeracao);

    }

    public void ToggleInstantaneo(bool v) {
        mainScript2.instance.setInstantaneo(v);
    }

    public void Limpa() {
        mainScript2.instance.Limpa();
        //GameObject.Find("BtnGera").GetComponent<Button>().interactable = true;
    }
}

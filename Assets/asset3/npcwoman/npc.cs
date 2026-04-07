using UnityEngine;
using UnityEngine.AI;

public class NPC_Beyin : MonoBehaviour {
    public Transform hedef; // Buraya Nokta_1'i sürükleyeceksin
    
    void Start() {
        // NPC'ye "Hadi yürü" diyoruz
        GetComponent<NavMeshAgent>().SetDestination(hedef.position);
    }
}
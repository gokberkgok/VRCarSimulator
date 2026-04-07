using UnityEngine;
using UnityEngine.AI;

public class NPC_Sistem : MonoBehaviour 
{
    private NavMeshAgent agent;
    private GameObject[] tumHedefler; // Sahnedeki tüm hedefleri otomatik bulacak

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        
        // Sahnedeki "NPC_Hedef" tag'ine sahip tüm objeleri bulur
        tumHedefler = GameObject.FindGameObjectsWithTag("NPC_Hedef");
        
        YeniHedefSec();
    }

    void Update() {
        // Hedefe yaklaştıysa durma, yenisini seç
        if (!agent.pathPending && agent.remainingDistance < 0.5f) {
            YeniHedefSec();
        }
    }

    void YeniHedefSec() {
        if (tumHedefler.Length > 0) {
            int rastgele = Random.Range(0, tumHedefler.Length);
            agent.SetDestination(tumHedefler[rastgele].transform.position);
        }
    }
}
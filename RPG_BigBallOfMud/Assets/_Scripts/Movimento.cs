using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : IMovimento
{
    private NavMeshAgent agent;
    private Transform transform;

    public NavMeshMovement(NavMeshAgent agent, Transform transform)
    {
        this.agent = agent;
        this.transform = transform;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public Vector3 Posicao => transform.position;
    public bool EstaParado => agent.isStopped || (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending);

    public void MoverPara(Vector3 destino)
    {
        agent.SetDestination(destino);
        agent.isStopped = false;
    }

    public void Parar()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    public void RotacionarPara(Vector3 alvo)
    {
        Vector3 direcao = (alvo - transform.position).normalized;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    public void RotacionarParaVelocidade()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}
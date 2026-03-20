using UnityEngine;

public class AtaqueBasico : IAtaque
{
    private float cooldownAtual;
    private float cooldownMax = 1f;

    public bool PodeAtacar => cooldownAtual <= 0;

    public void AtualizarCooldown()
    {
        if (cooldownAtual > 0)
            cooldownAtual -= Time.deltaTime;
    }

    public void Atacar(Transform atacante, string nomeInimigo, ILogger logger)
    {
        if (!PodeAtacar) return;

        logger?.Log($"O {nomeInimigo.ToLower()} ataca com ferocidade!");

        GameObject bastao = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(bastao.GetComponent<MeshCollider>());
        bastao.transform.SetParent(atacante);
        bastao.transform.localPosition = new Vector3(0.8f, 0, -0.1f);
        bastao.transform.localRotation = Quaternion.identity;
        bastao.transform.localScale = new Vector3(1f, 0.2f, 1f);
        bastao.GetComponent<Renderer>().material.color = Color.red;
        bastao.GetComponent<Renderer>().sortingOrder = 10;
        Object.Destroy(bastao, 0.2f);

        cooldownAtual = cooldownMax;
    }
}
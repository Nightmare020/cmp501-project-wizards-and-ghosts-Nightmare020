
using UnityEngine;

public class SetShaderValues : MonoBehaviour
{
    [SerializeField] private Material mat;

    private PlayerManager player;

    private static readonly int Pos = Shader.PropertyToID("_pos");

    // Start is called before the first frame update
    void Start()
    {
        player = GetGosht();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        player = GetGosht();
        if (player)
        {
            mat.SetVector(Pos, player.transform.position);
        }
    }

    private PlayerManager GetGosht()
    {
        if (player != null && player.CompareTag("ActiveGhost"))
        {
            return player;
        }

        foreach (var p in FindObjectsOfType<PlayerManager>())
        {
            if (p.CompareTag("ActiveGhost"))
            {
                return p;
            }
        }

        return null;
    }
}
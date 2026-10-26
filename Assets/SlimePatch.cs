using System.Collections;
using UnityEngine;


// Flaque de Slime (ConjuringSLime): Prévient le sort quand le joueur la touche, gère son apparition, son clignotement de contact et sa disparition.
public class SlimePatch : MonoBehaviour
{
    private ConjureSlime owner;
    private PlayerControler player;
    private Renderer rend;
    private Color baseColor;
    private Vector3 fullScale;
    private float lifetime;
    private int contacts;
    private bool dismissing;

    public bool IsWall { get; private set; }

    public void Init(ConjureSlime owner, PlayerControler player, bool isWall, Color color, float lifetime)
    {
        this.owner = owner;
        this.player = player;
        this.lifetime = lifetime;
        IsWall = isWall;
        baseColor = color;
        fullScale = transform.localScale;
        rend = GetComponentInChildren<Renderer>();
        StartCoroutine(LifeRoutine());
    }

    public void Dismiss()
    {
        if (dismissing) return;
        dismissing = true;
        StopAllCoroutines();
        StartCoroutine(ShrinkAndDestroy(0.2f));
    }

    private IEnumerator LifeRoutine()
    {
        yield return Scale(0f, 1f, 0.15f); // apparition

        if (lifetime > 0f)
        {
            yield return new WaitForSeconds(lifetime);
            dismissing = true;
            yield return ShrinkAndDestroy(0.4f);
        }
    }

    private IEnumerator ShrinkAndDestroy(float duration)
    {
        float start = transform.localScale.x / Mathf.Max(fullScale.x, 0.0001f);
        yield return Scale(start, 0f, duration);
        Destroy(gameObject);
    }

    private IEnumerator Scale(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(from, to, Mathf.Clamp01(t / duration));
            transform.localScale = fullScale * k;
            yield return null;
        }
        transform.localScale = fullScale * to;
    }

    private IEnumerator Flash()
    {
        if (rend == null || !rend.material.HasProperty("_Color")) yield break;

        Color bright = Color.Lerp(baseColor, Color.white, 0.6f);
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            rend.material.color = Color.Lerp(bright, baseColor, t / 0.2f);
            yield return null;
        }
        rend.material.color = baseColor;
    }

    private bool IsPlayer(Collision c)
    {
        return player != null && c.collider.GetComponentInParent<PlayerControler>() == player;
    }

    private void OnCollisionEnter(Collision c)
    {
        if (!IsPlayer(c)) return;
        if (contacts++ == 0)
        {
            owner.NotifyEnter(this);
            StartCoroutine(Flash());
        }
    }

    private void OnCollisionExit(Collision c)
    {
        if (!IsPlayer(c)) return;
        if (--contacts <= 0)
        {
            contacts = 0;
            owner.NotifyExit(this);
        }
    }

    private void OnDestroy()
    {
        if (owner != null) owner.NotifyExit(this);
    }
}

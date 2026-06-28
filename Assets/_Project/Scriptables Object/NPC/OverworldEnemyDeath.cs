using System.Collections;
using UnityEngine;

namespace WannaBHero
{
    // Pasang di root prefab enemy OVERWORLD
    // Tidak pakai timer — tunggu Animation Event dari clip Die
    public class OverworldEnemyDeath : MonoBehaviour
    {
        [Header("Animator Parameter")]
        [Tooltip("Nama parameter isDie di Animator overworld enemy")]
        [SerializeField] private string paramDie = "isDie";

        [Header("Fade Setelah Animasi Selesai")]
        [SerializeField] private float fadeOutDuration = 0.5f;

        private Animator animator;
        private Collider2D col;
        private Rigidbody2D rb;
        private bool deathAnimDone = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            col = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
        }

        // Dipanggil oleh BattleManager setelah kembali ke overworld
        public void PlayDeathAndDestroy()
        {
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            // Cegah trigger battle lagi & hentikan gerak
            if (col != null) col.enabled = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            DisableEnemyScripts();

            // Set animasi Die
            deathAnimDone = false;
            if (animator != null)
            {
                animator.SetBool(paramDie, true);
                Debug.Log($"[OverworldDeath] {name}: mulai animasi Die.");
            }
            else
            {
                // Tidak ada animator — langsung lanjut
                deathAnimDone = true;
                Debug.LogWarning($"[OverworldDeath] {name}: Animator tidak ditemukan!");
            }

            // Tunggu Animation Event OnDeathAnimationEnd() dari clip Die
            // Fallback 5 detik jika Animation Event belum dipasang
            float timeout = 0f;
            while (!deathAnimDone && timeout < 5f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            if (!deathAnimDone)
                Debug.LogWarning($"[OverworldDeath] {name}: Timeout! " +
                                 "Pasang Animation Event 'OnDeathAnimationEnd' " +
                                 "di frame terakhir clip Die.");

            // Fade out semua SpriteRenderer
            yield return StartCoroutine(FadeOut());

            Debug.Log($"[OverworldDeath] {name}: Destroy.");
            Destroy(gameObject);
        }

        private IEnumerator FadeOut()
        {
            SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                foreach (SpriteRenderer sr in srs)
                {
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, alpha);
                }
                yield return null;
            }
        }

        // ── Dipanggil Animation Event di frame TERAKHIR clip Die ──
        public void OnDeathAnimationEnd()
        {
            deathAnimDone = true;
            Debug.Log($"[OverworldDeath] {name}: Animasi Die selesai.");
        }

        private void DisableEnemyScripts()
        {
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour s in scripts)
                if (s != this) s.enabled = false;
        }
    }
}
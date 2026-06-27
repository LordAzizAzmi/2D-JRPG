using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace WannaBHero.Battle
{
    public enum TurnState { PlayerTurn, EnemyTurn, Win, Lose }

    public class BattleTurnManager : MonoBehaviour
    {
        public static BattleTurnManager Instance { get; private set; }

        [Header("Referensi")]
        [SerializeField] private BattlePlayerController playerController;

        [Header("Delay antar giliran (detik)")]
        [SerializeField] private float delayBetweenTurns = 0.8f;

        public UnityEvent<TurnState> OnTurnChanged;
        public UnityEvent<string, int> OnDamageDealt;
        public UnityEvent OnPlayerWin;
        public UnityEvent OnPlayerLose;

        public TurnState CurrentTurn { get; private set; }

        private EnemyBattleController enemyController;

        // Turn counter berdasarkan speed
        private int playerTurnsPerRound;
        private int enemyTurnsPerRound;
        private int playerTurnsLeft;
        private int enemyTurnsLeft;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable() => BattleEnemySpawner.OnEnemySpawned += HandleEnemySpawned;
        private void OnDisable() => BattleEnemySpawner.OnEnemySpawned -= HandleEnemySpawned;

        private void HandleEnemySpawned(Transform spawnedEnemy)
        {
            enemyController = spawnedEnemy.GetComponent<EnemyBattleController>();
            if (enemyController == null)
            {
                Debug.LogError("[BattleTurnManager] EnemyBattleController tidak ditemukan!");
                return;
            }

            playerController.OnActionEnd += HandlePlayerActionEnd;
            enemyController.OnActionEnd += HandleEnemyActionEnd;

            CalculateTurnRatio();
            StartNewRound();
        }

        // ─── Speed Ratio ─────────────────────────────────────────

        private void CalculateTurnRatio()
        {
            int pSpeed = playerController.Speed;
            int eSpeed = enemyController.Speed;
            int minSpeed = Mathf.Min(pSpeed, eSpeed);

            // Hitung berapa kali lipat speed masing-masing
            playerTurnsPerRound = Mathf.Max(1, Mathf.RoundToInt((float)pSpeed / minSpeed));
            enemyTurnsPerRound = Mathf.Max(1, Mathf.RoundToInt((float)eSpeed / minSpeed));

            Debug.Log($"[Battle] Speed Ratio — Player {pSpeed} vs Enemy {eSpeed}\n" +
                      $"Player: {playerTurnsPerRound}x attack, " +
                      $"Enemy: {enemyTurnsPerRound}x attack per round");
        }

        private void StartNewRound()
        {
            // Reset counter giliran untuk round baru
            playerTurnsLeft = playerTurnsPerRound;
            enemyTurnsLeft = enemyTurnsPerRound;

            Debug.Log($"[Battle] Round baru! " +
                      $"Player: {playerTurnsLeft} giliran, " +
                      $"Enemy: {enemyTurnsLeft} giliran");

            StartCoroutine(DelayThenSetTurn(TurnState.PlayerTurn));
        }

        // ─── Turn Flow ───────────────────────────────────────────

        private void SetTurn(TurnState newTurn)
        {
            CurrentTurn = newTurn;
            OnTurnChanged?.Invoke(newTurn);

            switch (newTurn)
            {
                case TurnState.PlayerTurn:
                    playerController.EnableInput(true);
                    Debug.Log($"[Battle] Giliran PLAYER " +
                              $"(sisa {playerTurnsLeft}x)");
                    break;

                case TurnState.EnemyTurn:
                    playerController.EnableInput(false);
                    Debug.Log($"[Battle] Giliran ENEMY " +
                              $"(sisa {enemyTurnsLeft}x)");
                    StartCoroutine(EnemyTurnRoutine());
                    break;

                case TurnState.Win:
                    playerController.EnableInput(false);
                    OnPlayerWin?.Invoke();
                    StartCoroutine(EndBattleRoutine(true));
                    break;

                case TurnState.Lose:
                    playerController.EnableInput(false);
                    OnPlayerLose?.Invoke();
                    StartCoroutine(EndBattleRoutine(false));
                    break;
            }
        }

        private IEnumerator EnemyTurnRoutine()
        {
            yield return new WaitForSeconds(delayBetweenTurns);
            enemyController.ExecuteAttack();
        }

        // ─── Callback ────────────────────────────────────────────

        private void HandlePlayerActionEnd()
        {
            if (!enemyController.IsAlive) { SetTurn(TurnState.Win); return; }

            playerTurnsLeft--;

            if (playerTurnsLeft > 0)
            {
                // Masih ada giliran player di round ini
                StartCoroutine(DelayThenSetTurn(TurnState.PlayerTurn));
            }
            else
            {
                // Giliran player habis → switch ke enemy
                StartCoroutine(DelayThenSetTurn(TurnState.EnemyTurn));
            }
        }

        private void HandleEnemyActionEnd()
        {
            if (!playerController.IsAlive) { SetTurn(TurnState.Lose); return; }

            enemyTurnsLeft--;

            if (enemyTurnsLeft > 0)
            {
                // Masih ada giliran enemy di round ini
                StartCoroutine(DelayThenSetTurn(TurnState.EnemyTurn));
            }
            else
            {
                // Round selesai → mulai round baru
                StartNewRound();
            }
        }

        // ─── Helpers ─────────────────────────────────────────────

        private IEnumerator DelayThenSetTurn(TurnState next)
        {
            yield return new WaitForSeconds(delayBetweenTurns);
            SetTurn(next);
        }

        private IEnumerator EndBattleRoutine(bool playerWon)
        {
            yield return new WaitForSeconds(2f);
            BattleManager.Instance?.EndBattle(playerWon);
        }

        public void NotifyDamage(string targetName, int amount)
        {
            OnDamageDealt?.Invoke(targetName, amount);
        }
    }
}
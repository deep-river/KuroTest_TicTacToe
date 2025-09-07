using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameRecorder : MonoBehaviour
{
    [Serializable]
    public class MoveRecord
    {
        public int step;      // 1..N
        public int index;     // 0..8
        public string mark;   // "X" / "O"
    }

    [Serializable]
    public class MatchLog
    {
        public int round;
        public string difficulty;
        public string startUtc;
        public string endUtc;
        public float durationSec;
        public string result;         // "Win" | "Lose" | "Draw"
        public int[] winningLine;     // 3 indices or null
        public List<MoveRecord> moves = new();
    }

    [Serializable]
    public class SessionLog
    {
        public string sessionStartUtc;
        public List<MatchLog> matches = new();
        public int playerWins;
        public int aiWins;
        public int draws;
    }

    [Header("References")]
    [SerializeField] private GameStateManager game;
    [SerializeField] private DifficultyManager difficulty;

    [Header("Logging")]
    [SerializeField] private bool writeJsonOnMatchEnd = false;

    private SessionLog session = new();
    private MatchLog current;
    private DateTime sessionStartUtc;

    public SessionLog Session => session;
    public MatchLog LastMatch => current;

    private void Awake()
    {
        if (!game) game = FindObjectOfType<GameStateManager>();
        if (!difficulty) difficulty = FindObjectOfType<DifficultyManager>();

        sessionStartUtc = DateTime.UtcNow;
        session.sessionStartUtc = sessionStartUtc.ToString("o");
    }

    private void OnEnable()
    {
        if (!game) return;
        game.OnRoundStarted += HandleRoundStarted;
        game.OnMoveCommitted += HandleMoveCommitted;
        game.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (!game) return;
        game.OnRoundStarted -= HandleRoundStarted;
        game.OnMoveCommitted -= HandleMoveCommitted;
        game.OnGameOver -= HandleGameOver;
    }

    private void HandleRoundStarted(int round)
    {
        current = new MatchLog
        {
            round = round,
            difficulty = difficulty ? difficulty.GetDisplayName() : "Unknown",
            startUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private void HandleMoveCommitted(int index, Mark mark)
    {
        if (current == null) return;
        current.moves.Add(new MoveRecord
        {
            step = current.moves.Count + 1,
            index = index,
            mark = (mark == Mark.X ? "X" : "O")
        });
    }

    private void HandleGameOver(GameResult result, int[] winningLine)
    {
        if (current == null) return;

        var end = DateTime.UtcNow;
        current.endUtc = end.ToString("o");
        current.durationSec = (float)(end - DateTime.Parse(current.startUtc)).TotalSeconds;
        current.result = result switch
        {
            GameResult.HumanWin => "Win",
            GameResult.AIWin => "Lose",
            _ => "Draw"
        };
        current.winningLine = winningLine;

        session.matches.Add(current);
        switch (result)
        {
            case GameResult.HumanWin: session.playerWins++; break;
            case GameResult.AIWin: session.aiWins++; break;
            case GameResult.Draw: session.draws++; break;
        }

        if (writeJsonOnMatchEnd)
        {
            TryWriteJson(current);
        }
    }

    private void TryWriteJson(MatchLog log)
    {
        try
        {
            var dir = Path.Combine(Application.persistentDataPath, "logs");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"match_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            File.WriteAllText(file, JsonUtility.ToJson(log, true));
            Debug.Log($"[GameRecorder] Match log saved: {file}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameRecorder] Save failed: {e.Message}");
        }
    }
}

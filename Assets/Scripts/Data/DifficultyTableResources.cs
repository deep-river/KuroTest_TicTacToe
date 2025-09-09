using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DifficultyRow
{
    public string id;
    public string displayName;
    public float mistakeRate;
    public bool randomizeAmongBest;
    public int depthLimit;
}

[Serializable]
public class DifficultyDb
{
    public List<DifficultyRow> items = new List<DifficultyRow>();
}

/// <summary>
/// 超简 Resources 读表 + 会话内热重载（仅内存）
/// 不做磁盘覆盖；下次重启仍回到 Resources 内置表。
/// </summary>
public static class DifficultyTableResources
{
    private const string ResourcePath = "Config/difficulty"; // 不带 .json
    private static string _currentJson;                                   // 当前内存中的原始 JSON 文本（用于调试面板显示）
    private static Dictionary<AIDifficultyId, AIDifficulty> _cache;       // 当前解析结果

    public static bool IsLoaded => _cache != null;

    public static IReadOnlyDictionary<AIDifficultyId, AIDifficulty> Get()
    {
        if (_cache == null) ReloadFromResources();
        return _cache;
    }

    /// <summary>从 Resources 重载（重置会话内的任何修改）。</summary>
    public static void ReloadFromResources()
    {
        var ta = Resources.Load<TextAsset>(ResourcePath);
        if (ta == null || string.IsNullOrEmpty(ta.text))
        {
            Debug.LogWarning($"[DifficultyTableResources] Missing TextAsset at Resources/{ResourcePath}.json, using defaults.");
            ApplyInMemory(BuildDefaultDb());
            return;
        }
        _currentJson = ta.text;
        if (!TryParse(_currentJson, out var dict))
        {
            Debug.LogWarning("[DifficultyTableResources] Parse failed, using defaults.");
            ApplyInMemory(BuildDefaultDb());
            return;
        }
        _cache = dict;
    }

    /// <summary>供调试面板显示的 JSON 原文（会随着 TryApplyJsonAtRuntime 成功应用而更新）。</summary>
    public static string GetCurrentJson()
    {
        if (_cache == null) ReloadFromResources();
        return string.IsNullOrEmpty(_currentJson) ? JsonUtility.ToJson(BuildDefaultDb(), true) : _currentJson;
    }

    /// <summary>
    /// 用于调试面板的Reset功能
    /// </summary>
    /// <returns></returns>
    public static string GetResourcesJson()
    {
        var ta = Resources.Load<TextAsset>("Config/difficulty"); // 与表路径一致
        if (ta != null && !string.IsNullOrEmpty(ta.text)) return ta.text;
        // 兜底：默认表
        return "";
    }

    /// <summary>
    /// 会话内热重载：把编辑后的 JSON 直接应用到内存。
    /// 成功返回 true；失败返回 false 并提供错误原因。
    /// </summary>
    public static bool TryApplyJsonAtRuntime(string json, out string error)
    {
        error = null;
        if (!TryParse(json, out var dict))
        {
            error = "JSON 解析失败或缺少必要字段。";
            return false;
        }
        _currentJson = Beautify(json);
        _cache = dict;
        return true;
    }

    // —— 内部 —— //
    private static bool TryParse(string json, out Dictionary<AIDifficultyId, AIDifficulty> dict)
    {
        dict = null;
        try
        {
            var db = JsonUtility.FromJson<DifficultyDb>(json);
            if (db == null || db.items == null || db.items.Count == 0) return false;

            var result = new Dictionary<AIDifficultyId, AIDifficulty>();
            foreach (var row in db.items)
            {
                if (!Enum.TryParse(row.id, true, out AIDifficultyId id)) continue;
                result[id] = new AIDifficulty
                {
                    id = id,
                    displayName = string.IsNullOrEmpty(row.displayName) ? id.ToString() : row.displayName,
                    depthLimit = Mathf.Max(1, row.depthLimit),
                    mistakeRate = Mathf.Clamp01(row.mistakeRate),
                    randomizeAmongBest = row.randomizeAmongBest
                };
            }

            // 补齐缺失档位
            var def = BuildDefaultDb();
            foreach (AIDifficultyId id in Enum.GetValues(typeof(AIDifficultyId)))
            {
                if (!result.ContainsKey(id))
                {
                    var d = def.items.Find(x => string.Equals(x.id, id.ToString(), StringComparison.OrdinalIgnoreCase));
                    result[id] = new AIDifficulty
                    {
                        id = id,
                        displayName = d?.displayName ?? id.ToString(),
                        depthLimit = d?.depthLimit ?? 9,
                        mistakeRate = d?.mistakeRate ?? 0f,
                        randomizeAmongBest = d?.randomizeAmongBest ?? false
                    };
                }
            }

            dict = result;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DifficultyTableResources] Parse exception: {e.Message}");
            return false;
        }
    }

    private static void ApplyInMemory(DifficultyDb db)
    {
        _currentJson = JsonUtility.ToJson(db, true);
        // 复用 TryParse 构造缓存
        TryParse(_currentJson, out _cache);
    }

    private static DifficultyDb BuildDefaultDb()
    {
        return new DifficultyDb
        {
            items = new List<DifficultyRow>
            {
                new DifficultyRow { id="Easy",   displayName="Easy",   mistakeRate=0.35f, randomizeAmongBest=true,  depthLimit=5 },
                new DifficultyRow { id="Normal", displayName="Normal", mistakeRate=0.10f, randomizeAmongBest=true,  depthLimit=9 },
                new DifficultyRow { id="Hard",   displayName="Hard",   mistakeRate=0.00f, randomizeAmongBest=false, depthLimit=9 },
            }
        };
    }

    private static string Beautify(string json)
    {
        try
        {
            var db = JsonUtility.FromJson<DifficultyDb>(json);
            return JsonUtility.ToJson(db, true);
        }
        catch { return json; }
    }
}

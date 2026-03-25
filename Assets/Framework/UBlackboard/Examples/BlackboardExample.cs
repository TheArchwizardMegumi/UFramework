using UnityEngine;

namespace UFramework.Examples
{
    /// <summary>
    /// 黑板系统使用示例
    /// </summary>
    public class BlackboardExample : MonoBehaviour
    {
        void Start()
        {
            // ========== 基础使用 ==========

            // 使用默认黑板
            UBlackboardManager.Instance.Set("PlayerLevel", 5);
            UBlackboardManager.Instance.Set("PlayerHealth", 100f);
            UBlackboardManager.Instance.Set("PlayerName", "Hero");

            // 获取数据
            int level = UBlackboardManager.Instance.Get<int>("PlayerLevel");
            float health = UBlackboardManager.Instance.Get<float>("PlayerHealth");
            string name = UBlackboardManager.Instance.Get<string>("PlayerName");

            Debug.Log($"玩家信息 - 等级: {level}, 生命: {health}, 名字: {name}");

            // ========== 尝试获取 ==========

            if (UBlackboardManager.Instance.TryGet("PlayerLevel", out int safeLevel))
            {
                Debug.Log($"安全获取玩家等级: {safeLevel}");
            }

            // ========== 数据存在性检查 ==========

            if (UBlackboardManager.Instance.ContainsKey("PlayerHealth"))
            {
                Debug.Log("玩家生命值存在");
            }

            // ========== 数据更新 ==========

            UBlackboardManager.Instance.Set("PlayerHealth", 80f); // 更新生命值
            UBlackboardManager.Instance.Add("PlayerLevel", 1);    // 等级+1

            // ========== 使用扩展方法 ==========

            var blackboard = UBlackboardManager.Instance.Default;

            // Vector3
            blackboard.SetVector3("PlayerPosition", new Vector3(10, 0, 5));
            Vector3 pos = blackboard.GetVector3("PlayerPosition");
            Debug.Log($"玩家位置: {pos}");

            // Color
            blackboard.SetColor("PlayerColor", Color.red);
            Color color = blackboard.GetColor("PlayerColor");
            Debug.Log($"玩家颜色: {color}");

            // 数组
            blackboard.SetArray("Inventory", new[] { "Sword", "Shield", "Potion" });
            string[] inventory = blackboard.GetArray<string>("Inventory");
            Debug.Log($"背包物品: {string.Join(", ", inventory)}");

            // ========== 多黑板支持 ==========

            var gameBlackboard = UBlackboardManager.Instance.GetBlackboard("Game");
            var uiBlackboard = UBlackboardManager.Instance.GetBlackboard("UI");
            var aiBlackboard = UBlackboardManager.Instance.GetBlackboard("AI");

            gameBlackboard.Set("GameTime", 120.5f);
            uiBlackboard.Set("CurrentPanel", "MainMenu");
            aiBlackboard.Set("EnemyCount", 10);

            // ========== 事件监听 ==========

            // 全局监听
            UBlackboardManager.Instance.AddListener((args) =>
            {
                Debug.Log($"数据变更: {args.Key} = {args.NewValue} ({args.ChangeType})");
            });

            // 键级监听
            UBlackboardManager.Instance.AddKeyListener("PlayerHealth", (args) =>
            {
                if (args.ChangeType != BlackboardChangeType.Removed)
                {
                    Debug.Log($"玩家生命值变更: {args.NewValue}");
                    // 可以在这里更新UI显示
                }
            });

            // 类型级监听
            UBlackboardManager.Instance.AddTypeListener<int>((args) =>
            {
                Debug.Log($"整数类型数据变更: {args.Key} = {args.NewValue}");
            });

            // ========== 持久化数据 ==========

            // 设置持久化数据
            UBlackboardManager.Instance.SetPersistent("HighScore", 10000);
            blackboard.SetPersistentVector3("LastPosition", new Vector3(5, 2, 10));

            // 保存持久化数据
            UBlackboardManager.Instance.SaveAllPersistent();

            // ========== 调试信息 ==========

            UBlackboardManager.Instance.PrintAllInfo();
        }

        void OnDestroy()
        {
            // 清理监听器
            // 注意：实际项目中应该保存监听器引用以便正确移除
        }

        void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 200), "黑板系统示例");

            int y = 40;

            // 显示玩家信息
            int level = UBlackboardManager.Instance.Get<int>("PlayerLevel");
            float health = UBlackboardManager.Instance.Get<float>("PlayerHealth");
            string name = UBlackboardManager.Instance.Get<string>("PlayerName");

            GUI.Label(new Rect(20, y, 280, 20), $"玩家等级: {level}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"玩家生命: {health}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"玩家名字: {name}");
            y += 30;

            // 操作按钮
            if (GUI.Button(new Rect(20, y, 100, 25), "增加等级"))
            {
                UBlackboardManager.Instance.Add("PlayerLevel", 1);
            }

            if (GUI.Button(new Rect(130, y, 100, 25), "减少生命"))
            {
                UBlackboardManager.Instance.Add("PlayerHealth", -10);
            }

            y += 30;

            if (GUI.Button(new Rect(20, y, 100, 25), "保存数据"))
            {
                UBlackboardManager.Instance.SaveAllPersistent();
            }

            if (GUI.Button(new Rect(130, y, 100, 25), "加载数据"))
            {
                UBlackboardManager.Instance.LoadAllPersistent();
            }
        }
    }
}

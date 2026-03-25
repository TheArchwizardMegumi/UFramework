using System;
using System.Linq;
using UnityEngine;

namespace UFramework.Examples
{
    /// <summary>
    /// 黑板验证器使用示例
    /// </summary>
    public class ValidatorExample : MonoBehaviour
    {
        private GameObject playerObj;

        void Start()
        {
            var board = UBlackboardManager.Instance.Default;

            // ========== 1. 使用过期时间验证器 ==========

            // 数据将在 10 秒后过期
            var ttlValidator = new ExpirationValidator(TimeSpan.FromSeconds(10), "TemporaryData");
            board.Set("TemporaryToken", "abc123", validator: ttlValidator);

            // 或者使用扩展方法
            board.Set("SessionId", "xyz789", validator: this.ExpireAfter(TimeSpan.FromSeconds(5)));

            // ========== 2. 使用委托验证器 ==========

            // 创建一个 GameObject 引用，验证对象是否仍然有效
            playerObj = new GameObject("Player");
            board.Set("PlayerReference", playerObj, validator: this.Validate<GameObject>(
                (obj) => obj != null && obj.activeInHierarchy,
                () => "Player 对象已销毁或未激活"
            ));

            // 验证数值范围
            board.Set("Health", 100, validator: this.Validate<int>(
                (value) => value >= 0 && value <= 100,
                () => "生命值必须在 0-100 之间"
            ));

            // 验证字符串非空
            board.Set("UserName", "Hero", validator: this.Validate<string>(
                (str) => !string.IsNullOrEmpty(str) && str.Length >= 3,
                () => "用户名不能为空且至少3个字符"
            ));

            // ========== 3. 组合验证器 ==========

            // 同时要求对象有效且数据未过期
            var complexValidator = this.Validate<GameObject>(
                (obj) => obj != null && obj.activeInHierarchy,
                () => "对象无效"
            ).And(
                this.ExpireAfter(TimeSpan.FromMinutes(30), "TimedObject")
            );

            board.Set("CachedObject", playerObj, validator: complexValidator);

            // 或者：多个条件任意一个满足即可
            var orValidator = this.Validate<GameObject>(
                (obj) => obj != null,
                () => "对象为空"
            ).Or(
                this.Validate<GameObject>(
                    (obj) => obj != null && obj.activeInHierarchy,
                    () => "对象未激活"
                )
            );

            // ========== 4. 使用原始委托验证器 ==========

            board.Set("CustomData", new object(), validator: new DelegateValidator(
                (value) => {
                    // 自定义验证逻辑
                    return value != null;
                },
                () => "自定义验证失败"
            ));

            // ========== 5. 动态修改验证器 ==========

            // 先设置一个基本验证器
            board.Set("Score", 0, validator: this.Validate<int>(
                (value) => value >= 0,
                () => "分数不能为负数"
            ));

            // 后续可以修改验证器
            board.SetValidator("Score", this.Validate<int>(
                (value) => value >= 0 && value <= 10000,
                () => "分数必须在 0-10000 之间"
            ));

            // 或者移除验证器
            // board.RemoveValidator("Score");

            // ========== 6. 验证数据有效性 ==========

            bool isValid = board.IsValid("Health");
            Debug.Log($"Health 数据有效: {isValid}");

            string error = board.GetValidationError("Health");
            if (error != null)
            {
                Debug.LogWarning($"验证失败: {error}");
            }

            // ========== 7. 访问无效数据 ==========

            // 默认 Get 会跳过无效数据
            int health = board.Get<int>("Health", 100);
            Debug.Log($"Health: {health}");

            // 如果需要访问可能无效的数据（忽略验证）
            int healthRaw = board.GetIgnoreValidation<int>("Health", 0);

            // ========== 8. 清理无效数据 ==========

            int cleanupCount = board.CleanupInvalidData();
            Debug.Log($"清理了 {cleanupCount} 条无效数据");

            // 获取所有无效的键
            var invalidKeys = board.GetInvalidKeys();
            foreach (var key in invalidKeys)
            {
                Debug.LogWarning($"无效数据: {key}");
            }
        }

        void Update()
        {
            var board = UBlackboardManager.Instance.Default;

            // ========== 测试过期数据 ==========

            if (board.TryGet("TemporaryToken", out string token))
            {
                Debug.Log($"临时令牌有效: {token}");
            }
            else
            {
                Debug.Log("临时令牌已过期");
            }

            // ========== 测试动态数据 ==========

            if (Input.GetKeyDown(KeyCode.Space))
            {
                int currentHealth = board.Get("Health", 100);
                int newHealth = Mathf.Max(0, currentHealth - 20);
                board.Set("Health", newHealth, validator: this.Validate<int>(
                    (value) => value >= 0 && value <= 100,
                    () => "生命值必须在 0-100 之间"
                ));
                Debug.Log($"生命值减少到: {newHealth}");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                board.Set("Health", 100, validator: this.Validate<int>(
                    (value) => value >= 0 && value <= 100,
                    () => "生命值必须在 0-100 之间"
                ));
                Debug.Log("生命值恢复");
            }

            // ========== 测试对象引用 ==========

            if (Input.GetKeyDown(KeyCode.D) && playerObj != null)
            {
                Destroy(playerObj);
                playerObj = null;
                Debug.Log("销毁 Player 对象");
            }

            if (board.TryGet("PlayerReference", out GameObject player))
            {
                // 对象仍然有效
            }
            else
            {
                Debug.Log("Player 引用已失效");
            }

            // ========== 定期清理无效数据 ==========

            if (Time.frameCount % 300 == 0) // 每 5 秒（60fps）
            {
                int cleaned = board.CleanupInvalidData();
                if (cleaned > 0)
                {
                    Debug.Log($"自动清理了 {cleaned} 条无效数据");
                }
            }
        }

        void OnGUI()
        {
            var board = UBlackboardManager.Instance.Default;

            GUI.Box(new Rect(10, 220, 300, 280), "验证器示例");

            int y = 245;

            // 显示数据状态
            GUI.Label(new Rect(20, y, 280, 20), $"TemporaryToken: {(board.IsValid("TemporaryToken") ? "✓有效" : "✗无效")}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"Health: {board.Get("Health", 100)} [{(board.IsValid("Health") ? "✓" : "✗")}]");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"Score: {board.Get("Score", 0)} [{(board.IsValid("Score") ? "✓" : "✗")}]");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"PlayerReference: {(board.IsValid("PlayerReference") ? "✓有效" : "✗无效")}");
            y += 30;

            // 按钮
            if (GUI.Button(new Rect(20, y, 100, 25), "减少生命"))
            {
                int health = board.Get("Health", 100);
                board.Set("Health", health - 10, validator: this.Validate<int>(
                    (value) => value >= 0 && value <= 100,
                    () => "生命值必须在 0-100 之间"
                ));
            }

            if (GUI.Button(new Rect(130, y, 100, 25), "恢复生命"))
            {
                board.Set("Health", 100, validator: this.Validate<int>(
                    (value) => value >= 0 && value <= 100,
                    () => "生命值必须在 0-100 之间"
                ));
            }

            y += 30;

            if (GUI.Button(new Rect(20, y, 100, 25), "设置过期数据"))
            {
                board.Set("TemporaryToken", "new_token", validator: this.ExpireAfter(TimeSpan.FromSeconds(3)));
            }

            if (GUI.Button(new Rect(130, y, 100, 25), "清理无效数据"))
            {
                int count = board.CleanupInvalidData();
                Debug.Log($"清理了 {count} 条无效数据");
            }

            y += 30;

            // 显示无效数据
            var invalidKeys = board.GetInvalidKeys();
            var keys = invalidKeys.ToArray();
            GUI.Label(new Rect(20, y, 280, 20), $"无效数据 ({keys.Length}):");
            y += 25;

            for (int i = 0; i < Mathf.Min(keys.Length, 4); i++)
            {
                GUI.Label(new Rect(20, y, 280, 20), $"  - {keys[i]}: {board.GetValidationError(keys[i])}");
                y += 20;
            }
        }

        void OnDestroy()
        {
            if (playerObj != null)
            {
                Destroy(playerObj);
            }
        }
    }
}

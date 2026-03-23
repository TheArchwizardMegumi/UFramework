using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    [DefaultExecutionOrder(-1000)] // Execute early to update input states before other scripts
    public class UInputManager : Singleton<UInputManager>
    {
        private void Update()
        {
            //自动更新轴到按键的映射状态，确保输入状态正确处理
            UInput.UpdateAxisMappings();
        }
    }
}

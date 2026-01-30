using DG.Tweening;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace UFramework
{
    /// <summary>
    /// 拓展工具类，用于实现拓展的工具方法
    /// </summary>
    public static class ExtensionTools
    {
        private static Dictionary<Action, List<Tweener>> actionTweenerDict = new Dictionary<Action, List<Tweener>>();
        /// <summary>
        /// 自适应显示UI,根据UI生成位置与屏幕边界来自动调整位置
        /// </summary>
        public static void ShowSelfAdaptingUI(this Image menuImg, Vector2 position)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float imageWidth = menuImg.GetComponent<RectTransform>().sizeDelta.x;
            float imageHeight = menuImg.GetComponent<RectTransform>().sizeDelta.y;
            if (position.x + imageWidth > (screenWidth))
            {
                menuImg.GetComponent<RectTransform>().pivot = new Vector2(1, menuImg.GetComponent<RectTransform>().pivot.y);
            }
            else
            {
                menuImg.GetComponent<RectTransform>().pivot = new Vector2(0, menuImg.GetComponent<RectTransform>().pivot.y);
            }
            if (position.y - imageWidth < 0)
            {
                menuImg.GetComponent<RectTransform>().pivot = new Vector2(menuImg.GetComponent<RectTransform>().pivot.x, 0);
            }
            else
            {
                menuImg.GetComponent<RectTransform>().pivot = new Vector2(menuImg.GetComponent<RectTransform>().pivot.x, 1);

            }
            menuImg.GetComponent<RectTransform>().position = position;
        }

        /// <summary>
        /// 尝试根据key得到value，得到了的话直接返回value，没有得到直接返回null
        /// this Dictionary<Tkey,Tvalue> dict 这个字典表示我们要获取值的字典
        /// </summary>
        public static Tvalue TryGet<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> dict, Tkey key)
        {
            Tvalue value;
            dict.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// 获取鼠标停留处UI
        /// </summary>
        /// <returns></returns>
        public static List<GameObject> GetOverUI()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            if (results.Count != 0)
            {
                List<GameObject> goList = new List<GameObject>();
                foreach (var result in results)
                {
                    goList.Add(result.gameObject);
                }
                if (goList.Count == 0)
                {
                    return null;
                }
                return goList;
            }
            return null;

        }

        /// <summary>
        /// 获取除自身外的所有子物体
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<GameObject> GetChildren(GameObject parent)
        {
            List<GameObject> objects = new List<GameObject>();
            GetChildrenRecursive(parent.transform, objects);
            //objects.Add(parent); // 添加父对象
            return objects;
        }

        /// <summary>
        /// 获取自身及所有子物体
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<GameObject> GetObjectAndChildren(GameObject parent)
        {
            List<GameObject> objects = new List<GameObject>();
            GetChildrenRecursive(parent.transform, objects);
            objects.Add(parent); // 添加父对象
            return objects;
        }

        /// <summary>
        /// 递归获取所有子对象 私有
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="objects"></param>
        private static void GetChildrenRecursive(Transform parent, List<GameObject> objects)
        {
            foreach (Transform child in parent)
            {
                objects.Add(child.gameObject); // 添加子对象
                GetChildrenRecursive(child, objects); // 递归获取子子对象
            }
        }

        /// <summary>
        /// 打开Windows窗体
        /// </summary>
        /// <param name="title"></param>
        /// <param name="dialogType"></param>
        /// <param name="fileSuffixList"></param>
        /// <returns></returns>
        public static string OpenWindowsDialog(string title, WindowsDialogType dialogType, string[] fileSuffixList)
        {

            string fileName = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (WindowsDialogType.OpenFile == dialogType || WindowsDialogType.SaveFile == dialogType)
            {
                OpenFileName openFileName = new OpenFileName();
                openFileName.structSize = Marshal.SizeOf(openFileName);
                string filter = "";
                if (fileSuffixList != null && fileSuffixList.Length > 0)
                {
                    foreach (var fileSuffix in fileSuffixList)
                    {
                        filter += "*." + fileSuffix + ";";
                    }
                    filter = filter.Substring(0, filter.Length - 1);
                    openFileName.filter = "所有文件(" + filter + ")\0" + filter;
                }
                else
                {
                    openFileName.filter = "所有文件(*)\0*.*";
                }
                openFileName.file = new string(new char[256]);
                openFileName.maxFile = openFileName.file.Length;
                openFileName.fileTitle = new string(new char[64]);
                openFileName.maxFileTitle = openFileName.fileTitle.Length;
                openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');
                openFileName.title = !string.IsNullOrEmpty(title) ? title : "映数智能";
                openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
                if (dialogType == WindowsDialogType.OpenFile && WindowsDialog.GetOpenFileName(openFileName))
                {
                    fileName = openFileName.file;
                }
                else if (dialogType == WindowsDialogType.SaveFile && WindowsDialog.GetSaveFileName(openFileName))
                {
                    fileName = openFileName.file;
                }
            }
            else if (WindowsDialogType.OpenDirectory == dialogType)
            {
                OpenDialogDir ofn = new OpenDialogDir();
                ofn.pszDisplayName = new string(new char[2000]); ;     // 存放目录路径缓冲区  
                ofn.title = title;// 标题  
                                  //ofn.ulFlags = BIF_NEWDIALOGSTYLE | BIF_EDITBOX; // 新的样式,带编辑框  
                IntPtr pidlPtr = WindowsDialog.SHBrowseForFolder(ofn);

                char[] charArray = new char[2000];
                for (int i = 0; i < 2000; i++)
                    charArray[i] = '\0';

                WindowsDialog.SHGetPathFromIDList(pidlPtr, charArray);
                string fullDirPath = new String(charArray);
                fileName = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
            }
#endif
            return fileName;


        }
        public static void DelayExecute(Action action, float delayTime)
        {
            float myValue = 0;
            Tweener tweener = DOTween.To(() => myValue, x => myValue = x, 0, 0).SetDelay(delayTime).OnComplete(() => {
                action();
            });
            if (actionTweenerDict.ContainsKey(action))
            {
                actionTweenerDict[action].Add(tweener);
            }
            else
            {
                List<Tweener> tweenerList = new List<Tweener>();
                tweenerList.Add(tweener);
                actionTweenerDict.Add(action, tweenerList);
            }
        }
        public static void CancelDelayExecute(Action action)
        {

            if (actionTweenerDict.ContainsKey(action))
            {
                foreach (var tweener in actionTweenerDict[action])
                {
                    if (tweener != null && tweener.IsPlaying())
                    {
                        tweener.Kill();
                    }
                }
                actionTweenerDict.Remove(action);
            }
        }

        public static void CancelAllDelayExecute()
        {
            foreach (var tweener in actionTweenerDict)
            {
                for (int i = 0; i < tweener.Value.Count; i++)
                {
                    if (tweener.Value[i] != null && tweener.Value[i].IsPlaying())
                    {
                        tweener.Value[i].Kill();
                    }
                }

            }
            actionTweenerDict.Clear();
        }

        public static Texture2D ScreenShot(Camera camera, int width = 400, int height = 225)
        {
            Rect rect = new Rect(0, 0, width, height);
            //对指定相机进行 RenderTexture
            RenderTexture renTex = new RenderTexture((int)rect.width, (int)rect.height, 0);
            camera.targetTexture = renTex;
            camera.Render();
            RenderTexture.active = renTex;
            //读取像素
            Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rect.width, rect.height), 0, 0);
            tex.Apply();
            //读取目标相机像素结束，渲染恢复原先的方式
            camera.targetTexture = null;
            RenderTexture.active = null;
            MonoBehaviour.Destroy(renTex);
            //保存读取的结果
            //string path = Application.streamingAssetsPath + "/" + name + ".png";
            //System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            return tex;
        }

        /// <summary>
        /// 计算最近的一个2的幂次方值
        /// </summary>
        /// <param name="number"></param>
        /// <param name="includeMax">是否包含最大分辨率</param>
        /// <returns></returns>
        public static int GetNearestPowerOfTwo(int number, bool includeMax = false)
        {
            if (number == 0)
                return 0;

            int power = (int)Math.Log(number, 2);
            int result = (int)Math.Pow(2, power);

            // Check if result is greater and one of the dimensions is odd
            if (includeMax)
            {
                if (result < number && (number - result) > (result - number) / 2)
                {
                    result *= 2;
                }
            }


            return result;
        }

    }
}
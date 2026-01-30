using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace UFramework
{ 
    /// <summary>
    /// 资源加载管理类，用于文件、图片、预制体、UI、模型、材质等资源的动态加载，当前主要加载Resources文件夹下资源
    /// </summary>
    public class LoadManager : Singleton<LoadManager>
    {        
        /// <summary>
        /// 初始化方法，用于添加资源加载事件监听
        /// </summary>
        protected override void Awake()
        {
            base.Awake();            
        }

        /// <summary>
        /// 资源加载方法，通过资源类型和名称来加载对应资源
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <param name="objectName">资源名称</param>
        /// <returns></returns>
        public object LoadResource(ResourceType resourceType, string objectName)
        {

            object o = null;
            switch (resourceType)
            {
                case ResourceType.Model:
                    o = Resources.Load<GameObject>("Models/" + objectName);
                    if (o != null)
                        o = Instantiate((GameObject)o);
                    break;
                case ResourceType.UIPanel:
                    o = Resources.Load<GameObject>("UIPanels/" + objectName);
                    if (o != null)
                        o = Instantiate((GameObject)o);
                    break;
                case ResourceType.UIItem:
                    o = Resources.Load<GameObject>("UIItems/" + objectName);
                    if (o != null)
                        o = Instantiate((GameObject)o);
                    break;
                case ResourceType.File:
                    o = Resources.Load<TextAsset>("Files/" + objectName);
                    break;
                case ResourceType.Image:
                    o = Resources.Load<Texture2D>("Images/" + objectName);
                    break;
                case ResourceType.Material:
                    o = Resources.Load<Material>("Materials/" + objectName);
                    break;
                case ResourceType.Font:
                    o = Resources.Load<Font>("Fonts/" + objectName);
                    break;
                case ResourceType.TMP_FontAsset:
                    o = Resources.Load<TMPro.TMP_FontAsset>("Fonts/" + objectName);
                    break;
                case ResourceType.Sprite:
                    o = Resources.Load<Sprite>("Images/" + objectName);
                    break;
                default:
                    break;
            }
            return o;
        }

        public void LoadStreamingAsset(string assetPath, Action<string, byte[]> callBack)
        {
            StartCoroutine(WaitLoadStreamingAssets(assetPath, callBack));
        }
        private IEnumerator WaitLoadStreamingAssets(string assetPath, Action<string, byte[]> callBack)
        {
            string url = Application.streamingAssetsPath + "/" + assetPath;
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (!request.isNetworkError)
            {
                callBack(assetPath, request.downloadHandler.data);
            }
            else
            {
                print("下载资源报错：" + request.error);
            }
        }

        /// <summary>
        /// 销毁方法， 用于移除资源加载事件的监听
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
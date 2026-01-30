using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text;

namespace UFramework
{
    public enum FileImportType
    {
        Texture,//图片
        Audio,//音频
        Video,//视频
        MapData
    }
    public class FileImportManager : Singleton<FileImportManager>
    {
        protected override void Awake()
        {
            base.Awake();
            
        }

        private void Start()
        {

        }

        public void ImportFile_OpenWindow(FileImportType fileType, object param, Action importStartAction, Action<bool, object> importFinishedAction = null)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            string filePath = null;
            if (fileType == FileImportType.Texture)
            {
                List<string> fileTypeList = new List<string>();
                fileTypeList.Add("png");
                fileTypeList.Add("jpg");
                fileTypeList.Add("jpeg");
                filePath = ExtensionTools.OpenWindowsDialog("选择文件", WindowsDialogType.OpenFile, fileTypeList.ToArray());
                byte[] bytes = File.ReadAllBytes(filePath);
                importFinishedAction?.Invoke(true, bytes);
            }
            else if (fileType == FileImportType.Audio)
            {
                List<string> fileTypeList = new List<string>();
                fileTypeList.Add("mp3");
                filePath = ExtensionTools.OpenWindowsDialog("选择文件", WindowsDialogType.OpenFile, fileTypeList.ToArray());
                byte[] bytes = File.ReadAllBytes(filePath);
                importFinishedAction?.Invoke(true, bytes);
            }
            else if (fileType == FileImportType.Video)
            {
                List<string> fileTypeList = new List<string>();
                fileTypeList.Add("mp4");
                filePath = ExtensionTools.OpenWindowsDialog("选择文件", WindowsDialogType.OpenFile, fileTypeList.ToArray());
                byte[] bytes = File.ReadAllBytes(filePath);
                importFinishedAction?.Invoke(true, bytes);
            }
            else if (fileType == FileImportType.MapData)
            {
                List<string> fileTypeList = new List<string>();
                fileTypeList.Add("map");
                filePath = ExtensionTools.OpenWindowsDialog("选择文件", WindowsDialogType.OpenFile, fileTypeList.ToArray());
                byte[] bytes = File.ReadAllBytes(filePath);
                importFinishedAction?.Invoke(true, bytes);
            }
#endif
        }

        public void ImportFile(FileImportType fileType, object param, Action importStartAction,string filePath, Action<bool, object> importFinishedAction = null)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            byte[] bytes = File.ReadAllBytes(filePath);
            if (fileType == FileImportType.Texture)
            {
                importFinishedAction?.Invoke(true, bytes);
            }
            else if (fileType == FileImportType.Audio)
            {
                importFinishedAction?.Invoke(true, bytes);
            }
            else if (fileType == FileImportType.Video)
            {
                importFinishedAction?.Invoke(true, bytes);
            }
#endif
        }

    }

}

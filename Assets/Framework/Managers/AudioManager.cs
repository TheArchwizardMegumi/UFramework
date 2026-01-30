using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UFramework
{
    public class AudioManager : Singleton<AudioManager>
    {
        Dictionary<string, AudioSource> audioSourceDic = new Dictionary<string, AudioSource>();

        public GameObject CreateAudio(AudioClip audioClip,string ID)
        {
            if(audioSourceDic.ContainsKey(ID))
            {
                return audioSourceDic[ID].gameObject;
            }
            else
            {
                GameObject audioObj = new GameObject(ID);
                AudioSource audioSource = audioObj.AddComponent<AudioSource>();
                audioSourceDic.Add(ID, audioSource);
                //audioSourceDic_Obj.Add(audioObj, audioSource);
                return audioObj;
            }
        }

        public GameObject GetAudio(AudioClip audioClip, string ID)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                return audioSourceDic[ID].gameObject;
            }else
            {
                return null;
            }
        }

        public void SetClip(string ID,AudioClip audioClip)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic[ID].clip = audioClip;
            }
        }

        public void Play(string ID)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic[ID].Play();
            }
        }

        public void Pause(string ID)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic[ID].Pause();
            }
        }

        public void Stop(string ID)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic[ID].Stop();
            }
        }

        public void SetVolume(string ID,float volume)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic[ID].volume = volume;
            }
        }

        public void SetLoop(string ID, bool loop)
        {
            if (audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic[ID].loop = loop;
            }
        }

        public List<string> GetAllID()
        {
            List<string> Ids = new List<string>();

            foreach (var item in audioSourceDic)
            {
                Ids.Add(item.Key);
            }

            return Ids;
        }

        public string GetID(AudioSource audioSource)
        {
            string ID = null;
            foreach (var item in audioSourceDic)
            {
                if(item.Value.Equals(audioSource))
                {
                    ID = item.Key;
                }
            }
            return ID;
        }

        public void RemoveSource(string ID)
        {
            if(audioSourceDic.ContainsKey(ID))
            {
                audioSourceDic.Remove(ID);
            }
        }
    }

}

using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SaveWave;
namespace KaKaoVoiceAPI
{
    [System.Serializable]
    public struct nBestMember
    {
        [SerializeField] public string value;
        [SerializeField] public int score;
    }
    [System.Serializable]
    public struct voiceProfileMember
    {
        [SerializeField] public bool registered;
        [SerializeField] public bool authenticated;
    }
    [System.Serializable]
    public struct sttJson
    {
        [SerializeField] public string type;
        [SerializeField] public string value;
        [SerializeField] public List<nBestMember> nBest;
        [SerializeField] public voiceProfileMember voiceProfile;
        [SerializeField] public string durationMS;
        [SerializeField] public string qmarkScore;
        [SerializeField] public string gender;
        public void print()
        {
            Debug.Log(type);
            Debug.Log(value);
            Debug.Log(nBest);
            Debug.Log(voiceProfile);
            Debug.Log(durationMS);
            Debug.Log(qmarkScore);
            Debug.Log(gender);
        }
    }

    public class kakaoSTT
    {
        private string url;
        private string rest_api_key;
        private HttpWebRequest request;
        private string FiePath;
        private string responseText;
        public static kakaoSTT MakeSTT(string rest_api_key, AudioClip clip = null)=>new kakaoSTT(rest_api_key, clip);
        private void init(string rest_api_key, AudioClip clip = null)
        {
            url = "https://kakaoi-newtone-openapi.kakao.com/v1/recognize"; // HOST 및 URL
            this.rest_api_key = rest_api_key;// 내 어플리케이션 => 어플선택 => 기본정보의 앱 키 > REST Key 값 부여
            if (clip != null)
            {
                FiePath = SavWav.Save(clip);
                Post();
                Request();
            }
            else FiePath = "";
        }

        private kakaoSTT(string rest_api_key)
        {
            init(rest_api_key, null);
        }
        private kakaoSTT(string rest_api_key, AudioSource source)
        {
            init(rest_api_key, source.clip);
        }
        private kakaoSTT(string rest_api_key, AudioClip clip)
        {
            init(rest_api_key, clip);
        }
        private void Post()
        {
            Debug.Assert(FiePath.Length > 0,"Path is Empty");
            request = (HttpWebRequest)WebRequest.Create(url); // 해당 URL로 네트웍을 만든
            request.Headers.Add("Authorization", rest_api_key); // 헤더에 옵션값을 추가한다.
            request.ContentType = "application/octet-stream";// 콘텐츠타입을 명시한다
            request.Method = "POST"; // get 으로 보낼지 post로 보낼지 명시한다.
                                     //FileStream file = new FileStream(FiePath, FileMode.Open); // 보낼 파일을 오픈한다.
            FileStream file = new FileStream(FiePath, FileMode.Open); // 보낼 파일을 오픈한다.
            byte[] byteDataParams = new byte[file.Length]; // 보낼파일의 크기만큼 바이트배열을 만든다.

            file.Read(byteDataParams, 0, byteDataParams.Length); // 파일을 읽어서 바이트배열에 데이터를 넣는다.

            request.ContentLength = byteDataParams.Length; // 네트웍으로 보낼 데이터 길이를 명시한다.

            using (Stream reqStream = request.GetRequestStream()) // 네트웍을 열어서 데이터를 보낸다.
            {
                reqStream.Write(byteDataParams, 0, byteDataParams.Length); // 데이터 쓰기
            }
        }
        private void Request()
        {
            responseText = string.Empty;
            using (WebResponse response = request.GetResponse()) // 보낸데이터를 기반으로 받는다
            {
                Stream stream = response.GetResponseStream(); // 받은 데이터를 스트림으로 쓴다
                using (StreamReader sr = new StreamReader(stream)) // 스트림을 읽기 위해 리더를 오픈한다.
                {
                    responseText = sr.ReadToEnd(); // 스트림의 내용을 읽어서 문자열로 반환해준다.
                }
            }
        }

        static public sttJson getJSON(string text)
        {
            var textList = text.Split(new string[] { "\r\n" }, StringSplitOptions.None).
                Where(v => v.Contains("finalResult") || v.Contains("errorCalled"));
            var finalResult = string.Join(", ", textList.ToArray());
            return JsonUtility.FromJson<sttJson>(finalResult);
        }
        public kakaoSTT Update(AudioSource source = null) => this.Update(source.clip);
        public kakaoSTT Update(AudioClip clip = null)
        {
            if (clip != null) FiePath = SavWav.Save(clip);
            this.Post();
            this.Request();
            return this;
        }
        public string getText() => responseText;
        public sttJson getJSON() => kakaoSTT.getJSON(this.responseText);
    }
}


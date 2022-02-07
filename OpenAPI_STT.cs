using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Text;
using SaveWave;
using openAPI.JsonSturcture;
namespace openAPI
{
    namespace JsonSturcture
    {
        [System.Serializable]
        public struct Field
        {
            [SerializeField] public string language_code;
            [SerializeField] public string audio;
        }
        [System.Serializable]
        public struct OpenSTT_POST_JSON
        {

            [SerializeField] public string access_key;
            [SerializeField] public Field argument;
            public OpenSTT_POST_JSON(string key, string language_code, string audio)
            {
                access_key = key;
                argument.language_code = language_code;
                argument.audio = audio;
            }
            static public string ToJson(OpenSTT_POST_JSON json) => JsonUtility.ToJson(json);
            static public string ToJson(string key, string language_code, string audio) =>
                ToJson(new OpenSTT_POST_JSON(key, language_code, audio));
            public string ToJson() => ToJson(this);
        }

        [System.Serializable]
        public struct returnObj
        {
            [SerializeField] public string recognized;
        }
        [System.Serializable]
        public struct OpenSTT_GET_JSON
        {
            [SerializeField] public int result;
            [SerializeField] public string reason;
            [SerializeField] public returnObj return_object;
            static public OpenSTT_GET_JSON FromJson(string responseText) => JsonUtility.FromJson<OpenSTT_GET_JSON>(responseText);
        }
    }
    // 지원 언어 코드
    public enum LangCode { none = 0, korean, english, japanese, chinese, spanish, french, german, russian, vietnam, arabic, thailand };
    public class OpenAPI_STT
    {
        private string url;// 통신 URL
        private HttpWebRequest request;
        private string FiePath; // 음성 파일 저장된 경로
        private string audioBase64; // 음성파일 base64로 저장
        private OpenSTT_POST_JSON post_json;// POST 문자 JSON
        private string responseText;// GET 문자 그대로 
        private OpenSTT_GET_JSON get_json; // GET 문자 JSON
        static public OpenSTT_GET_JSON getJSON(string text) => JsonUtility.FromJson<OpenSTT_GET_JSON>(text);
        public static OpenAPI_STT initSTT(string rest_api_key, LangCode code = LangCode.korean, AudioClip clip = null) =>
            new OpenAPI_STT(rest_api_key, Enum.GetName(typeof(LangCode), code), clip);
        private OpenAPI_STT(string rest_api_key, string code = "", AudioClip clip = null)
        {
            url = "http://aiopen.etri.re.kr:8000/WiseASR/Recognition"; // HOST 및 URL
            post_json.access_key = rest_api_key;
            post_json.argument.language_code = code;
            if (clip != null)
            {
                FiePath = SavWav.Save(clip);
                audioBase64 = convertBase64(FiePath);
                post_json.argument.audio = audioBase64;
                _ = Post().Request();
            }
            else FiePath = "";
        }
        private string convertBase64(string FiePath)//파일 경로를 받는다.
        {
            if (FiePath.Length == 0) return null;
            FileStream file = new FileStream(FiePath, FileMode.Open); // 보낼 파일을 오픈한다.
            byte[] byteAudio = new byte[file.Length]; // 보낼파일의 크기만큼 바이트배열을 만든다.
            file.Read(byteAudio, 0, Convert.ToInt32(byteAudio.Length)); // 파일을 읽어서 바이트배열에 데이터를 넣는다.
            return Convert.ToBase64String(byteAudio);
        }
        private OpenAPI_STT Post()
        {
            Debug.Assert(FiePath.Length > 0, "Path is Empty");
            request = (HttpWebRequest)WebRequest.Create(url); // 해당 URL로 네트웍을 만든
            request.Headers.Add("Authorization", post_json.access_key); // 헤더에 옵션값을 추가한다.
            request.ContentType = "application/json; charset=UTF-8";// 콘텐츠타입을 명시한다 
            request.Method = "POST"; // get 으로 보낼지 post로 보낼지 명시한다.
            byte[] byteJson = Encoding.UTF8.GetBytes(OpenSTT_POST_JSON.ToJson(post_json)); //Json 파일 통신 규격에 맞추기
            using Stream reqStream = request.GetRequestStream(); // 네트웍을 열어서 데이터를 보낸다.
            reqStream.Write(byteJson, 0, byteJson.Length);
            return this;
        }
        private OpenAPI_STT Request()
        {
            responseText = string.Empty;
            using (WebResponse response = request.GetResponse()) // 보낸데이터를 기반으로 받는다
            {
                Stream stream = response.GetResponseStream(); // 받은 데이터를 스트림으로 쓴다
                using (StreamReader sr = new StreamReader(stream)) // 스트림을 읽기 위해 리더를 오픈한다.
                {
                    responseText = sr.ReadToEnd(); // 스트림의 내용을 읽어서 문자열로 반환해준다.
                    get_json = OpenSTT_GET_JSON.FromJson(responseText);
                }
            }
            return this;
        }
        public OpenAPI_STT Update(AudioClip clip = null, LangCode code = LangCode.none)//음성 내용, 언어 변경
        {
            if (code == LangCode.none && clip == null) return this;
            else
            {
                if (code != LangCode.none) post_json.argument.language_code = Enum.GetName(typeof(LangCode), code);
                if (clip != null)
                {
                    FiePath = SavWav.Save(clip);
                    post_json.argument.audio = convertBase64(FiePath);
                }
                _ = Post().Request();
            }
            return this;
        }
        public string getText() => responseText;
        public OpenSTT_GET_JSON getJSON() => get_json;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
namespace RecordClipAPI
{
    class voiceValue
    {
        private float rmsValue;
        public int ResultValue;
        //목소리 가중치
        public int Modulate { get; }
        //목소리 크기 얼마나 할까
        public int CutValue { get; }
        public voiceValue(int modulate = 1000, int cutValue = 5)
        {// modulate: 마이크 증폭 조절 / cutValue: 음성 인식 임계값
            rmsValue = 0; ResultValue = 0;
            Modulate = modulate;
            CutValue = cutValue;
        }
        private float getSum(float[] samples) => samples.Select(v => v * v).Aggregate((acc, x) => acc + x);
        public void SetrmsValue(float[] samples)
        {
            float temp = Mathf.Sqrt(getSum(samples) / samples.Length);
            rmsValue = Mathf.Clamp(temp * Modulate, 0, 100);
        }
        public void SetResultValue(float[] samples)
        {
            SetrmsValue(samples);
            ResultValue = Mathf.RoundToInt(rmsValue);
        }
        public bool isRecord() => ResultValue >= CutValue;
        public bool isRecord(float[] samples)
        {
            SetResultValue(samples);
            return isRecord();
        }
    }
    public class STT_Record : MonoBehaviour
    {
        private AudioClip recordClip;
        protected AudioClip newClip;
        private int sampleRate = 16000;
        private float[] samples;
        private List<float> samplesList;
        private float StartTime, LastRecordTime;
        private float Duration, cutRecordDuration;
        private voiceValue voiceValue;
        private bool isRecord;

        protected void GetRecord()
        {
            try
            {
                if (Microphone.devices.Length <= 0) throw new Exception("No Mic Device");
                //최근 녹음 시간,녹음 시작 시간
                StartTime = LastRecordTime = Time.time;
                // Duration: 녹음 경과 시간, cutRecordTime: 녹음이 안된 경과 시간
                Duration = cutRecordDuration = 0;
                recordClip = Microphone.Start(Microphone.devices[0].ToString(), true, 1, sampleRate); //음성 녹음기
                samples = new float[sampleRate]; // 1초마다 받는 음성 배열
                samplesList = new List<float>(); // 음성 배열들 모음 리스트
                voiceValue = new voiceValue(); // 음성 배열 후 처리
                // 동시 진행
                StartCoroutine("Record"); // 녹음 파일 만드는 함수
                StartCoroutine("TimeChecker"); // 녹음 경과 시간, 녹음이 안된 경과 시간 측정기
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        private IEnumerator Record()
        {
            while (Duration < 10 && cutRecordDuration < 3)
            {
                recordClip.GetData(samples, 0);
                samplesList.AddRange(samples); // 1초 마다의 음성 추가
                yield return new WaitForSeconds(1f); //1초마다 반복
            }
            //조건이 끝난 후 오디오 파일 만들기
            Microphone.End(Microphone.devices[0].ToString());
            float[] cutSamples = samplesList.ToArray();
            newClip = AudioClip.Create("SaveClip", cutSamples.Length, 1, sampleRate, false);
            newClip.SetData(cutSamples, 0);
            // 이후 처리
            addEvent();
            StopCoroutine(nameof(Record));
        }
        public virtual void addEvent()
        {
            Debug.LogError("It must be Override!!");
        }
        protected AudioClip getAudioClip() => newClip;
        private IEnumerator TimeChecker()
        {
            while (Duration < 10 && cutRecordDuration < 3)//실행 조건
            {
                Duration = Time.time - StartTime; // 녹음 경과 시간
                _ = recordClip.GetData(samples, 0); // 녹음 배열값 얻기
                //녹음 임계값을 통해 얻는 (녹음이 안된 경과시간, 최근 녹음 시간)
                (cutRecordDuration, LastRecordTime) =
                    voiceValue.isRecord(samples) ? (0, Time.time) : (Time.time - LastRecordTime, LastRecordTime);
                yield return null;//프레임 마다 반복
            }
            StopCoroutine(nameof(TimeChecker));
        }
    }
}

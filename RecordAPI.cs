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
        //��Ҹ� ����ġ
        public int Modulate { get; }
        //��Ҹ� ũ�� �󸶳� �ұ�
        public int CutValue { get; }
        public voiceValue(int modulate = 1000, int cutValue = 5)
        {// modulate: ����ũ ���� ���� / cutValue: ���� �ν� �Ӱ谪
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
                //�ֱ� ���� �ð�,���� ���� �ð�
                StartTime = LastRecordTime = Time.time;
                // Duration: ���� ��� �ð�, cutRecordTime: ������ �ȵ� ��� �ð�
                Duration = cutRecordDuration = 0;
                recordClip = Microphone.Start(Microphone.devices[0].ToString(), true, 1, sampleRate); //���� ������
                samples = new float[sampleRate]; // 1�ʸ��� �޴� ���� �迭
                samplesList = new List<float>(); // ���� �迭�� ���� ����Ʈ
                voiceValue = new voiceValue(); // ���� �迭 �� ó��
                // ���� ����
                StartCoroutine("Record"); // ���� ���� ����� �Լ�
                StartCoroutine("TimeChecker"); // ���� ��� �ð�, ������ �ȵ� ��� �ð� ������
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
                samplesList.AddRange(samples); // 1�� ������ ���� �߰�
                yield return new WaitForSeconds(1f); //1�ʸ��� �ݺ�
            }
            //������ ���� �� ����� ���� �����
            Microphone.End(Microphone.devices[0].ToString());
            float[] cutSamples = samplesList.ToArray();
            newClip = AudioClip.Create("SaveClip", cutSamples.Length, 1, sampleRate, false);
            newClip.SetData(cutSamples, 0);
            // ���� ó��
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
            while (Duration < 10 && cutRecordDuration < 3)//���� ����
            {
                Duration = Time.time - StartTime; // ���� ��� �ð�
                _ = recordClip.GetData(samples, 0); // ���� �迭�� ���
                //���� �Ӱ谪�� ���� ��� (������ �ȵ� ����ð�, �ֱ� ���� �ð�)
                (cutRecordDuration, LastRecordTime) =
                    voiceValue.isRecord(samples) ? (0, Time.time) : (Time.time - LastRecordTime, LastRecordTime);
                yield return null;//������ ���� �ݺ�
            }
            StopCoroutine(nameof(TimeChecker));
        }
    }
}

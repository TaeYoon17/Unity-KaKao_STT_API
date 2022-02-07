using UnityEngine;
using RecordClipAPI;
using openAPI;
public class STT_Event : STT_Record
{
    public string OpenApi_Key;
    public LangCode code;
    private OpenAPI_STT sttAPI;
    public void onEvent() => GetRecord();
    new void Start()
    {
        sttAPI=OpenAPI_STT.initSTT(OpenApi_Key);
    }
    //여기에 추가할 이벤트 실행 입력
    public override void addEvent()
    {
        _ = sttAPI.Update(getAudioClip());
        Debug.Log(sttAPI.getText());
    }

}
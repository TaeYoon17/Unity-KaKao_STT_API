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
    //���⿡ �߰��� �̺�Ʈ ���� �Է�
    public override void addEvent()
    {
        _ = sttAPI.Update(getAudioClip());
        Debug.Log(sttAPI.getText());
    }

}
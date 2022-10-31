# Unity STT_API
 카카오 음성인식 API와 유니티 연동
 ETRI 음성인식 API와 유니티 연동
## 주의사항
내부 구현에 C# Nuget 매니저를 사용해 모바일에 적합하지 않습니다.
### 코드 설명 링크
https://sage-crowley-0ac.notion.site/REST-API-526dd2ade47f420bb1fa761debb43802

 참고 코드 링크
 1. wav 바꾸는 코드
  https://gist.github.com/drawcode/10341911
 2. C# 카카오 음성 API 통신 코드
  https://simonwithwoogi.github.io/posts/kakaovoice/
 사용법:
 0. 모든 파일 임포트
 1. STT_Event 파일을 사용한다.
 2. 이벤트를 트리거 할 상황에 STT_Event 클래스에 onEvent() 호출
 3. 이벤트 내부 로직은 STT_Event 클래스에 addEvent()에 작성
 \n
 ---내부 구현---
 onEvent(){
 ... 비동기를 위하 이것 저것 ...
 addEvent()
 ...
 }


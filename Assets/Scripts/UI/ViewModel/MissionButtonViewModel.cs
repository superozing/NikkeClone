using System;
using UI;
using UnityEngine;

public class MissionButtonViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;
    public event Action OnRequestMissionPopup;

    /// <summary>
    /// View에 표시될 현재 미션 설명 텍스트입니다.
    /// </summary>
    public string MissionDesc { get; private set; } = "미션 UI 설명이에요..";

    public MissionButtonViewModel()
    {
        // 데이터매니저에서 미션 데이터 가져올 예정..
        // 근데 데이터매니저에서 가져오기 전에 처리해야 할 정보가 있을 것 같아요.
        // 다음 날짜가 된다면 미션을 초기화 시켜야 한다거나.. 그런 로직이 필요하니까
        // 이런 것을 담당할 수 있는 유틸 함수를 파야 하나?
        // MissionDesc = "가져온 설명 넣어야 한다";
    }

    public void OnMissionButtonClicked()
    {
        Debug.Log("미션 버튼 클릭");

        // "팝업 만들어라"
        OnRequestMissionPopup?.Invoke();
    }

    public void Dispose()
    {
        // 데이터 이벤트 바인딩 제거 코드
    }
}

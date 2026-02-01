using System;

/// <summary>
/// 스테이지 정보 팝업 등에서 사용하는 표시 전용 니케 아이콘 ViewModel입니다.
/// 클릭 시 스쿼드 편집 요청, 롱프레스 시 상세 정보 요청을 보냅니다.
/// 드래그는 불가능합니다.
/// </summary>
public class DisplayNikkeIconViewModel : NikkeIconViewModel
{
    /// <summary>
    /// 스쿼드 편집 요청 이벤트입니다.
    /// </summary>
    public event Action OnEditRequest;

    /// <summary>
    /// 니케 상세 정보 요청 이벤트입니다.
    /// </summary>
    public event Action OnDetailRequest;

    public override bool IsDraggable => false;

    public DisplayNikkeIconViewModel()
    {
    }

    public override void OnClick()
    {
        // 빈 슬롯이라도 편집 요청은 가능해야 함 (빈 슬롯 클릭 -> 스쿼드 편집)
        OnEditRequest?.Invoke();
    }

    public override void OnLongPress()
    {
        // 빈 슬롯이면 상세 정보 없음
        if (IsSlotEmpty || NikkeId == -1) return;

        OnDetailRequest?.Invoke();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        OnEditRequest = null;
        OnDetailRequest = null;
    }
}

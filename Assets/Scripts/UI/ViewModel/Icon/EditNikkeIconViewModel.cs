using System;

/// <summary>
/// 스쿼드 편집 팝업 등에서 사용하는 편집용 니케 아이콘 ViewModel입니다.
/// 클릭 시 슬롯 해제(Remove), 롱프레스 시 상세 정보, 드래그&드롭 시 교체를 요청합니다.
/// 드래그가 가능합니다.
/// </summary>
public class EditNikkeIconViewModel : NikkeIconViewModel
{
    /// <summary>
    /// 슬롯 비우기 요청 이벤트입니다.
    /// </summary>
    public event Action OnClearRequest;

    /// <summary>
    /// 니케 상세 정보 요청 이벤트입니다.
    /// </summary>
    public event Action OnDetailRequest;

    /// <summary>
    /// 스왑 요청 이벤트입니다. 인자는 드래그해온 원래 슬롯의 인덱스입니다.
    /// </summary>
    public event Action<int> OnSwapRequest;

    public override bool IsDraggable => true;

    public EditNikkeIconViewModel()
    {
    }

    public override void OnClick()
    {
        // 클릭 시 해당 슬롯의 니케 해제
        if (!IsSlotEmpty)
        {
            OnClearRequest?.Invoke();
        }
    }

    public override void OnLongPress()
    {
        // 롱프레스 시 상세 정보
        if (!IsSlotEmpty)
        {
            OnDetailRequest?.Invoke();
        }
    }

    public override void OnDrop(int fromSlotIndex)
    {
        // 드롭 시 스왑 요청
        OnSwapRequest?.Invoke(fromSlotIndex);
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        OnClearRequest = null;
        OnDetailRequest = null;
        OnSwapRequest = null;
    }
}

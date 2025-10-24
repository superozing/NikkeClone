using System;

namespace UI
{
    public interface IIconViewModel : IViewModel
    {
        /// <summary>
        /// 메인 아이콘 스프라이트의 Addressable 주소입니다.
        /// </summary>
        string MainIconAddress { get; }

        /// <summary>
        /// 아이콘의 희귀도(Rarity) 등급을 표시하는 프레임 스프라이트의 Addressable 주소입니다.
        /// (예: "UI/Frames/SSR", "UI/Frames/Common")
        /// 표시할 프레임이 없으면 null 또는 string.Empty를 반환해야 합니다.
        /// </summary>
        string RarityFrameAddress { get; }

        /// <summary>
        /// 아이콘 하단에 표시될 수량 텍스트입니다.
        /// (예: "x99", "Lv.10", "5/10")
        /// 수량을 표시하지 않으려면 null 또는 string.Empty를 반환해야 합니다.
        /// </summary>
        string QuantityText { get; }
    }
}
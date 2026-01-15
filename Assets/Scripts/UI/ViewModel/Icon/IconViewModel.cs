using System;
using UnityEngine;

namespace UI
{
    public abstract class IconViewModel : ViewModelBase
    {
        /// <summary>
        /// 메인 아이콘 스프라이트 객체입니다.
        /// </summary>
        public abstract ReactiveProperty<Sprite> MainIconSprite { get; }

        /// <summary>
        /// 희귀도 프레임 스프라이트 객체입니다.
        /// </summary>
        public abstract ReactiveProperty<Sprite> RarityFrameSprite { get; }

        /// <summary>
        /// 아이콘 하단에 표시될 수량 텍스트입니다.
        /// (예: "x99", "Lv.10", "5/10")
        /// 수량을 표시하지 않으려면 null 또는 string.Empty를 반환해야 합니다.
        /// </summary>
        public abstract ReactiveProperty<string> QuantityText { get; }

        /// <summary>
        /// UI_Icon이 클릭되었을 때 호출될 메서드입니다.
        /// </summary>
        public abstract void OnClickButton();
    }
}
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UI_View : MonoBehaviour
    {
        /// <summary>
        /// View와 상호작용할 ViewModel
        /// </summary>
        public IViewModel ViewModel { get; private set; }

        /// <summary>
        /// UI 연출을 제어하기 위한 CanvasGroup
        /// </summary>
        protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        /// <summary>
        /// 이 View와 상호작용할 ViewModel을 설정(주입)하고 데이터 바인딩을 시작합니다.
        /// </summary>
        /// <param name="viewModel">주입할 ViewModel입니다.</param>
        public virtual void SetViewModel(IViewModel viewModel)
        {
            // 기존 ViewModel이 있다면 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
            if (ViewModel != null)
                ViewModel.OnStateChanged -= OnStateChanged;

            ViewModel = viewModel;

            // 새로운 ViewModel의 상태 변경 이벤트를 구독합니다.
            if (ViewModel != null)
                ViewModel.OnStateChanged += OnStateChanged;

            // ViewModel이 설정된 직후, 초기 데이터를 UI에 반영하기 위해 OnStateChanged를 호출합니다.
            OnStateChanged();
        }

        /// <summary>
        /// ViewModel의 상태가 변경되었을 때 호출되는 메서드입니다.
        /// 파생 클래스는 이 메서드를 재정의하여 UI 컴포넌트를 갱신해야 합니다.
        /// </summary>
        protected abstract void OnStateChanged();

        /// <summary>
        /// 오브젝트 파괴 시 이벤트 구독을 확실히 해제합니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (ViewModel != null)
                ViewModel.OnStateChanged -= OnStateChanged;
        }
    }
}
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 뷰모델에 바인딩 된 액션들
        /// </summary>
        private readonly List<Action> _disposables = new List<Action>();

        protected virtual void Awake()
        {
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        /// <summary>
        /// ReactiveProperty를 구독하고 콜백을 실행합니다.
        /// 등록된 콜백은 자동으로 관리됩니다.
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="property">구독할 ReactiveProperty</param>
        /// <param name="callback"></param>
        protected void Bind<T>(ReactiveProperty<T> property, Action<T> callback)
        {
            if (property == null || callback == null)
                return;

            // 1. 현재 값을 기준으로 즉시 콜백을 실행합니다. (View 갱신 보장)
            callback(property.Value);

            // 2. 값이 변경될 때 실행되도록 이벤트에 등록합니다.
            property.OnValueChanged += callback;

            // 3. 나중에 해제하기 위해 '해제하는 행동' 자체를 리스트에 저장합니다.
            _disposables.Add(() => property.OnValueChanged -= callback);
        }

        /// <summary>
        /// 저장된 모든 구독을 해제하고 리스트를 비웁니다.
        /// </summary>
        protected void UnbindAll()
        {
            foreach (var disposeAction in _disposables)
                disposeAction?.Invoke();

            _disposables.Clear();
        }

        /// <summary>
        /// 이 View와 상호작용할 ViewModel을 설정(주입)하고 데이터 바인딩을 시작합니다.
        /// </summary>
        /// <param name="viewModel">주입할 ViewModel입니다.</param>
        public virtual void SetViewModel(IViewModel viewModel)
        {
            // 1. 기존 바인딩(Bind 함수로 등록된 것들) 해제
            UnbindAll();

            // 2. 기존 ViewModel 연결 해제 및 참조 감소
            if (ViewModel != null)
            {
                ViewModel.OnStateChanged -= OnStateChanged;
                (ViewModel as ViewModelBase)?.Release();
            }

            ViewModel = viewModel;

            // 3. 새 ViewModel 연결 및 참조 증가
            if (ViewModel != null)
            {
                ViewModel.OnStateChanged += OnStateChanged;
                (ViewModel as ViewModelBase)?.AddRef();

                // ViewModel이 설정된 직후, 초기 데이터를 UI에 반영하기 위해 OnStateChanged를 호출합니다.
                OnStateChanged();
            }
        }

        /// <summary>
        /// ViewModel의 상태가 변경되었을 때 호출되는 메서드입니다.
        /// 파생 클래스는 이 메서드를 재정의하여 UI 컴포넌트를 갱신해야 합니다.
        /// 이제 제거해도 될 함수이긴 한데. 좀 더 고민해보는 것이 좋겠어요.
        /// 전체 초기화를 담당했었지만 이제 필요가 사라졌다.
        /// 이 것을 제거하고 추상 클래스로 만드려면 뭘 골라야 할 지도 생각해보아야 해요.
        /// </summary>
        protected abstract void OnStateChanged();

        /// <summary>
        /// 오브젝트 파괴 시 이벤트 구독을 확실히 해제합니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Bind 함수로 등록된 구독 해제
            UnbindAll();

            // OnStateChanged 구독 해제
            if (ViewModel != null)
                ViewModel.OnStateChanged -= OnStateChanged;
        }
    }
}